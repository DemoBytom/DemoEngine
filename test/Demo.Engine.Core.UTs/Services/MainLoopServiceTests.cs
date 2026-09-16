// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Services;
using Demo.Engine.Core.ValueObjects;
using Mediator;
using Microsoft.Extensions.Logging;
using TUnit.Assertions.Should;
using TUnit.Assertions.Should.Extensions;

namespace Demo.Engine.Core.UTs.Services;

public sealed class MainLoopServiceTests
{
    private readonly MockRepository _mockRepository;
    private readonly Mock<ILogger<MainLoopService>> _subLogger;
    private readonly Mock<IStaThreadWriter> _subStaThreadWriter;
    private readonly Mock<IMediator> _subMediator;
    private readonly IFpsTimer _subFpsTimer;
    private readonly Mock<IRenderingEngine> _subRenderingEngine;
    private readonly Mock<IMainLoopLifetime> _subMainLoopLifetime;
    private readonly Mock<ILoopJob> _subLoopJob;

    public MainLoopServiceTests()
    {
        _mockRepository = new MockRepository(MockBehavior.Strict);

        _subLogger = _mockRepository.Of<ILogger<MainLoopService>>(MockBehavior.Loose);
        _subStaThreadWriter = _mockRepository.Of<IStaThreadWriter>();
        _subMediator = _mockRepository.Of<IMediator>();

        _subFpsTimer = new FpsTimer(
            _mockRepository
                .Of<ILogger<FpsTimer>>(MockBehavior.Loose)
                .Object);

        _subRenderingEngine = _mockRepository.Of<IRenderingEngine>();
        _subMainLoopLifetime = _mockRepository.Of<IMainLoopLifetime>();
        _subLoopJob = _mockRepository.Of<ILoopJob>();
    }

    private MainLoopService CreateMainLoopService()
        => new(
            _subLogger.Object,
            _subStaThreadWriter.Object,
            _subMediator.Object,
            _subFpsTimer,
            _subRenderingEngine.Object,
            _subMainLoopLifetime.Object,
            _subLoopJob.Object);

    [Test]
    [Timeout(10_000)]
    [SuppressMessage(
        category: "Reliability",
        checkId: "CA2012:Use ValueTasks correctly",
        Justification = "There are several ValueTasks that report as not being awaited, because they are actually only nSubstitute mock setups/verifies")]
    public async Task MainLoopService_Constructor_Starts_Loop_And_Can_Be_Properly_Finished(
        CancellationToken cancellationToken)
    {
        // Arrange
        var keyboardCacheSub = IKeyboardCache.Mock(MockBehavior.Strict);
        var keyboardCharCache = new KeyboardCharCache(keyboardCacheSub.Object);
        var keyboardHandle = new KeyboardHandle(keyboardCacheSub.Object);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = _subMainLoopLifetime.Token.Returns(cts.Token);
        _subMainLoopLifetime.Cancel().Callback(cts.Cancel);

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
            Is(_subRenderingEngine.Object),
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

        _subLoopJob
            .Render(
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

            await Task.Delay(100, cancellationToken);
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

        _mockRepository.VerifyAll();

        _subLoopJob
            .Update(
                renderingSurface,
                keyboardHandle,
                keyboardCharCache)
            .WasCalled(Times.AtLeastOnce);

        _subLoopJob
            .Render(
                Is(_subRenderingEngine.Object),
                renderingSurfaceId)
            .WasCalled(Times.AtLeastOnce);

    }
}