// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using Demo.Engine.Core.Interfaces;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services;

internal sealed class FpsTimer(
    ILogger<FpsTimer> logger)
    : IFpsTimer
{
    internal class SurfaceFpsCounter(
        ILogger logger,
        RenderingSurfaceId surfaceId)
        : IFpsTimer.IFpsInternalTimer
    {
        private readonly ILogger _logger = logger;
        private readonly RenderingSurfaceId _surfaceId = surfaceId;

        private float _averageMs { get; set; } = 0.0f;
        private ulong _fpsCounter { get; set; } = 1;
        private long _start;
        private long _seconds = Stopwatch.GetTimestamp();

        public void Start()
            => _start = Stopwatch.GetTimestamp();

        public void Stop()
        {
            var dt = Stopwatch.GetElapsedTime(_start);

            _averageMs += (dt.Milliseconds - _averageMs) / _fpsCounter;
            ++_fpsCounter;

            if (Stopwatch.GetElapsedTime(_seconds).TotalSeconds >= 1)
            {
                _logger.LogTrace(
                    "{surfaceId}: Avg. frame (ms): {millisecond}, fps: {fps}",
                    _surfaceId,
                    _averageMs,
                    _fpsCounter);

                _averageMs = 0.0f;
                _fpsCounter = 1;
                _seconds = Stopwatch.GetTimestamp();
            }
        }
    }

    private readonly ILogger<FpsTimer> _logger = logger;
    private readonly Dictionary<RenderingSurfaceId, SurfaceFpsCounter> _surfaceFpsCounters = [];

    public void StartRenderingTimer(
        RenderingSurfaceId surfaceId)
    {
        if (!_surfaceFpsCounters.TryGetValue(
            surfaceId,
            out var fpsCounter))
        {
            fpsCounter = new SurfaceFpsCounter(
                _logger,
                surfaceId);

            _surfaceFpsCounters.Add(
                surfaceId,
                fpsCounter);
        }

        fpsCounter.Start();
    }

    public FpsTimerScope StartRenderingTimerScope(
        RenderingSurfaceId surfaceId)
    {
        if (!_surfaceFpsCounters.TryGetValue(
            surfaceId,
            out var fpsCounter))
        {
            fpsCounter = new SurfaceFpsCounter(
                _logger,
                surfaceId);

            _surfaceFpsCounters.Add(
                surfaceId,
                fpsCounter);
        }

        return new FpsTimerScope(
            fpsCounter);
    }

    public void StopRenderingTimer(
        RenderingSurfaceId surfaceId)
    {
        if (_surfaceFpsCounters.TryGetValue(
            surfaceId,
            out var fpsCounter))
        {
            fpsCounter.Stop();
        }
    }

    private float _averageMs { get; set; } = 0.0f;
    private ulong _upsCounter { get; set; } = 1;
    private long _start;
    private long _seconds = Stopwatch.GetTimestamp();

    public void StartUpdateTimer()
        => _start = Stopwatch.GetTimestamp();

    public void StopUpdateTimer()
    {
        var dt = Stopwatch.GetElapsedTime(_start);

        _averageMs += (dt.Milliseconds - _averageMs) / _upsCounter;
        ++_upsCounter;

        if (Stopwatch.GetElapsedTime(_seconds).TotalSeconds >= 1)
        {
            _logger.LogTrace(
                "Avg. update (ms): {millisecond}, ups: {ups}",
                _averageMs,
                _upsCounter);

            _averageMs = 0.0f;
            _upsCounter = 1;
            _seconds = Stopwatch.GetTimestamp();
        }
    }
}