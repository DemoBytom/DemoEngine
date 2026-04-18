// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Drawing;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Interfaces.Platform;

public readonly ref struct RenderingControlSizeEventArgs
{
    private readonly ref readonly Width _width;
    private readonly ref readonly Height _height;

    public readonly ref readonly Width Width => ref _width;
    public readonly ref readonly Height Height => ref _height;
    public RenderingControlSizeEventArgs(
        ref readonly Width drawWidth,
        ref readonly Height drawHeight)
    {
        _width = ref drawWidth;
        _height = ref drawHeight;
    }
}

public interface IRenderingControl : IDisposable
{
    /// <summary>
    /// User resized event
    /// </summary>
    event EventHandler<RenderingControlSizeEventArgs>? UserResized;
    event EventHandler<EventArgs>? RenderingFormClosed;

    /// <summary>
    /// Displays the control to the user.
    /// </summary>
    void Show();

    IntPtr Handle { get; }

    bool IsDisposed { get; }

    bool IsFullscreen { get; }

    /// <summary>
    /// Width of the drawable area
    /// </summary>
    Width DrawWidth { get; }

    /// <summary>
    /// Height of the drawable area
    /// </summary>
    Height DrawHeight { get; }

    RectangleF DrawingArea { get; }

    public void SetFullscreen(bool isFullscreen);

    Task<T> InvokeAsync<T>(
        Func<CancellationToken, ValueTask<T>> callback,
        CancellationToken cancellationToken = default);

    ValueTask<RenderingSurfaceId> CreateSurface(
        IRenderingEngine renderingEngine,
        CancellationToken cancellationToken = default);
}