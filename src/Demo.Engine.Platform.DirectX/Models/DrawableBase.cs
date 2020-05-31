using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Direct3D;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX.Models
{
    public abstract class DrawableBase : IDrawable, IDisposable
    {
        private bool _disposedValue;
        protected readonly ID3D11RenderingEngine _renderingEngine;
        internal ReadOnlyCollection<IBindable> _bindables { get; private set; }
        internal ReadOnlyCollection<IUpdatable> _updatables { get; private set; }

        protected int IndexCount { get; private set; } = int.MinValue;

        protected DrawableBase(
            ID3D11RenderingEngine renderingEngine)
        {
            _renderingEngine = renderingEngine;

            _bindables = new ReadOnlyCollection<IBindable>(Array.Empty<IBindable>());
            _updatables = new ReadOnlyCollection<IUpdatable>(Array.Empty<IUpdatable>());
        }

        public virtual void Draw()
        {
            foreach (var bindable in _bindables)
            {
                bindable.Bind();
            }

            _renderingEngine.DeviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);

            //viewport
            _renderingEngine.DeviceContext.RSSetViewport(new Viewport(_renderingEngine.Control.DrawingArea)
            {
                MinDepth = 0,
                MaxDepth = 1
            });

            _renderingEngine.DeviceContext.DrawIndexed(IndexCount, 0, 0);
        }

        internal void BuildBindableUpdatableLists(params IBindable[] bindables)
        {
            if (bindables?.Any() != true)
            {
                return;
            }
            var bindableList = new ReadOnlyCollectionBuilder<IBindable>();
            var updatableList = new ReadOnlyCollectionBuilder<IUpdatable>();
            foreach (var bindable in bindables)
            {
                bindableList.Add(bindable);
                if (bindable is IUpdatable u)
                {
                    updatableList.Add(u);
                }
                if (bindable is IIndexBuffer ib)
                {
                    IndexCount = IndexCount == int.MinValue
                        ? ib.IndexCount
                        : throw new ArgumentException("Cannot add multiple index buffers!");
                }
            }

            _bindables = bindableList.ToReadOnlyCollection();
            _updatables = updatableList.ToReadOnlyCollection();
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var disposable in _bindables.OfType<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}