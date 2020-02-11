using System;
using Demo.Engine.Core.Platform;

namespace Demo.Engine.Core.Interfaces.Rendering
{
    public interface IRenderingEngine : IDisposable
    {
        IRenderingControl Control { get; }
    }
}