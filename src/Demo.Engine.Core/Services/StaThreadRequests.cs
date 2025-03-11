// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Threading.Channels;
using Demo.Engine.Core.Interfaces.Platform;
using Demo.Engine.Core.Interfaces.Rendering;
using Demo.Engine.Core.ValueObjects;
using Microsoft.Extensions.ObjectPool;

namespace Demo.Engine.Core.Services;

internal static class StaThreadWorkExtensions
{
    public static async ValueTask<bool> DoEventsOk(
        this ChannelWriter<StaThreadRequests> channelWriter,
        RenderingSurfaceId renderingSurfaceId,
        StaThreadRequests.DoEventsOkRequest? doEventsOkRequest = null,
        CancellationToken cancellationToken = default)
    {
        doEventsOkRequest?.Reset(renderingSurfaceId, cancellationToken);

        var request = doEventsOkRequest
            ?? StaThreadRequests.DoEventsOk(renderingSurfaceId);

        await channelWriter.WriteAsync(
            request,
            cancellationToken);

        return await request.Invoked;
    }

    public static async ValueTask<RenderingSurfaceId> CreateSurface(
        this ChannelWriter<StaThreadRequests> channelWriter,
        CancellationToken cancellationToken = default)
    {
        var createSurfaceRequest = StaThreadRequests.CreateSurface();
        await channelWriter.WriteAsync(
                createSurfaceRequest,
                cancellationToken);

        return await createSurfaceRequest.Invoked;
    }
}

internal abstract record StaThreadRequests
{
    public static DoEventsOkRequest DoEventsOk(
        RenderingSurfaceId renderingSurfaceId)
        => new(renderingSurfaceId);

    public static CreateSurfaceRequest CreateSurface()
        => new();

    public abstract bool Invoke(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler);

    internal abstract record StaThreadWorkInner<TResult>
        : StaThreadRequests
    {
        private TaskCompletionSource<TResult> _tcs = new();
        private CancellationTokenRegistration? _tcsRegistration;

        public Task<TResult> Invoked => _tcs.Task;

        protected abstract TResult InvokeFuncInternal(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler);

        public override bool Invoke(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler)
        {
            if (Invoked.IsCompleted)
            {
                return Invoked.IsCompletedSuccessfully;
            }

            try
            {
                var returnValue = InvokeFuncInternal(renderingEngine, osMessageHandler);
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
            _tcs = new TaskCompletionSource<TResult>();
            _tcsRegistration = cancellationToken.Register(()
                => _tcs.SetCanceled());
        }
    }
    internal sealed record CreateSurfaceRequest
        : StaThreadWorkInner<RenderingSurfaceId>
    {
        protected override RenderingSurfaceId InvokeFuncInternal(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler)
            => renderingEngine.CreateSurface();
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
            => _renderingSurfaceId = renderingSurfaceId;

        protected override bool InvokeFuncInternal(
            IRenderingEngine renderingEngine,
            IOSMessageHandler osMessageHandler)
            => renderingEngine.TryGetRenderingSurface(
                _renderingSurfaceId,
                out var renderingSurface)
            ? osMessageHandler.DoEvents(
                renderingSurface.RenderingControl)
            : throw new InvalidOperationException("No RenderingSurface provided!");

        public void Reset(
            RenderingSurfaceId renderingSurfaceId,
            CancellationToken cancellationToken)
        {
            Reset(cancellationToken);
            _renderingSurfaceId = renderingSurfaceId;
        }

        /* TODO: Every time this is reset here
         * it creates new TaskCompletionSource
         * It should be optimized to only create it if it's reset using the Reset method
         * Otherwise it should just have a precomputed Empty value, or something */
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