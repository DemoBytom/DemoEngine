// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12;

internal static class RendererExtensions
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

        logger?.LogDebug("Created object {name}",
                name);

        d3D12Object.Name = name;
    }
}