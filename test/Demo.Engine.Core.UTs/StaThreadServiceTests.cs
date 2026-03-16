// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using System.Threading.Channels;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using NSubstitute;
using Shouldly;
using static Demo.Engine.Core.Features.StaThread.StaThreadRequests;
using WorkItem = Demo.Engine.Core.Features.StaThread.StaThreadService.StaSingleThreadedSynchronizationContext.WorkItem;

namespace Demo.Engine.Core.UTs;

public class StaThreadServiceTests
{
    /// <summary>
    /// Unit test validating that the <see cref="StaThreadService"/> correctly runs passed method invocations on the STA Main Thread.
    /// <para/>
    /// Producer task sends 5 consecutive requests, ensuring that they are sent from a thread different to the STA main thread.
    /// Then, when invoked, an assertion is performed to validate that invocation occurs on STA main thread.
    /// <para/>
    /// Test takes into account that STA thread apartment can only be enforced on Windows,
    /// so it validates that the thread name is <c>"Main STA thread"</c> on Windows,
    /// and <c>"Main thread"</c> on other platforms.
    /// </summary>
    /// <param name="timeoutToken">If the test doesn't end in time, it will be cancelled by the test framework</param>
    /// <remarks>
    /// <see cref="StaThreadService"/> is designed to run indefinitely, so we set a timeout to prevent the test from hanging indefinitely.
    /// </remarks>
    [Test]
    [Timeout(timeoutInMilliseconds: 20_000)]
    [Arguments(true)]
    [Arguments(false)]
    public async Task TestStaThreadService(
        bool useObjectPool,
        CancellationToken timeoutToken)
    {
        // Arrrange
        var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken);

        var serviceUnderTest = CreateService(
            useObjectPool: useObjectPool,
            cancellationTokenSource: cts,
            channel: out var channel);

        var executingTask = serviceUnderTest.ExecutingTask;
        var sendTestRequestTask = SendTestRequests(
            channel.Writer,
            cancellationTokenSource: cts,
            cancellationToken: cts.Token);

