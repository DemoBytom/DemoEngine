// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.ObjectPool;

namespace Demo.Engine.Core.Features.StaThread;

internal abstract record StaThreadRequests
{
    public static DoEventsOkRequest DoEventsOk(
        RenderingSurfaceId renderingSurfaceId)
        => new(renderingSurfaceId);

    public static CreateSurfaceRequest CreateSurface()
        => new();

    public abstract ValueTask<bool> InvokeAsync(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
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
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
            CancellationToken cancellationToken = default);

        public override async ValueTask<bool> InvokeAsync(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
            CancellationToken cancellationToken = default)
        {
            if (Invoked.IsCompleted)
            {
                return Invoked.IsCompletedSuccessfully;
            }

            try
            {
                var returnValue = await InvokeFuncInternalAsync(renderingEngine, osMessageHandler, cancellationToken);
                _tcs.SetResult(returnValue);
                _ = _tcsRegistration?.Unregister();
                return true;
            }
            catch (Exception ex)
            {
                _ = _tcsRegistration?.Unregister();
                _tcs.SetException(ex);
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
                => _tcs.SetCanceled());
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
    internal sealed record CreateSurfaceRequest
        : StaThreadWorkInner<RenderingSurfaceId>
    {
        protected override async ValueTask<RenderingSurfaceId> InvokeFuncInternalAsync(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
            CancellationToken cancellationToken = default)
            => await renderingEngine.CreateSurfaceAsync(cancellationToken);
    }

    internal sealed record DoEventsOkRequest
        : StaThreadWorkInner<bool>,
          IResettable
    {
        private RenderingSurfaceId _renderingSurfaceId;

        public DoEventsOkRequest()
            : this(RenderingSurfaceId.Empty)
        {

        }

        internal DoEventsOkRequest(
            RenderingSurfaceId renderingSurfaceId)
            : base(false)
            => _renderingSurfaceId = renderingSurfaceId;

        protected override ValueTask<bool> InvokeFuncInternalAsync(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
            CancellationToken cancellationToken = default)
            => renderingEngine.TryGetRenderingSurface(
                _renderingSurfaceId,
                out var renderingSurface)
            ? ValueTask.FromResult(osMessageHandler.DoEvents(
                renderingSurface.RenderingControl))
            : throw new InvalidOperationException("No RenderingSurface provided!");

        public void Reset(
            RenderingSurfaceId renderingSurfaceId,
            CancellationToken cancellationToken)
        {
            if (renderingSurfaceId == RenderingSurfaceId.Empty)
            {
                ResetCompleted();
            }
            else
            {
                Reset(cancellationToken);
            }
            _renderingSurfaceId = renderingSurfaceId;
        }

        public bool TryReset()
        {
            if (Invoked.IsCompleted == false)
            {
                return false;
            }

            Reset(RenderingSurfaceId.Empty, CancellationToken.None);

            return true;
        }
    }
}