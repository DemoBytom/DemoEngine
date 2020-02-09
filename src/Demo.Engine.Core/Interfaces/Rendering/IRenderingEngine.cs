using System;

namespace Demo.Engine.Core.Interfaces.Rendering
{
    public interface IRenderingEngine : IDisposable
    {
        public bool DoEvents();
    }
}