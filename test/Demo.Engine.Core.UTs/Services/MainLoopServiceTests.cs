// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Engine.Core.Services;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Demo.Engine.Core.UTs.Services;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "unit tests don't need those")]
public class MainLoopServiceTests
{
    private readonly MockRepository _mockRepository;

    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<IHostApplicationLifetime> _mockHostApplicationLifetime;
    private readonly Mock<IOSMessageHandler> _mockOSMessageHandler;
    private readonly Mock<IRenderingEngine> _mockRenderingEngine;
    private readonly Mock<IDebugLayerLogger> _mockDebugLayer;

    public MainLoopServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);

        _mockMediator = _mockRepository.Create<IMediator>();
        _mockHostApplicationLifetime = _mockRepository.Create<IHostApplicationLifetime>();
        _mockOSMessageHandler = _mockRepository.Create<IOSMessageHandler>();
        _mockRenderingEngine = _mockRepository.Create<IRenderingEngine>();
        _mockDebugLayer = _mockRepository.Create<IDebugLayerLogger>(MockBehavior.Loose);
    }

    private MainLoopService CreateService()
        => new(
            _mockMediator.Object,
            _mockHostApplicationLifetime.Object,
            _mockOSMessageHandler.Object,
            _mockRenderingEngine.Object,
            _mockDebugLayer.Object);

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
        func.Should().ThrowAsync<ArgumentNullException>();
        _mockRepository.VerifyAll();
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
        func.Should().ThrowAsync<ArgumentNullException>();
        _mockRepository.VerifyAll();
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

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardCharCacheRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardCharCache(new Mock<IKeyboardCache>().Object));

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardHandleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardHandle(new Mock<IKeyboardCache>().Object));

        var renderingControllMock = new Mock<IRenderingControl>();
        _mockRenderingEngine
            .SetupGet(o => o.Control)
            .Returns(renderingControllMock.Object);

        _mockOSMessageHandler
            .Setup(o => o.DoEvents(renderingControllMock.Object))
            .Returns(true);

        _mockHostApplicationLifetime
            .Setup(o => o.ApplicationStopping)
            .Returns(new CancellationToken());

        // Act
        var sw = Stopwatch.StartNew();
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);
        sw.Stop();

        // Assert
        //We let it have a bit of a leeway
        sw.Elapsed.TotalSeconds.Should().BeInRange(
            TimeSpan.FromSeconds(4.9).TotalSeconds,
            TimeSpan.FromSeconds(5.9).TotalSeconds);
        _mockRepository.VerifyAll();
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

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardCharCacheRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardCharCache(new Mock<IKeyboardCache>().Object));

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardHandleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardHandle(new Mock<IKeyboardCache>().Object));

        var renderingControllMock = new Mock<IRenderingControl>();
        _mockRenderingEngine
            .SetupGet(o => o.Control)
            .Returns(renderingControllMock.Object);

        _mockOSMessageHandler
            .Setup(o => o.DoEvents(renderingControllMock.Object))
            .Returns(true);

        _mockHostApplicationLifetime
            .Setup(o => o.ApplicationStopping)
            .Returns(new CancellationToken());

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.Should().Be(6);
        _mockRepository.VerifyAll();
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

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardCharCacheRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardCharCache(new Mock<IKeyboardCache>().Object));

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardHandleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardHandle(new Mock<IKeyboardCache>().Object));

        var renderingControllMock = new Mock<IRenderingControl>();
        _mockRenderingEngine
            .SetupGet(o => o.Control)
            .Returns(renderingControllMock.Object);

        _mockOSMessageHandler
            .Setup(o => o.DoEvents(renderingControllMock.Object))
            .Returns(true);

        _mockHostApplicationLifetime
            .Setup(o => o.ApplicationStopping)
            .Returns(appLifetimeCTS.Token);

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.Should().Be(6);
        _mockRepository.VerifyAll();
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

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardCharCacheRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardCharCache(new Mock<IKeyboardCache>().Object));

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardHandleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardHandle(new Mock<IKeyboardCache>().Object));

        var renderingControllMock = new Mock<IRenderingControl>();
        _mockRenderingEngine
            .SetupGet(o => o.Control)
            .Returns(renderingControllMock.Object);

        _mockOSMessageHandler
            .Setup(o => o.DoEvents(renderingControllMock.Object))
            .Returns(true);

        _mockHostApplicationLifetime
            .Setup(o => o.ApplicationStopping)
            .Returns(appLifetimeCTS.Token);

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.Should().BeGreaterThan(0);
        iRender.Should().BeGreaterThan(0);
        _mockRepository.VerifyAll();
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

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardCharCacheRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardCharCache(new Mock<IKeyboardCache>().Object));

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardHandleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardHandle(new Mock<IKeyboardCache>().Object));

        var renderingControllMock = new Mock<IRenderingControl>();
        _mockRenderingEngine
            .SetupGet(o => o.Control)
            .Returns(renderingControllMock.Object);

        _mockOSMessageHandler
            .Setup(o => o.DoEvents(renderingControllMock.Object))
            .Returns(() => iUpdate <= 5);

        _mockHostApplicationLifetime
            .Setup(o => o.ApplicationStopping)
            .Returns(appLifetimeCTS.Token);

        // Act
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        iUpdate.Should().Be(6);
        _mockRepository.VerifyAll();
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
            service.IsRunning.Should().BeTrue();
            return Task.CompletedTask;
        }

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardCharCacheRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardCharCache(new Mock<IKeyboardCache>().Object));

        _mockMediator
            .Setup(o => o.Send(It.IsAny<KeyboardHandleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyboardHandle(new Mock<IKeyboardCache>().Object));

        var renderingControllMock = new Mock<IRenderingControl>();
        _mockRenderingEngine
            .SetupGet(o => o.Control)
            .Returns(renderingControllMock.Object);

        _mockOSMessageHandler
            .Setup(o => o.DoEvents(renderingControllMock.Object))
            .Returns(true);

        _mockHostApplicationLifetime
            .Setup(o => o.ApplicationStopping)
            .Returns(appLifetimeCTS.Token);

        // Act
        service.IsRunning.Should().BeFalse();
        await service.RunAsync(
            UpdateCallback,
            RenderCallback,
            cts.Token);

        // Assert
        service.IsRunning.Should().BeFalse();

        _mockRepository.VerifyAll();
    }
}