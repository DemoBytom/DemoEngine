// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Vortice.DXGI;

namespace Demo.Engine.Platform.DirectX12;

internal static class Common
{
    public const ushort FRAME_BUFFER_COUNT = 3;

    public const ushort BACK_BUFFER_COUNT = 3;

    public const Format DEFAULT_BACK_BUFFER_FORMAT = Format.R8G8B8A8_UNorm_SRgb;

    /// <summary>
    /// Max amount of mip levels. Support up to 16k resolutions,
    /// </summary>
    public const ushort MAX_MIPS = 14;
}