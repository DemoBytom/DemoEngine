using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Demo.Tools.Common.Collections
{
    public class CircularQueue<T> : IReadOnlyCollection<T>, ICollection
    {
        private readonly int _capacity;
        private readonly T[] _buffer;
        private int _tail;
        private int _head;

        public CircularQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new InvalidOperationException($"{nameof(capacity)} cannot be negative!");
            }

            _capacity = capacity;
            _buffer = new T[capacity];
        }

        public void Enqueue(T value)
        {
            if (_tail == _head && Count != 0)
            {
                IncreaseAddres(ref _head);
            }
            else
            {
                ++Count;
            }

            _buffer[_tail] = value;

            IncreaseAddres(ref _tail);
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty!");
            }

            var item = _buffer[_head];
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                _buffer[_head] = default!;
            }
            IncreaseAddres(ref _head);
            --Count;
            return item;
        }

        public bool TryDequeue([MaybeNullWhen(false)] out T result)
        {
            if (Count == 0)
            {
                result = default!;
                return false;
            }
            result = Dequeue();
            return true;
        }

        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Queue is empty!");
            }
            return _buffer[_head];
        }

        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            if (Count == 0)
            {
                result = default!;
                return false;
            }

            result = Peek();
            return true;
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <returns>The number of elements in the collection.</returns>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection"></see> is
        /// synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="ICollection"></see> is synchronized (thread safe);
        /// otherwise, false.
        /// </returns>
        public bool IsSynchronized => false;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"></see>.
        /// </summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="ICollection"></see>.</returns>
        object ICollection.SyncRoot => this;

        /// <summary>
        /// Copies the elements of the <see cref="CircularQueue{T}"></see> to an
        /// <see cref="Array"></see>, starting at a particular <see cref="Array"></see> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"></see> that is the destination of the elements
        /// copied from <see cref="CircularQueue{T}"></see>. The <see cref="Array"></see> must have
        /// zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array">array</paramref> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index">index</paramref> is less than zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="array">array</paramref> is multidimensional. -or- The number of elements
        /// in the source <see cref="CircularQueue{T}"></see> is greater than the available space
        /// from <paramref name="index">index</paramref> to the end of the destination
        /// <paramref name="array">array</paramref>. -or- The type of the source
        /// <see cref="CircularQueue{T}"></see> cannot be cast automatically to the type of the
        /// destination <paramref name="array">array</paramref>.
        /// </exception>
        public void CopyTo(T[] array, int index)
        {
            var head = _head;
            for (var i = 0; i < Count;
                ++i,
                IncreaseAddres(ref head),
                ++index)
            {
                array[index] = _buffer[head];
            }
        }

        private IEnumerator<T> GetEnum()
        {
            var head = _head;
            for (var i = 0; i < Count; ++i, IncreaseAddres(ref head))
            {
                yield return _buffer[head];
            }
        }

        public IEnumerator<T> GetEnumerator() => GetEnum();

        IEnumerator IEnumerable.GetEnumerator() => GetEnum();

        /// <summary>
        /// Copies the elements of the <see cref="CircularQueue{T}"></see> to an
        /// <see cref="Array"></see>, starting at a particular <see cref="Array"></see> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"></see> that is the destination of the elements
        /// copied from <see cref="CircularQueue{T}"></see>. The <see cref="Array"></see> must have
        /// zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array">array</paramref> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index">index</paramref> is less than zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="array">array</paramref> is multidimensional. -or- The number of elements
        /// in the source <see cref="CircularQueue{T}"></see> is greater than the available space
        /// from <paramref name="index">index</paramref> to the end of the destination
        /// <paramref name="array">array</paramref>. -or- The type of the source
        /// <see cref="CircularQueue{T}"></see> cannot be cast automatically to the type of the
        /// destination <paramref name="array">array</paramref>.
        /// </exception>
        void ICollection.CopyTo(Array array, int index) => CopyTo((T[])array, index);

        private void IncreaseAddres(ref int adr) => adr = (adr + 1) % _capacity;
    }
}