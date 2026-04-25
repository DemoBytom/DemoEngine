// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Platform.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Demo.Engine.UTs;

[DisplayName("WindowsMessagePump")]
public class WindowsMessagePumpTests
{
    [Test]
    [Timeout(10_000)]
    [Category("Unit")]
    [DisplayName("WindowsMessagePump should invoke the provided callback on a STA thread")]
    public async Task WindowsMessagePumpInvokesOnStaThread(
        CancellationToken timeoutToken)
    {
        // Arrange
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken);

        var logger = Substitute.For<ILogger<WindowsMessagePump>>();
        var hostApplicationLifetime = Substitute.For<IHostApplicationLifetime>();
        var staThreadReader = new TestStaThreadReader(cts);

        hostApplicationLifetime.ApplicationStopping.Returns(cts.Token);

        using var windowsMessagePump = new WindowsMessagePump(
            logger,
            hostApplicationLifetime,
            staThreadReader);

        // Act
        await windowsMessagePump.StartAsync(timeoutToken);
        await windowsMessagePump.ExecuteTask
            .ShouldNotBeNull()
            .WaitAsync(timeoutToken);
        await windowsMessagePump.StopAsync(timeoutToken);

        // Assert
        staThreadReader.Invoked.ShouldBeTrue();
        staThreadReader.InvokedThreadName.ShouldBe("Main STA thread");
        staThreadReader.ApartmentState.ShouldBe(ApartmentState.STA);
    }

    [Test]
    [Timeout(10_000)]
    [Category("Unit")]
    [DisplayName("WindowsMessagePump should correctly cancel the STA thread when requested")]
    public async Task WindowsMessagePumpCorrectlyCancels(
        CancellationToken timeoutToken)
    {
        // Arrange
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutToken);

        var logger = Substitute.For<ILogger<WindowsMessagePump>>();
        var hostApplicationLifetime = Substitute.For<IHostApplicationLifetime>();
        var staThreadReader = new TestStaThreadReaderWithDelay(cts, TimeSpan.FromSeconds(5));

        hostApplicationLifetime.ApplicationStopping.Returns(cts.Token);
        using var windowsMessagePump = new WindowsMessagePump(
            logger,
            hostApplicationLifetime,
            staThreadReader);

        await windowsMessagePump.StartAsync(timeoutToken);
        await Task.WhenAll(
            windowsMessagePump.ExecuteTask
                .ShouldNotBeNull()
                .WaitAsync(timeoutToken),
            CancelAfterDelyingStarted());
        await windowsMessagePump.StopAsync(timeoutToken);

        // Assert
        staThreadReader.Delying.ShouldBeTrue();
        staThreadReader.Cancelled.ShouldBeFalse();
        timeoutToken.IsCancellationRequested.ShouldBeFalse();

        // local functions
        async Task CancelAfterDelyingStarted()
        {
            await staThreadReader.DelyingStarted.WaitAsync(timeoutToken);
            await cts.CancelAsync();
        }
    }

    private sealed class TestStaThreadReader(
        CancellationTokenSource cancellationTokenSource)
        : IStaThreadReader
    {
        public bool Invoked { get; private set; }
        public string? InvokedThreadName { get; private set; }
        public ApartmentState ApartmentState { get; private set; }

        public async Task Invoke(
            Func<Func<CancellationToken, ValueTask<bool>>, CancellationToken, Task<bool>> callback,
            CancellationToken cancellationToken = default)
            => await callback(
                async ct =>
                {
                    Invoked = true;
                    InvokedThreadName = Thread.CurrentThread.Name;
                    ApartmentState = Thread.CurrentThread.GetApartmentState();

                    cancellationTokenSource.Cancel();
                    return true;
                },
                cancellationToken);
    }

    private sealed class TestStaThreadReaderWithDelay(
        CancellationTokenSource cancellationTokenSource,
        TimeSpan delay)
        : IStaThreadReader
    {
        public bool Cancelled { get; private set; }
        public bool Delying { get; private set; }

        private readonly TaskCompletionSource _delyingStartedCompletionSource = new(
            TaskCreationOptions.RunContinuationsAsynchronously);

        public Task DelyingStarted => _delyingStartedCompletionSource.Task;

        public async Task Invoke(
            Func<Func<CancellationToken, ValueTask<bool>>, CancellationToken, Task<bool>> callback,
            CancellationToken cancellationToken = default)
            => await callback(
                async ct =>
                {
                    Delying = true;
                    _delyingStartedCompletionSource.SetResult();
                    await Task.Delay(delay, ct);

                    cancellationTokenSource.Cancel();
                    Cancelled = true;
                    return true;
                },
                cancellationToken);
    }
}