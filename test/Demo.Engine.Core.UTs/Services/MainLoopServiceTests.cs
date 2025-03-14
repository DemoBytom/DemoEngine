// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Engine.Core.Services;
using MediatR;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Demo.Engine.Core.UTs.Services;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "unit tests don't need those")]
public class MainLoopServiceTests
{
    private readonly IMediator _mockMediator;
    private readonly IHostApplicationLifetime _mockHostApplicationLifetime;
    private readonly IOSMessageHandler _mockOSMessageHandler;
    private readonly IRenderingEngine _mockRenderingEngine;
    private readonly IDebugLayerLogger _mockDebugLayer;

    public MainLoopServiceTests()
    {
        _mockMediator = Substitute.For<IMediator>();
        _mockHostApplicationLifetime = Substitute.For<IHostApplicationLifetime>();
        _mockOSMessageHandler = Substitute.For<IOSMessageHandler>();
        _mockRenderingEngine = Substitute.For<IRenderingEngine>();
        _mockDebugLayer = Substitute.For<IDebugLayerLogger>();
    }

    private MainLoopService CreateService()
        => new(
            _mockMediator,
            _mockHostApplicationLifetime,
            _mockOSMessageHandler,
            _mockRenderingEngine,
            _mockDebugLayer);

    [Fact]
    public void RunAsync_Throws_On_UpdateCallback_Null()
    {
        // Arrange
        var service = CreateService();

        CancellationToken cancellationToken = default;
        static Task RenderCallback(IRenderingEngine _) => Task.CompletedTask;

        // Act
        Func<Task> func = () => service.RunAsync(
            null!,
            RenderCallback,
            cancellationToken);

        // Assert
        _ = func.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void RunAsync_Throws_On_RenderCallback_Null()
    {
        // Arrange
        var service = CreateService();

        CancellationToken cancellationToken = default;
        static Task UpdateCallback(KeyboardHandle _, KeyboardCharCache __) => Task.CompletedTask;

        // Act
        Func<Task> func = () => service.RunAsync(
            UpdateCallback,
            null!,
            cancellationToken);

        // Assert
        _ = func.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RunAsync_CancellationToken_Properly_Cancels_After_Time()
    {
        // Arrange
        var service = CreateService();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        static Task RenderCallback(IRenderingEngine _) => Task.CompletedTask;
        static Task UpdateCallback(KeyboardHandle _, KeyboardCharCache __) => Task.CompletedTask;

        var keybaorCacheSub = Substitute.For<IKeyboardCache>();
        var keyboardCache = new KeyboardCharCache(keybaorCacheSub);
        var keyboardHandle = new KeyboardHandle(keybaorCacheSub);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardCharCacheRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardCache);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardHandleRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingControlMock = Substitute.For<IRenderingControl>();
        _ = _mockRenderingEngine.Control.Returns(renderingControlMock);

        _ = _mockOSMessageHandler.DoEvents(renderingControlMock).Returns(true);

        _ = _mockHostApplicationLifetime.ApplicationStopping.Returns(new CancellationToken());

        // Act
        var sw = Stopwatch.StartNew();
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);
        sw.Stop();

        // Assert
        // We let it have a bit of a leeway
        sw.Elapsed.TotalSeconds.ShouldBeInRange(
            TimeSpan.FromSeconds(4.9).TotalSeconds,
            TimeSpan.FromSeconds(5.9).TotalSeconds);
    }

    [Fact]
    public async Task RunAsync_CancellationToken_Properly_Cancels_From_Update()
    {
        // Arrange
        var service = CreateService();

        var cts = new CancellationTokenSource();

        static Task RenderCallback(IRenderingEngine _) => Task.CompletedTask;
        var iUpdate = 0;
        Task UpdateCallback(KeyboardHandle _, KeyboardCharCache __)
        {
            if (iUpdate++ >= 5)
            {
                cts.Cancel();
            }

            return Task.CompletedTask;
        }
        var keybaorCacheSub = Substitute.For<IKeyboardCache>();
        var keyboardCache = new KeyboardCharCache(keybaorCacheSub);
        var keyboardHandle = new KeyboardHandle(keybaorCacheSub);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardCharCacheRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardCache);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardHandleRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingControlMock = Substitute.For<IRenderingControl>();
        _ = _mockRenderingEngine.Control.Returns(renderingControlMock);

        _ = _mockOSMessageHandler.DoEvents(renderingControlMock).Returns(true);

