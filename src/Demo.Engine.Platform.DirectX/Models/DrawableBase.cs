using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX.Interfaces;
using Demo.Tools.Common.Extensions.LockSlim;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX.Models
{
    public abstract class DrawableBase<T> : IDrawable, IDisposable
        where T : DrawableBase<T>
    {
        private bool _disposedValue;
        protected readonly ID3D11RenderingEngine _renderingEngine;
        private readonly Guid _drawableGuid;
        private static readonly ReaderWriterLockSlim _lockSlim = new();
        private static ReadOnlyCollection<IBindable>? _bindables;

        private static ReadOnlyDictionary<Type, IUpdatable>? _updatables;

        private static readonly HashSet<Guid> _references = new();

        protected static int? IndexCount { get; private set; }

        internal DrawableBase(
            ID3D11RenderingEngine renderingEngine,
            Func<T, IBindable[]> func)
        {
            _renderingEngine = renderingEngine;
            //Add reference
            _drawableGuid = Guid.NewGuid();
            _ = _lockSlim.EnterWriteLockBlock(() =>
            {
                _ = _references.Add(_drawableGuid);
                if (_bindables?.Any() != true)
                {
                    var (bindables, updatables) = DrawableBase<T>.BuildBindableUpdatableLists(func((T)this));
                    _bindables = bindables;
                    _updatables = updatables;
                }
            });
        }

        protected abstract void UpdateUpdatables();

        public virtual void Draw()
        {
            UpdateUpdatables();

            if (_bindables is object)
            {
                foreach (var bindable in _bindables)
                {
                    bindable.Bind();
                }
            }

            //viewport
            _renderingEngine.DeviceContext.RSSetViewport(new Viewport(
                _renderingEngine.Control.DrawingArea.X,
                _renderingEngine.Control.DrawingArea.Y,
                _renderingEngine.Control.DrawingArea.Width,
                _renderingEngine.Control.DrawingArea.Height,
                0, 1));

            _renderingEngine.DeviceContext.DrawIndexed(
                indexCount: IndexCount ?? throw new ArgumentException("No index buffer!"),
                startIndexLocation: 0,
                baseVertexLocation: 0);
        }

#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.

        internal static TUpdatable GetUpdatable<TUpdatable>()
            where TUpdatable : IUpdatable =>
                _updatables is object
                && _updatables.TryGetValue(typeof(TUpdatable), out var u)
                && u is TUpdatable updatable
            ? updatable
            : throw new ArgumentException("Type not found!");

#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.

        private static (ReadOnlyCollection<IBindable> bindables, ReadOnlyDictionary<Type, IUpdatable> updatables) BuildBindableUpdatableLists(params IBindable[] bindables)
        {
            if (bindables?.Any() != true)
            {
                return (
                    new ReadOnlyCollection<IBindable>(Array.Empty<IBindable>()),
                    new ReadOnlyDictionary<Type, IUpdatable>(new Dictionary<Type, IUpdatable>()));
            }

            var bindableList = new ReadOnlyCollectionBuilder<IBindable>();
            var updatableList = new Dictionary<Type, IUpdatable>();

            foreach (var bindable in bindables)
            {
                bindableList.Add(bindable);
                if (bindable is IUpdatable u)
                {
                    updatableList.Add(u.GetType(), u);
                }
                if (bindable is IIndexBuffer ib)
                {
                    IndexCount = IndexCount is null
                        ? ib.IndexCount
                        : throw new ArgumentException("Cannot add multiple index buffers!");
                }
            }

            return (bindableList.ToReadOnlyCollection(), new ReadOnlyDictionary<Type, IUpdatable>(updatableList));
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _ = _lockSlim.EnterUpgradableReadLockBlock(
                        lockSlim =>
                        {
                            if (!_references.Remove(_drawableGuid))
                            {
                                throw new InvalidOperationException("Missing reference!");
                            }
                            _ = lockSlim.IfActionEnterWriteLockBlock(
                                () => _references.Count == 0,
                                () =>
                                {
                                    if (_bindables is not null)
                                    {
                                        foreach (var disposable in _bindables.OfType<IDisposable>())
                                        {
                                            disposable.Dispose();
                                        }
                                    }

                                    _bindables = null;
                                    _updatables = null;
                                    IndexCount = null;
                                });
                        });
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