// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Components.Keyboard;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Interfaces;

internal interface ILoopJob
{
    void Render(IRenderingEngine renderingEngine, RenderingSurfaceId renderingSurfaceId);

    ValueTask Update(IRenderingSurface renderingSurface, KeyboardHandle keyboardHandle, KeyboardCharCache keyboardCharCache);
}