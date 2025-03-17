// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.Interfaces.Components;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.Interfaces.Rendering.Shaders;
using Demo.Engine.Core.Requests.Keyboard;
using Demo.Engine.Core.Services;
using Demo.Engine.Core.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Demo.Engine.Core.UTs.Services;

public class MainLoopServiceNewTests
{
    private readonly ILogger<MainLoopServiceNew> _subLogger;
    private readonly IStaThreadWriter _subStaThreadWriter;
    private readonly IMediator _subMediator;
    private readonly IShaderAsyncCompiler _subShaderAsyncCompiler;
    private readonly IFpsTimer _subFpsTimer;
    private readonly IRenderingEngine _subRenderingEngine;
    private readonly IMainLoopLifetime _subMainLoopLifetime;
    private readonly ILoopJob _subLoopJob;

    public MainLoopServiceNewTests()
    {
        _subLogger = Substitute.For<ILogger<MainLoopServiceNew>>();
        _subStaThreadWriter = Substitute.For<IStaThreadWriter>();
        _subMediator = Substitute.For<IMediator>();
        _subShaderAsyncCompiler = Substitute.For<IShaderAsyncCompiler>();
        //_subFpsTimer = Substitute.For<IFpsTimer>();
        _subFpsTimer = new FpsTimer(
            Substitute.For<ILogger<FpsTimer>>());
        _subRenderingEngine = Substitute.For<IRenderingEngine>();
        _subMainLoopLifetime = Substitute.For<IMainLoopLifetime>();
        _subLoopJob = Substitute.For<ILoopJob>();
    }

    private MainLoopServiceNew CreateMainLoopServiceNew()
        => new(
            _subLogger,
            _subStaThreadWriter,
            _subMediator,
            _subShaderAsyncCompiler,
            _subFpsTimer,
            _subRenderingEngine,
            _subMainLoopLifetime,
            _subLoopJob);

    [Fact]
    [SuppressMessage(
        category: "Reliability",
        checkId: "CA2012:Use ValueTasks correctly",
        Justification = "There are several ValueTasks that report as not being awaited, because they are actually only nSubstitute mock setups/verifies")]
    public async Task MainLoopService_Constructor_Starts_Loop_And_Can_Be_Properly_Finished()
    {
        // Arrange
        var keyboardCacheSub = Substitute.For<IKeyboardCache>();
        var keyboardCharCache = new KeyboardCharCache(keyboardCacheSub);
        var keyboardHandle = new KeyboardHandle(keyboardCacheSub);

        var cts = new CancellationTokenSource();
        _ = _subMainLoopLifetime.Token.Returns(cts.Token);

        _ = _subMediator
            .Send(
                Arg.Any<KeyboardCharCacheRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardCharCache);

        _ = _subMediator
            .Send(
                Arg.Any<KeyboardHandleRequest>(),
                Arg.Any<CancellationToken>())
            .Returns(
                keyboardHandle);

        var renderingSurfaceId = RenderingSurfaceId.NewId();
        _ = _subStaThreadWriter.CreateSurface(
            cts.Token)
            .Returns(
                Task.FromResult(
                    renderingSurfaceId));

        var renderingSurface = Substitute.For<IRenderingSurface>();

        _ = _subRenderingEngine.TryGetRenderingSurface(
            renderingSurfaceId,
            out Arg.Any<IRenderingSurface>()!)
            .Returns(parameters
            =>
            {
                parameters[1] = renderingSurface;
                return true;
            });

        _ = renderingSurface.ShouldNotBeNull();

        _ = _subLoopJob
            .Update(
                renderingSurface: renderingSurface!,
                keyboardHandle: keyboardHandle,
                keyboardCharCache: keyboardCharCache)
            .Returns(
                new ValueTask());

        _ = _subStaThreadWriter
            .DoEventsOk(
                renderingSurfaceId: renderingSurfaceId,
                cancellationToken: cts.Token)
            .Returns(
                ValueTask.FromResult(true));

        _subLoopJob.Render(
            _subRenderingEngine,
            renderingSurfaceId);

        // Act
        MainLoopServiceNew? mainLoopServiceNew = null;
        try
        {
            mainLoopServiceNew = CreateMainLoopServiceNew();

            // Assert
            mainLoopServiceNew.ExecutingTask.IsCompleted
                .ShouldBeFalse();

            await Task.Delay(100);
        }
        finally
        {
            if (mainLoopServiceNew is not null)
            {
                await mainLoopServiceNew.DisposeAsync();
            }
        }

        mainLoopServiceNew.ExecutingTask.IsCompleted
            .ShouldBeTrue();

        _ = _subLoopJob.Received().Update(
            renderingSurface!,
            keyboardHandle,
            keyboardCharCache);

        _subLoopJob.Received()
            .Render(
                _subRenderingEngine,
                renderingSurfaceId);
    }
}