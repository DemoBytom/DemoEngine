// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Demo.Tools.Common.Collections;

public class CircularQueue<T> : IReadOnlyCollection<T>, ICollection
{
    private readonly int _capacity;
    private readonly ConcurrentQueue<T> _buffer;

    private readonly ReaderWriterLockSlim _lock = new();

    public CircularQueue(int capacity)
    {
        if (capacity < 0)
        {
            throw new InvalidOperationException($"{nameof(capacity)} cannot be negative!");
        }

        _capacity = capacity;
        _buffer = new ConcurrentQueue<T>();
    }

    /// <summary>
    /// Enqueues new element onto the queue, removing the oldest one if capacity was reached
    /// </summary>
    /// <param name="value"></param>
    public void Enqueue(T value)
    {
        _lock.EnterWriteLock();
        try
        {
            if (_buffer.Count == _capacity)
            {
                _ = _buffer.TryDequeue(out _);
            }

            _buffer.Enqueue(value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Dequeues top element from the queue
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If can't dequeue</exception>
    public T Dequeue()
    {
        _lock.EnterWriteLock();
        try
        {
            return _buffer.TryDequeue(out var retVal)
                ? retVal
                : throw new InvalidOperationException("Dequeue failed!");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Tries to dequeue top element from the queue
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryDequeue([MaybeNullWhen(false)] out T result) =>
        _buffer.TryDequeue(out result);

    /// <summary>
    /// Returns top element from the queue without dequeuing it
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">If can't peek</exception>
    public T Peek()
        => TryPeek(out var retVal)
            ? retVal
            : throw new InvalidOperationException("Peek failed!");

    /// <summary>
    /// Tries to return top element from the queue without dequeuing it
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        _lock.EnterReadLock();
        try
        {
            return _buffer.TryPeek(out result);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets the number of elements in the collection.
    /// </summary>
    /// <returns>The number of elements in the collection.</returns>
    public int Count => _buffer.Count;

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
        _lock.EnterReadLock();
        try
        {
            _buffer.CopyTo(array, index);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public IEnumerator<T> GetEnumerator() => _buffer.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _buffer.GetEnumerator();

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

    ~CircularQueue() => _lock?.Dispose();
}