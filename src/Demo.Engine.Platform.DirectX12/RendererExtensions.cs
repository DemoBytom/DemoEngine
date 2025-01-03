// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX12;

internal static partial class RendererExtensions
{
    public static void NameObject(
        this ID3D12Object? d3D12Object,
        string name,
        ILogger? logger = null)
    {
        if (d3D12Object is null)
        {
            return;
        }

        logger?.LogCreatedObject(name);

        d3D12Object.Name = name;

        if (logger?.IsEnabled(LogLevel.Trace) == true)
        {
            d3D12Object.Disposed += (_, _)
                => logger.LogDisposedObject(name);
        }
    }

    public static void NameObject(
        this IDXGIObject? dxgiObject,
        string name,
        ILogger? logger = null)
    {
        if (dxgiObject is null)
        {
            return;
        }

        logger?.LogCreatedObject(name);

        dxgiObject.DebugName = name;

        if (logger?.IsEnabled(LogLevel.Trace) == true)
        {
            dxgiObject.Disposed += (_, _)
                => logger.LogDisposedObject(name);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Created object {name}")]
    internal static partial void LogCreatedObject(
        this ILogger logger,
        string name);

    [LoggerMessage(
        Level = LogLevel.Trace,
        Message = "Disposed object {name}",
        SkipEnabledCheck = true)]
    internal static partial void LogDisposedObject(
        this ILogger logger,
        string name);
}