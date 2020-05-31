using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Platform.DirectX.Interfaces;
using Vortice.Mathematics;

namespace Demo.Engine.Platform.DirectX.Models
{
    public abstract class DrawableBase<T> : IDrawable, IDisposable
        where T : DrawableBase<T>
    {
        private bool _disposedValue;
        protected readonly ID3D11RenderingEngine _renderingEngine;

#pragma warning disable RCS1158 // Static member in generic type should use a type parameter.

        internal static ReadOnlyCollection<IBindable>? _bindables { get; private set; }

        internal static UpdatableCollection? _updatables { get; private set; }

#pragma warning restore RCS1158 // Static member in generic type should use a type parameter.

        protected static int IndexCount { get; private set; } = int.MinValue;

        internal DrawableBase(
            ID3D11RenderingEngine renderingEngine,
            Func<T, IBindable[]> func)
        {
            _renderingEngine = renderingEngine;
            if (_bindables?.Any() != true)
            {
                var (bindables, updatables) = BuildBindableUpdatableLists(func((T)this));
                _bindables = bindables;
                _updatables = updatables;
            }
        }

        protected abstract void UpdateUpdatables();

        public virtual void Draw()
        {
            UpdateUpdatables();

            foreach (var bindable in _bindables)
            {
                bindable.Bind();
            }

            //viewport
            _renderingEngine.DeviceContext.RSSetViewport(new Viewport(_renderingEngine.Control.DrawingArea)
            {
                MinDepth = 0,
                MaxDepth = 1
            });

            _renderingEngine.DeviceContext.DrawIndexed(IndexCount, 0, 0);
        }

        private (ReadOnlyCollection<IBindable> bindables, UpdatableCollection updatables) BuildBindableUpdatableLists(params IBindable[] bindables)
        {
            if (bindables?.Any() != true)
            {
                return (
                    new ReadOnlyCollection<IBindable>(Array.Empty<IBindable>()),
                    new UpdatableCollection());
            }

            var bindableList = new ReadOnlyCollectionBuilder<IBindable>();
            var updatableList = new UpdatableCollection();

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

            return (bindableList.ToReadOnlyCollection(), updatableList);
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

    internal class UpdatableCollection
    {
        private Dictionary<Type, IUpdatable> _dict = new Dictionary<Type, IUpdatable>();

        public int Count => _dict.Count;

        public bool IsReadOnly => false;

        public void Add<T>(IUpdatable<T> item)
        {
            if (item is null)
            {
                throw new NullReferenceException("Collection can't contain null values!");
            }

            _dict.Add(item.GetType(), item);
        }

        public void Add(IUpdatable item)
        {
            if (item is null)
            {
                throw new NullReferenceException("Collection can't contain null values!");
            }

            _dict.Add(item.GetType(), item);
        }

        public void Clear() => _dict.Clear();

        public bool Contains<T>(IUpdatable<T> item)
        {
            if (item is null)
            {
                throw new NullReferenceException("Collection can't contain null values!");
            }
            return _dict.ContainsKey(item.GetType());
        }

        public bool Remove<T>(IUpdatable<T> item)
        {
            if (item is null)
            {
                throw new NullReferenceException("Collection can't contain null values!");
            }

            return _dict.Remove(item.GetType());
        }

        public T GetUpdatable<T>()
            where T : class
        {
            return _dict[typeof(T)] as T ?? throw new ArgumentException("Type not found!");
        }
    }
}