// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Core.Services;

// cannot be file scoped!
internal static partial class LoggingExtensions
{
    private const string SERVICE_IS_STARTING = "{ServiceName} starting! v{Version}";
    private const string SERVICE_IS_WORKING = "{ServiceName} working! v{Version}";
    private const string SERVICE_IS_STOPPING = "{ServiceName} stopping!";
    private const string SERVICE_FAILED_WITH_ERROR = "{ServiceName} failed with error! {ErrorMessage}";

    private const string AVERAGE_FPS = "{SurfaceId}: Avg. frame (ms): {Millisecond}, fps: {FPS}";
    private const string AVERAGE_UPS = "Avg. update (ms): {Millisecond}, ups: {UPS}";

    private const string MAIN_LOOP_RENDERING_SURFACE_NOT_FOUND = "Rendering surface {SurfaceId} not found!";

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = SERVICE_IS_STARTING)]
    internal static partial void LogServiceIsStarting(
        this ILogger logger,
        string serviceName,
        string version);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = SERVICE_IS_WORKING)]
    internal static partial void LogServiceIsWorking(
        this ILogger logger,
        string serviceName,
        string version);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = SERVICE_IS_STOPPING)]
    internal static partial void LogServiceStopping(
        this ILogger logger,
        string serviceName);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = SERVICE_FAILED_WITH_ERROR)]
    private static partial void LogServiceFailedWithError(
        this ILogger logger,
        Exception exception,
        string serviceName,
        string errorMessage);

    internal static void LogServiceFailedWithError(
        this ILogger logger,
        Exception exception,
        string serviceName)
        => logger.LogServiceFailedWithError(
            exception,
            serviceName,
            exception.Message);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = AVERAGE_FPS)]
    internal static partial void LogAverageSurfaceFps(
        this ILogger logger,
        RenderingSurfaceId surfaceId,
        float millisecond,
        ulong fps);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = AVERAGE_UPS)]
    internal static partial void LogAverageUps(
        this ILogger logger,
        float millisecond,
        ulong ups);

    internal static void LogMainLoopFailedWithError(
        this ILogger<MainLoopService> logger,
        Exception exception)
        => logger.LogServiceFailedWithError(
            exception,
            nameof(MainLoopService),
            exception.Message);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = MAIN_LOOP_RENDERING_SURFACE_NOT_FOUND)]
    internal static partial void LogRenderingSurfaceNotFound(
        this ILogger<MainLoopService> logger,
        RenderingSurfaceId surfaceId);
}