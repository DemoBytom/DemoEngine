// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces.Rendering;
using Microsoft.Extensions.Hosting;

namespace Demo.Engine.Core.Interfaces;

/// <summary>
/// Main loop of the application. Handles dispatching of messages from OS, as well as calling
/// Update and Render functions
/// </summary>
public interface IMainLoopService
{
    /// <summary>
    /// Starts the main loop that periodically calls provided Update and Render methods
    /// </summary>
    /// <param name="updateCallback">Method that is called to update the current state</param>
    /// <param name="renderCallback">Method that is called to render the current state</param>
    /// <param name="cancellationToken">
    /// <see cref="CancellationToken"/> that can be used stop and finish the loop
    /// </param>
    /// <remarks>
    /// Loop can be stopped using <see cref="CancellationToken"/> or by invoking
    /// <see cref="IHostApplicationLifetime.StopApplication"/> method. Both will gracefully
    /// finish the loop and exit the Run method
    /// </remarks>
    Task RunAsync(
        Func<IRenderingSurface, KeyboardHandle, KeyboardCharCache, ValueTask> updateCallback,
        Func<IRenderingEngine, Guid, ValueTask> renderCallback,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Whether the loop is running
    /// </summary>
    bool IsRunning { get; }
}