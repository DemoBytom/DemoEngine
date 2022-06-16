// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.Models.Options;

public class RenderSettings
{
    /// <summary>
    /// Screen width
    /// </summary>
    public int Width { get; set; } = 1024;

    /// <summary>
    /// Screen height
    /// </summary>
    public int Height { get; set; } = 768;

    /// <summary>
    /// Should the application run in Fullscreen mode
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Use Vertical Sync
    /// </summary>
    public bool VSync { get; set; }

    /// <summary>
    /// Screen that should be used to display the application in fullscreen mode
    /// <para>0 is the primary screen</para>
    /// </summary>
    public byte Screen { get; set; } = 0;

    /// <summary>
    /// Can you resize the non-fullscreen window?
    /// </summary>
    public bool AllowResizing { get; set; }
}