        // Act & Assert
        await Task.WhenAll(
            executingTask,
            sendTestRequestTask);
    }

    /// <summary>
    /// This test verifies that an exception thrown within the STA thread,
    /// that is cought in the context's <see cref="StaThreadService.StaSingleThreadedSynchronizationContext.Post(SendOrPostCallback, object?)"/> method,
    /// is property cought and gracefully shuts the STA thread down, without crashing the process.
    /// The exception is thrown from an async void method, to simulate a fire-and-forget scenario, where the exception would be unobserved if not properly handled.
    /// <para/>
    /// Completion <b>must</b> be awaited with <see cref="Task.ConfigureAwait(bool)"/> set to <see langword="false"/>, so that it doesn't deadlock on the STA thread, that is no longer alive if the exception is thrown.
    /// <code>
    /// await syncContext.Completion.ConfigureAwait(continueOnCapturedContext: false);
    /// </code>
    /// </summary>
    /// <param name="timeoutToken">If the test doesn't end in time, it will be cancelled by the test framework</param>
    /// <remarks>
    /// <see cref="StaThreadService"/> is designed to run indefinitely, so we set a timeout to prevent the test from hanging indefinitely.
    /// </remarks>
    [Test]
    [Timeout(timeoutInMilliseconds: 20_000)]
    [Arguments(true)]
    [Arguments(false)]
    public async Task TestExceptionHandling(
        bool useObjectPool,
        CancellationToken timeoutToken)
    {
        // Arrange
        StaThreadService.StaSingleThreadedSynchronizationContext? syncContext = null;

        var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken);
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        cts.Token.Register(() => tcs.TrySetResult(), useSynchronizationContext: false);

        try
        {
            syncContext = new StaThreadService.StaSingleThreadedSynchronizationContext(
                useObjectPool
                    ? GetObjectPool()
                    : null);

            syncContext.OnError += SyncContextOnError;

            SynchronizationContext.SetSynchronizationContext(syncContext);
            await Task.Yield();

            // Act
            TestException();

            // Assert
            await tcs.Task;
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(null);
            await Task.Yield();

            syncContext?.OnError -= SyncContextOnError;
            syncContext?.Dispose();
        }

        // Ensures that the syncContext internal 10 second timer to exit the process was cancelled
        await Task.Delay(15_000, timeoutToken);

        void SyncContextOnError(object? sender, Exception ex)
        {
            ex.Message.ShouldBe("TEST EXCEPTION");
            cts.Cancel();
        }
    }

    private static ObjectPool<WorkItem> GetObjectPool()
    {
        var provider = new DefaultObjectPoolProvider();
        var policy = new DefaultPooledObjectPolicy<WorkItem>();
        var workItemPool = provider.Create(policy);
        return workItemPool;
    }

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable TUnit0031 // Async void methods and lambdas are not allowed
    /// <summary>
    /// async void methods are not allowed in the codebase,
    /// but here it's used to simulate an exception happening in the <see cref="StaThreadService.StaSingleThreadedSynchronizationContext.Post(SendOrPostCallback, object?)"/>,
    /// from the Task continuation
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static async void TestException() => throw new Exception("TEST EXCEPTION");
#pragma warning restore TUnit0031 // Async void methods and lambdas are not allowed
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Sends a series of test requests to the specified channel for STA thread validation and signals cancellation upon
    /// completion.
    /// </summary>
    /// <remarks>
    /// This method is intended for test scenarios involving STA thread request validation.
    /// After the requests are sent, the <see cref="CancellationTokenSource"/> is cancelled,
    /// to signal that <see cref="StaThreadService"/> should stop.
    /// </remarks>
    /// <param name="channelWriter">The channel writer used to enqueue test STA thread requests. Must not be null.</param>
    /// <param name="cancellationTokenSource">The cancellation token source that will be cancelled when all test requests have been sent. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation. Optional.</param>
    /// <returns>A task that represents the asynchronous operation of sending test requests.</returns>
    private static async Task SendTestRequests(
        ChannelWriter<StaThreadRequests> channelWriter,
        CancellationTokenSource cancellationTokenSource,
        CancellationToken cancellationToken = default)
    {

        var requestNumber = 0;
        try
        {
            var (expectedThreadName, shouldBeSTA) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ("Main STA thread", true)
                : ("Main thread", false);

            var request = new TestStaThreadRequest(expectedThreadName, shouldBeSTA);
            for (; requestNumber < 5; requestNumber++)
            {
                await Task.Yield();
                Thread.CurrentThread.Name.ShouldNotBe(expectedThreadName);

                request.Invoked.Status.ShouldBe(TaskStatus.WaitingForActivation);

                await channelWriter.WriteAsync(
                    request,
                    cancellationToken);

                // Delay added to ensure that if any exception was thrown that would crash the sync context
                // that it would be processed before all requests are sent
                await Task.Delay(100, cancellationToken);

                var invokedResult = await request.Invoked.WaitAsync(cancellationToken);
                invokedResult.ShouldBeTrue();

                request.Reset(cancellationToken);
            }
        }
        finally
        {
            // Used to ensure all the requests were sent
            requestNumber.ShouldBe(5);
            cancellationTokenSource.Cancel();
        }
    }

    /// <summary>
    /// Test request that validates that the code is running on the expected thread, by checking the thread name.
    /// It can be reused for multiple invocations, by calling <see cref="Reset(CancellationToken)"/> after each invocation.
    /// </summary>
    /// <param name="ExpectedThreadName"></param>
    private sealed record TestStaThreadRequest(
        string ExpectedThreadName,
        bool ShouldBeSTA)
        : StaThreadWorkInner<bool>
    {
        protected override ValueTask<bool> InvokeFuncInternalAsync(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
            CancellationToken cancellationToken = default)
        {
            Thread.CurrentThread.Name.ShouldBe(ExpectedThreadName);
            //TestException();
            if (ShouldBeSTA)
            {
                Thread.CurrentThread.GetApartmentState().ShouldBe(ApartmentState.STA);
            }
            else
            {
                Thread.CurrentThread.GetApartmentState().ShouldNotBe(ApartmentState.STA);
            }

            return ValueTask.FromResult(true);
        }

        public new void Reset(CancellationToken cancellationToken)
            => base.Reset(cancellationToken);
    }

    private static StaThreadService CreateService(
        bool useObjectPool,
        CancellationTokenSource cancellationTokenSource,
        out Channel<StaThreadRequests> channel)
    {
        var loggerMock = Substitute.For<ILogger<StaThreadService>>();
        var hostApplicationLifetimeMock = Substitute.For<IHostApplicationLifetime>();
        var renderingEngineMock = Substitute.For<IRenderingEngine>();
        var osMessageHandlerMock = Substitute.For<IOSMessageHandler>();
        var mainLoopLifetimeMock = Substitute.For<IMainLoopLifetime>();

        mainLoopLifetimeMock.Token
            .Returns(cancellationTokenSource.Token);

        mainLoopLifetimeMock
            .When(
                mock => mock.Cancel())
            .Do(
                call => cancellationTokenSource.Cancel());

        hostApplicationLifetimeMock.ApplicationStopping.Returns(cancellationTokenSource.Token);

        channel = Channel.CreateBounded<StaThreadRequests>(
            // Configuration mirrors .AddStaThreadFeature() configuration
            new BoundedChannelOptions(10)
            {
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
            });

        var workItemPool = useObjectPool
            ? GetObjectPool()
            : null;

        return new StaThreadService(
                loggerMock,
                hostApplicationLifetimeMock,
                renderingEngineMock,
                osMessageHandlerMock,
                channel.Reader,
                mainLoopLifetimeMock,
                workItemPool);
    }
}