        _ = _mockHostApplicationLifetime.ApplicationStopping.Returns(new CancellationToken());

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.ShouldBe(6);
    }

    [Fact]
    public async Task RunAsync_HostApplicationLifetime_Properly_Cancels_From_Update()
    {
        // Arrange
        var service = CreateService();

        var cts = new CancellationTokenSource();
        var appLifetimeCTS = new CancellationTokenSource();

        static Task RenderCallback(IRenderingEngine _) => Task.CompletedTask;
        var iUpdate = 0;
        Task UpdateCallback(KeyboardHandle _, KeyboardCharCache __)
        {
            if (iUpdate++ >= 5)
            {
                appLifetimeCTS.Cancel();
            }

            return Task.CompletedTask;
        }

        var keybaorCacheSub = Substitute.For<IKeyboardCache>();
        var keyboardCache = new KeyboardCharCache(keybaorCacheSub);
        var keyboardHandle = new KeyboardHandle(keybaorCacheSub);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardCharCacheRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardCache);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardHandleRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingControlMock = Substitute.For<IRenderingControl>();
        _ = _mockRenderingEngine.Control.Returns(renderingControlMock);

        _ = _mockOSMessageHandler.DoEvents(renderingControlMock).Returns(true);

        _ = _mockHostApplicationLifetime.ApplicationStopping.Returns(appLifetimeCTS.Token);

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.ShouldBe(6);
    }

    [Fact]
    public async Task RunAsync_Verify_Update_And_Render_All_Called()
    {
        // Arrange
        var service = CreateService();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        var appLifetimeCTS = new CancellationTokenSource();

        var iUpdate = 0;
        var iRender = 0;
        Task RenderCallback(IRenderingEngine _)
        {
            ++iRender;
            return Task.CompletedTask;
        }

        Task UpdateCallback(KeyboardHandle _, KeyboardCharCache __)
        {
            ++iUpdate;
            return Task.CompletedTask;
        }

        var keybaorCacheSub = Substitute.For<IKeyboardCache>();
        var keyboardCache = new KeyboardCharCache(keybaorCacheSub);
        var keyboardHandle = new KeyboardHandle(keybaorCacheSub);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardCharCacheRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardCache);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardHandleRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingControlMock = Substitute.For<IRenderingControl>();
        _ = _mockRenderingEngine.Control.Returns(renderingControlMock);

        _ = _mockOSMessageHandler.DoEvents(renderingControlMock).Returns(true);

        _ = _mockHostApplicationLifetime.ApplicationStopping.Returns(appLifetimeCTS.Token);

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.ShouldBeGreaterThan(0);
        iRender.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task RunAsync_DoEvents_Properly_Cancels_From_Update()
    {
        // Arrange
        var service = CreateService();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        var appLifetimeCTS = new CancellationTokenSource();

        static Task RenderCallback(IRenderingEngine _) => Task.CompletedTask;
        var iUpdate = 0;
        Task UpdateCallback(KeyboardHandle _, KeyboardCharCache __)
        {
            ++iUpdate;
            return Task.CompletedTask;
        }

        var keybaorCacheSub = Substitute.For<IKeyboardCache>();
        var keyboardCache = new KeyboardCharCache(keybaorCacheSub);
        var keyboardHandle = new KeyboardHandle(keybaorCacheSub);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardCharCacheRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardCache);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardHandleRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingControlMock = Substitute.For<IRenderingControl>();
        _ = _mockRenderingEngine.Control.Returns(renderingControlMock);

        _ = _mockOSMessageHandler.DoEvents(renderingControlMock).Returns(_ => iUpdate <= 5);

        _ = _mockHostApplicationLifetime.ApplicationStopping.Returns(appLifetimeCTS.Token);

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.ShouldBe(6);
    }

    [Fact]
    public async Task RunAsync_IsRunning_Is_Properly_Set()
    {
        // Arrange
        var service = CreateService();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        var appLifetimeCTS = new CancellationTokenSource();

        static Task RenderCallback(IRenderingEngine _) => Task.CompletedTask;

        Task UpdateCallback(KeyboardHandle _, KeyboardCharCache __)
        {
            service.IsRunning.ShouldBeTrue();
            return Task.CompletedTask;
        }

        var keybaorCacheSub = Substitute.For<IKeyboardCache>();
        var keyboardCache = new KeyboardCharCache(keybaorCacheSub);
        var keyboardHandle = new KeyboardHandle(keybaorCacheSub);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardCharCacheRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardCache);

        _ = _mockMediator
            .Send(
                Arg.Any<KeyboardHandleRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingControlMock = Substitute.For<IRenderingControl>();
        _ = _mockRenderingEngine.Control.Returns(renderingControlMock);

        _ = _mockOSMessageHandler.DoEvents(renderingControlMock).Returns(true);

        _ = _mockHostApplicationLifetime.ApplicationStopping.Returns(appLifetimeCTS.Token);

        // Act
        service.IsRunning.ShouldBeFalse();
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        service.IsRunning.ShouldBeFalse();
    }
}