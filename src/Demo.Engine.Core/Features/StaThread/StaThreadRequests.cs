// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;

namespace Demo.Engine.Core.Features.StaThread;

internal abstract record StaThreadRequests
{
    public static CreateSurfaceRequest CreateSurface(
        IRenderingEngine renderingEngine)
        => new(renderingEngine);

    public abstract ValueTask<bool> InvokeAsync(
            CancellationToken cancellationToken = default);

    internal abstract record StaThreadWorkInner<TResult>
        : StaThreadRequests
    {
        private static TaskCompletionSource<TResult> Empty { get; }
        private TaskCompletionSource<TResult> _tcs;
        private CancellationTokenRegistration? _tcsRegistration;

        static StaThreadWorkInner()
        {
            Empty = new();
            Empty.SetResult(default!);
        }

        protected StaThreadWorkInner(
            bool startCompleted = false)
            => _tcs = startCompleted
                ? Empty
                : new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<TResult> Invoked
            => _tcs.Task;

        protected abstract ValueTask<TResult> InvokeFuncInternalAsync(
            CancellationToken cancellationToken = default);

        public override async ValueTask<bool> InvokeAsync(
            CancellationToken cancellationToken = default)
        {
            if (Invoked.IsCompleted)
            {
                return Invoked.IsCompletedSuccessfully;
            }

            try
            {
                var returnValue = await InvokeFuncInternalAsync(cancellationToken);
                _ = _tcs.TrySetResult(returnValue);
                _ = _tcsRegistration?.Unregister();
                return true;
            }
            catch (Exception ex)
            {
                _ = _tcsRegistration?.Unregister();
                _ = _tcs.TrySetException(ex);
            }
            return false;
        }

        protected void Reset(
            CancellationToken cancellationToken)
        {
            if (!_tcs.Task.IsCompleted)
            {
                _ = _tcs.TrySetException(
                    new TaskCanceledException("Message reset!"));
            }
            _ = _tcsRegistration?.Unregister();
            _tcsRegistration = null;
            _tcs = new TaskCompletionSource<TResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _tcsRegistration = cancellationToken.Register(()
                => _tcs.TrySetCanceled());
        }

        protected void ResetCompleted()
        {
            if (!_tcs.Task.IsCompleted)
            {
                _ = _tcs.TrySetException(
                    new TaskCanceledException("Message reset!"));
            }
            _ = _tcsRegistration?.Unregister();
            _tcsRegistration = null;
            _tcs = Empty;
        }
    }

    internal sealed record CreateSurfaceRequest(
        IRenderingEngine RenderingEngine)
        : StaThreadWorkInner<RenderingSurfaceId>
    {
        protected override async ValueTask<RenderingSurfaceId> InvokeFuncInternalAsync(
            CancellationToken cancellationToken = default)
            => await RenderingEngine.CreateSurfaceAsync(cancellationToken);
    }
}