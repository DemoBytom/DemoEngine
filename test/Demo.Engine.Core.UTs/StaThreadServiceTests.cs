// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using System.Threading.Channels;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;
using static Demo.Engine.Core.Features.StaThread.StaThreadRequests;

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
    public async Task TestStaThreadService(CancellationToken timeoutToken)
    {
        // Arrrange
        var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken);

        using var serviceUnderTest = CreateService(
            cts,
            out var channel);

        var executingTask = serviceUnderTest.ExecutingTask;
        var sendTestRequestTask = SendTestRequests(
            channel.Writer,
            cancellationTokenSource: cts,
            cancellationToken: timeoutToken);

        // Act & Assert
        await Task.WhenAll(
            executingTask,
            sendTestRequestTask);
    }

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
        try
        {
            var (expectedThreadName, shouldBeSTA) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ("Main STA thread", true)
                : ("Main thread", false);

            var request = new TestStaThreadRequest(expectedThreadName, shouldBeSTA);
            for (var i = 0; i < 5; i++)
            {
                await Task.Yield();
                Thread.CurrentThread.Name.ShouldNotBe(expectedThreadName);

                request.Invoked.Status.ShouldBe(TaskStatus.WaitingForActivation);

                await channelWriter.WriteAsync(
                    request,
                    cancellationToken);

                var invokedResult = await request.Invoked;
                invokedResult.ShouldBeTrue();

                request.Reset(cancellationToken);
            }
        }
        finally
        {
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
        protected override bool InvokeFuncInternal(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler)
        {
            Thread.CurrentThread.Name.ShouldBe(ExpectedThreadName);
            if (ShouldBeSTA)
            {
                Thread.CurrentThread.GetApartmentState().ShouldBe(ApartmentState.STA);
            }
            else
            {
                Thread.CurrentThread.GetApartmentState().ShouldNotBe(ApartmentState.STA);
            }

            return true;
        }

        public new void Reset(CancellationToken cancellationToken)
            => base.Reset(cancellationToken);
    }

    private static StaThreadService CreateService(
        in CancellationTokenSource cancellationTokenSource,
        out Channel<StaThreadRequests> channel)
    {
        var hostApplicationLifetimeMock = Substitute.For<IHostApplicationLifetime>();
        var renderingEngineMock = Substitute.For<IRenderingEngine>();
        var osMessageHandlerMock = Substitute.For<IOSMessageHandler>();
        var mainLoopLifetimeMock = Substitute.For<IMainLoopLifetime>();

        mainLoopLifetimeMock.Token.Returns(cancellationTokenSource.Token);
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

        return new StaThreadService(
                hostApplicationLifetimeMock,
                renderingEngineMock,
                osMessageHandlerMock,
                channel.Reader,
                mainLoopLifetimeMock);
    }
}