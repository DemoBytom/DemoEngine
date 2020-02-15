using System;
using Demo.Engine.Core.Interfaces.Platform;

namespace Demo.Engine.Core.Interfaces.Rendering
{
    public interface IRenderingEngine : IDisposable
    {
        IRenderingControl Control { get; }
    }
}