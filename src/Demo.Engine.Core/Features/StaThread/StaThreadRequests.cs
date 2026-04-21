// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.ObjectPool;

namespace Demo.Engine.Core.Features.StaThread;

public abstract record StaThreadRequests
{
    public static DoEventsOkRequest DoEventsOk(
        IRenderingEngine renderingEngine,
        IOSMessageHandler osMessageHandler,
        RenderingSurfaceId renderingSurfaceId,
        bool blockingCall)
        => new(renderingEngine, osMessageHandler, renderingSurfaceId, blockingCall);

    public static CreateSurfaceRequest CreateSurface(
        IRenderingEngine renderingEngine)
        => new(renderingEngine);

    public abstract ValueTask<bool> InvokeAsync(
            CancellationToken cancellationToken = default);

    public abstract record StaThreadWorkInner<TResult>
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
    public sealed record CreateSurfaceRequest(
        IRenderingEngine RenderingEngine)
        : StaThreadWorkInner<RenderingSurfaceId>
    {
        protected override async ValueTask<RenderingSurfaceId> InvokeFuncInternalAsync(
            CancellationToken cancellationToken = default)
            => await RenderingEngine.CreateSurfaceAsync(cancellationToken);
    }

    public sealed record DoEventsOkRequest
        : StaThreadWorkInner<bool>,
          IResettable
    {
        private IRenderingEngine _renderingEngine;
        private IOSMessageHandler _osMessageHandler;
        private RenderingSurfaceId _renderingSurfaceId;
        private bool _blockingCall;

        public DoEventsOkRequest()
            : this(
                  null!,
                  null!,
                  RenderingSurfaceId.Empty, blockingCall: false)
        {
        }

        internal DoEventsOkRequest(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
            RenderingSurfaceId renderingSurfaceId,
            bool blockingCall)
            : base(false)
            => (_renderingEngine, _osMessageHandler, _renderingSurfaceId, _blockingCall) = (renderingEngine, osMessageHandler, renderingSurfaceId, blockingCall);

        protected override ValueTask<bool> InvokeFuncInternalAsync(
            CancellationToken cancellationToken = default)
            => _renderingEngine.TryGetRenderingSurface(
                _renderingSurfaceId,
                out var renderingSurface)
            ? _blockingCall
                ? ValueTask.FromResult(_osMessageHandler.BlockingDoEvents(
                    renderingSurface.RenderingControl,
                    cancellationToken))
                : ValueTask.FromResult(_osMessageHandler.DoEvents(
                    renderingSurface.RenderingControl))
            : throw new InvalidOperationException("No RenderingSurface provided!");

        public void Reset(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler,
            RenderingSurfaceId renderingSurfaceId,
            bool blockingCall,
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
            _renderingEngine = renderingEngine;
            _osMessageHandler = osMessageHandler;
            _renderingSurfaceId = renderingSurfaceId;
            _blockingCall = blockingCall;
        }

        public bool TryReset()
        {
            if (Invoked.IsCompleted == false)
            {
                return false;
            }

            Reset(
                renderingEngine: null!,
                osMessageHandler: null!,
                RenderingSurfaceId.Empty, blockingCall: false, CancellationToken.None);

            return true;
        }
    }
}