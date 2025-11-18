// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Interfaces;

public interface IFpsTimer
{
    void StartRenderingTimer(
        RenderingSurfaceId surfaceId);

    void StopRenderingTimer(
        RenderingSurfaceId surfaceId);

    FpsTimerScope StartRenderingTimerScope(
        RenderingSurfaceId surfaceId);

    void StartUpdateTimer();

    void StopUpdateTimer();

    internal interface IFpsInternalTimer
    {
        void Start();

        void Stop();
    }
}

public readonly ref struct FpsTimerScope
    : IDisposable
{
    private readonly IFpsTimer.IFpsInternalTimer _fpsCounter;

    [Obsolete($"{nameof(FpsTimerScope)} must not be initialized via this constructor!")]
    public FpsTimerScope()
        => throw new InvalidOperationException(
            $"{nameof(FpsTimerScope)} must not be initialized via this constructor!");

    internal FpsTimerScope(
        IFpsTimer.IFpsInternalTimer fpsCounter)
    {
        _fpsCounter = fpsCounter;
        _fpsCounter.Start();
    }

    public void Dispose()
        => _fpsCounter.Stop();
}