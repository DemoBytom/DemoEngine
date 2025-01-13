// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using Demo.Engine.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services;

internal sealed class FpsTimer(
    ILogger<FpsTimer> logger)
    : IFpsTimer
{
    private readonly ILogger<FpsTimer> _logger = logger;
    private float _averageMs = 0.0f;
    private ulong _fpsCounter = 1;
    private long _start;
    private long _seconds = Stopwatch.GetTimestamp();

    public void Start()
        => _start = Stopwatch.GetTimestamp();

    public void Stop()
    {
        var dt = Stopwatch.GetElapsedTime(_start);

        _averageMs += (dt.Milliseconds - _averageMs) / _fpsCounter;
        ++_fpsCounter;

        if (Stopwatch.GetElapsedTime(_seconds).Seconds >= 1)
        {
            _logger.LogTrace(
                "Avg. frame (ms): {millisecond}, fps: {fps}", _averageMs, _fpsCounter);

            _averageMs = 0.0f;
            _fpsCounter = 1;
            _seconds = Stopwatch.GetTimestamp();
        }
    }
}