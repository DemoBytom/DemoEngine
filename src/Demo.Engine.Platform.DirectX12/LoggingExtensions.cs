// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Models.Enums;
using Demo.Engine.Core.ValueObjects;
using Demo.Engine.Platform.DirectX12.RenderingResources;
using Demo.Engine.Platform.DirectX12.Shaders;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D;

namespace Demo.Engine.Platform.DirectX12;

internal static partial class LoggingExtensions
{
    private const string CREATED_DEVICE_SUPPORTING_FEATURE_LEVEL = "Created device supporting {FeatureLevel}";
    private const string DEBUG_LAYER_MESSAGE = "[{@category}:{@id}] {@description}";

    private const string DISPOSING_RENDERING_SURFACE = "Disposing Rendering Surface {SurfaceId}";
    private const string DISPOSED_RENDERING_SURFACE = "Disposed Rendering Surface {SurfaceId}";
    private const string DISPOSING_RTV = "Disposing RTV {RtvId}";
    private const string DISPOSED_RTV = "Disposed RTV {RtvId}";

    private const string FAILED_TO_CREATE_DESCRIPTOR_HEAPS = "Failed to create descriptor heaps: {ErrorMessage}";
    private const string ERROR_DISPOSING_RENDERING_ENGINE = "Error disposing Rendering Engine!";
    private const string DESTROYED_ENGINE = "Destroyed engine!";

    private const string ATTEMPTING_SHADER_COMPILATION = "Attempting {ShaderFilePath} shader compliation";
    private const string COMPILING_SHADER = "Compiling {ShaderStage} {ShaderFilePath} with {ShaderProfile}";
    private const string FATAL_ERROR_WHEN_COMPILING = "Fatal error when compiling {ShaderFilePath}!";

    private const string LOG_INVALID_SRV_DESCRIPTOR = "Invalid SRV descriptor!";
    private const string NOT_ENOUGH_DATA_TO_READ_32BIT_INT = "Not enough data to read 32-bit integer!";
    private const string UNEXPECTED_END_OF_STREAM = "Unexpected end of stream!";

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = CREATED_DEVICE_SUPPORTING_FEATURE_LEVEL)]
    internal static partial void LogCreatedDeviceSupportingFeatureLevel(
        this ILogger<D3D12RenderingEngine> logger,
        FeatureLevel featureLevel);

    [LoggerMessage(
        Message = DEBUG_LAYER_MESSAGE)]
    internal static partial void LogDebugLayerMessage(
        this ILogger<DebugLayerLogger> logger,
        LogLevel level,
        string @category,
        string @id,
        string @description);

    [LoggerMessage(
        Message = DEBUG_LAYER_MESSAGE,
        EventName = nameof(LogDebugLayerMessage),
        SkipEnabledCheck = true)]
    internal static partial void LogDebugLayerMessageNoCheck(
        this ILogger<DebugLayerLogger> logger,
        LogLevel level,
        string @category,
        string @id,
        string @description);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = DISPOSING_RENDERING_SURFACE)]
    internal static partial void LogDisposingRenderingSurface(
        this ILogger<RenderingSurface> logger,
        RenderingSurfaceId surfaceId);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = DISPOSED_RENDERING_SURFACE)]
    internal static partial void LogDisposedRenderingSurface(
        this ILogger<RenderingSurface> logger,
        RenderingSurfaceId surfaceId);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = DISPOSING_RTV)]
    internal static partial void LogDisposingRTV(
        this ILogger<RenderingSurface> logger,
        int rtvId);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = DISPOSED_RTV)]
    internal static partial void LogDisposedRTV(
        this ILogger<RenderingSurface> logger,
        int rtvId);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = FAILED_TO_CREATE_DESCRIPTOR_HEAPS)]
    internal static partial void LogFailedToCreateDescriptorHeaps(
        this ILogger<D3D12RenderingEngine> logger,
        string errorMessage);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = ERROR_DISPOSING_RENDERING_ENGINE)]
    internal static partial void LogErrorDisposingRenderingEngine(
        this ILogger<D3D12RenderingEngine> logger,
        Exception ex);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = DESTROYED_ENGINE)]
    internal static partial void LogDestroyedEngine(
        this ILogger<D3D12RenderingEngine> logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = ATTEMPTING_SHADER_COMPILATION)]
    internal static partial void LogCompilingShaderFile(
        this ILogger<ShaderCompiler> logger,
        string shaderFilePath);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = COMPILING_SHADER)]
    internal static partial void LogCompilingShaderFile(
        this ILogger<ShaderCompilerOld> logger,
        ShaderStage shaderStage,
        string shaderFilePath,
        string shaderProfile);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = FATAL_ERROR_WHEN_COMPILING)]
    internal static partial void LogErrorCompilingShader(
        this ILogger<ShaderCompiler> logger,
        Exception exception,
        string shaderFilePath);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = LOG_INVALID_SRV_DESCRIPTOR)]
    internal static partial void LogInvalidSrvDescriptor(
        this ILogger<Texture> logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = NOT_ENOUGH_DATA_TO_READ_32BIT_INT)]
    internal static partial void LogNotEnoughDataToRead32BitInteger(
        this ILogger<EngineShaderManager> logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = UNEXPECTED_END_OF_STREAM)]
    internal static partial void LogUnexpectedEndOfStream(
        this ILogger<EngineShaderManager> logger);
}