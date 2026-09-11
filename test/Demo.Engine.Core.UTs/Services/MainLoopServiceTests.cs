// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Services;
using Demo.Engine.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Engine.Core.UTs.Services;

public class MainLoopServiceTests
{
    private readonly Mock<ILogger<MainLoopService>> _subLogger;
    private readonly Mock<IStaThreadWriter> _subStaThreadWriter;
    private readonly Mock<IMediator> _subMediator;
    private readonly Mock<IShaderAsyncCompiler> _subShaderAsyncCompiler;
    private readonly IFpsTimer _subFpsTimer;
    private readonly Mock<IRenderingEngine> _subRenderingEngine;
    private readonly Mock<IMainLoopLifetime> _subMainLoopLifetime;
    private readonly Mock<ILoopJob> _subLoopJob;

    public MainLoopServiceTests()
    {
        _subLogger = ILogger<MainLoopService>.Mock(MockBehavior.Loose);
        _subStaThreadWriter = IStaThreadWriter.Mock(MockBehavior.Strict);
        _subMediator = IMediator.Mock(MockBehavior.Strict);
        _subShaderAsyncCompiler = IShaderAsyncCompiler.Mock(MockBehavior.Strict);
        _subFpsTimer = new FpsTimer(
            ILogger<FpsTimer>
                .Mock(MockBehavior.Loose)
                .Object);
        _subRenderingEngine = IRenderingEngine.Mock(MockBehavior.Strict);
        _subMainLoopLifetime = IMainLoopLifetime.Mock(MockBehavior.Strict);
        _subLoopJob = ILoopJob.Mock(MockBehavior.Strict);
    }

    private MainLoopService CreateMainLoopService()
        => new(
            _subLogger.Object,
            _subStaThreadWriter.Object,
            _subMediator.Object,
            _subShaderAsyncCompiler.Object,
            _subFpsTimer,
            _subRenderingEngine.Object,
            _subMainLoopLifetime.Object,
            _subLoopJob.Object);

    [Test]
    [SuppressMessage(
        category: "Reliability",
        checkId: "CA2012:Use ValueTasks correctly",
        Justification = "There are several ValueTasks that report as not being awaited, because they are actually only nSubstitute mock setups/verifies")]
    public async Task MainLoopService_Constructor_Starts_Loop_And_Can_Be_Properly_Finished()
    {
        // Arrange
        var keyboardCacheSub = IKeyboardCache.Mock(MockBehavior.Strict);
        var keyboardCharCache = new KeyboardCharCache(keyboardCacheSub.Object);
        var keyboardHandle = new KeyboardHandle(keyboardCacheSub.Object);

        var cts = new CancellationTokenSource();
        _ = _subMainLoopLifetime.Token.Returns(cts.Token);
        _subMainLoopLifetime.Cancel().Callback(cts.Cancel);

        _subShaderAsyncCompiler
            .CompileShaders(
                Is(cts.Token))
            .Returns(true);

        _ = _subMediator
            .Send(
                Any<IRequest<KeyboardCharCache>>(),
                Any<CancellationToken>())
            .Returns(
                keyboardCharCache);

        _ = _subMediator
            .Send(
                Any<IRequest<KeyboardHandle>>(),
                Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingSurfaceId = RenderingSurfaceId.NewId();
        _ = _subStaThreadWriter.CreateSurface(
            cts.Token)
            .ReturnsAsync(
                Task.FromResult(
                    renderingSurfaceId));

        var renderingSurface = IRenderingSurface.Mock(MockBehavior.Strict);

        _ = _subRenderingEngine
            .TryGetRenderingSurface(
                renderingSurfaceId)
            .SetsOutRenderingSurface(
                renderingSurface)
            .Returns(true);

        await renderingSurface.Should().NotBeNull();

        _ = _subLoopJob
            .Update(
                renderingSurface: renderingSurface,
                keyboardHandle: keyboardHandle,
                keyboardCharCache: keyboardCharCache)
            .ReturnsAsync(
                new ValueTask());

        _ = _subStaThreadWriter
            .DoEventsOk(
                renderingSurfaceId: renderingSurfaceId,
                cancellationToken: cts.Token)
            .ReturnsAsync(
                ValueTask.FromResult(true));

        _subLoopJob.Render(
            Is(_subRenderingEngine.Object),
            renderingSurfaceId);

        // Act
        MainLoopService? mainLoopService = null;
        try
        {
            mainLoopService = CreateMainLoopService();

            // Assert
            await mainLoopService.ExecutingTask.IsCompleted
                .Should().BeFalse();

            await Task.Delay(100);
        }
        finally
        {
            if (mainLoopService is not null)
            {
                await mainLoopService.DisposeAsync();
            }
        }

        await mainLoopService.ExecutingTask.IsCompleted
            .Should().BeTrue();

        _subLoopJob
            .Update(
                renderingSurface,
                keyboardHandle,
                keyboardCharCache)
            .WasCalled();

        _subLoopJob
            .Render(
                Is(_subRenderingEngine.Object),
                renderingSurfaceId)
            .WasCalled();
    }
}