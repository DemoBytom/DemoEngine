using System;
using System.Collections.Generic;
using Demo.Engine.Core.Interfaces.Platform;
using Vortice.Mathematics;

namespace Demo.Engine.Core.Interfaces.Rendering
{
    public interface IRenderingEngine : IDisposable
    {
        IRenderingControl Control { get; }

        void BeginScene(Color4 color);

        bool EndScene();

        void BeginScene();

        void Draw(IEnumerable<IDrawable> drawables);
    }
}