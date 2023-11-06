// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DXGI;
using Vortice.DXGI.Debug;

namespace Demo.Engine.Platform.DirectX12;

public sealed class DebugLayerLogger
    : IDebugLayerLogger,
      IDisposable
{
    private bool _disposedValue;
    private readonly ILogger<DebugLayerLogger> _logger;
    private readonly Guid _dxgiGuid;
    private readonly IDXGIDebug1? _dxgiDebug;
    private readonly ID3D12Debug6? _d3d12Debug;
    private readonly IDXGIInfoQueue? _dxgiInfoQueue;

    public DebugLayerLogger(
        ILogger<DebugLayerLogger> logger)
    {
        _logger = logger;
        _dxgiGuid = DXGI.DebugAll;

        if (D3D12.D3D12GetDebugInterface(out _d3d12Debug)
            .Success)
        {
            _d3d12Debug!.EnableDebugLayer();

            if (DXGI.DXGIGetDebugInterface1(out _dxgiDebug)
                .Success)
            {
                _dxgiDebug!.EnableLeakTrackingForThread();
            }

            _dxgiInfoQueue = DXGI.DXGIGetDebugInterface1<IDXGIInfoQueue>();
        }
    }

    public ulong MessageQueuePosition()
        => 0;

    public DebugLayerMessage[] ReadMessages(
        ulong readFrom = 0)
    {
        var stored = _dxgiInfoQueue?.GetNumStoredMessages(_dxgiGuid) ?? 0;

        var messages = stored - readFrom
            is var toRead and > 0
            ? new DebugLayerMessage[toRead]
            : Array.Empty<DebugLayerMessage>();

        for (var i = readFrom; i < stored; ++i)
        {
            var message = _dxgiInfoQueue!.GetMessage(_dxgiGuid, i);

            messages[i] = new(
                message.Category.ToString(),
                Id: ((MessageId)message.Id).ToString(),
                Message: message.Description.UnterminateString());
        }

        return messages;
    }

    public T WrapCallInMessageExceptionHandler<T>(
        Func<T> func)
        => func();

    public void WrapCallInMessageExceptionHandler(
        Action act)
        => act();

    public void LogMessages()
    {
        var stored = _dxgiInfoQueue?.GetNumStoredMessages(_dxgiGuid) ?? 0;

        for (
            ulong i = 0;
            i < stored;
            ++i)
        {
            var message = _dxgiInfoQueue!.GetMessage(_dxgiGuid, i);

            _logger.Log(
                message.Severity.ToLogLevel(),
                "[{@category}:{@id}] {@description}",
                    message.Category.ToString(),
                    ((MessageId)message.Id).ToString(),
                    message.Description.UnterminateString());
        }

        _dxgiInfoQueue?.ClearStoredMessages(_dxgiGuid);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                if (_dxgiDebug is not null)
                {
                    _dxgiDebug!.ReportLiveObjects(
                        apiid: _dxgiGuid,
                        flags: ReportLiveObjectFlags.Detail
                             | ReportLiveObjectFlags.IgnoreInternal);

                    LogMessages();
                }

                _d3d12Debug?.Dispose();
                _dxgiDebug?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DebugLayerLogger()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

internal static class DebugLayerLoggerExtensions
{
    internal static LogLevel ToLogLevel(
        this InfoQueueMessageSeverity severity)
        => severity switch
        {
#pragma warning disable IDE0055 //Allow custom formatting
            InfoQueueMessageSeverity.Corruption => LogLevel.Critical,
            InfoQueueMessageSeverity.Error      => LogLevel.Error,
            InfoQueueMessageSeverity.Warning    => LogLevel.Warning,
            InfoQueueMessageSeverity.Info       => LogLevel.Information,
            InfoQueueMessageSeverity.Message    => LogLevel.Debug,
            _                                   => LogLevel.Trace,
#pragma warning restore IDE0055 //Allow custom formatting
        };

    internal static string UnterminateString(
        this string message)
        => message.EndsWith('\0')
            ? message[0..^1]
            : message;
}