// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Core.Exceptions;
using Demo.Engine.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;
using Vortice.DXGI.Debug;

namespace Demo.Engine.Platform.DirectX;

public sealed class DebugLayerLogger : IDebugLayerLogger, IDisposable
{
    private readonly ILogger<DebugLayerLogger> _logger;
    private readonly IDXGIInfoQueue? _dxgiInfoQueue;
    private readonly Guid _dxgiDebugGuid;
    private bool _disposedValue;

    public DebugLayerLogger(
        ILogger<DebugLayerLogger> logger)
    {
        _logger = logger;
        _dxgiDebugGuid = DXGI.DebugAll;
        _dxgiInfoQueue = DXGI.DXGIGetDebugInterface1<IDXGIInfoQueue>();

        if (_dxgiInfoQueue is not null)
        {
            _dxgiInfoQueue.PushEmptyStorageFilter(_dxgiDebugGuid);
            _dxgiInfoQueue.AddStorageFilterEntries(_dxgiDebugGuid, new Vortice.DXGI.Debug.InfoQueueFilter
            {
                AllowList = new Vortice.DXGI.Debug.InfoQueueFilterDescription
                {
                    Severities = new[]
                    {
                        InfoQueueMessageSeverity.Corruption,
                        InfoQueueMessageSeverity.Error,
                        InfoQueueMessageSeverity.Warning,
                    }
                }
            });
            _dxgiInfoQueue.SetBreakOnSeverity(_dxgiDebugGuid, InfoQueueMessageSeverity.Error, true);
            _dxgiInfoQueue.SetBreakOnSeverity(_dxgiDebugGuid, InfoQueueMessageSeverity.Corruption, true);
        }
    }

    public void LogMessages()
    {
        var stored = _dxgiInfoQueue?.GetNumStoredMessages(_dxgiDebugGuid) ?? 0;

        for (ulong i = 0; i < stored; ++i)
        {
            var message = _dxgiInfoQueue!.GetMessage(_dxgiDebugGuid, i);

            _logger.Log(
                SeverityToLogLevel(message.Severity),
                "[{@category}:{@id}] {@description}",
                    message.Category.ToString(),
                    ((MessageId)message.Id).ToString(),
                    UnterminateString(message.Description));
        }
        _dxgiInfoQueue?.ClearStoredMessages(_dxgiDebugGuid);
    }

    public DebugLayerMessage[] ReadMessages(ulong readFrom = 0)
    {
        var stored = _dxgiInfoQueue?.GetNumStoredMessages(_dxgiDebugGuid) ?? 0;
        var messages = stored - readFrom is var toRead and > 0
            ? new DebugLayerMessage[toRead]
            : Array.Empty<DebugLayerMessage>();

        for (var i = readFrom; i < stored; ++i)
        {
            var message = _dxgiInfoQueue!.GetMessage(_dxgiDebugGuid, i);

            var msg = UnterminateString(message.Description);

            messages[i] = new(
                Category: message.Category.ToString(),
                Id: ((MessageId)message.Id).ToString(),
                Message: msg,
                LogLevel: SeverityToLogLevel(message.Severity));
        }

        return messages;
    }

    public ulong MessageQueuePosition()
        => _dxgiInfoQueue?.GetNumStoredMessages(_dxgiDebugGuid) ?? 0;

    public T WrapCallInMessageExceptionHandler<T>(Func<T> func)
    {
        var pinned = MessageQueuePosition();
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            var messages = ReadMessages(pinned);
            throw new GraphicsException(
                messages.ElementAtOrDefault(0) is DebugLayerMessage msg
                    ? $"[{msg.Category}:{msg.Id}] {msg.Message}"
                    : "Error creating DirectX resource!",
                messages,
                ex);
        }
    }

    public void WrapCallInMessageExceptionHandler(Action act)
    {
        var pinned = MessageQueuePosition();
        try
        {
            act();
        }
        catch (Exception ex)
        {
            var messages = ReadMessages(pinned);
            throw new GraphicsException(
                messages.ElementAtOrDefault(0) is DebugLayerMessage msg
                    ? $"[{msg.Category}:{msg.Id}] {msg.Message}"
                    : "Error executing DirectXs action!",
                messages,
                ex);
        }
    }

    private static string UnterminateString(string message)
        => message.EndsWith('\0')
            ? message[0..^1]
            : message;

    private static LogLevel SeverityToLogLevel(InfoQueueMessageSeverity severity)
        => severity switch
        {
            InfoQueueMessageSeverity.Corruption => LogLevel.Critical,
            InfoQueueMessageSeverity.Error => LogLevel.Error,
            InfoQueueMessageSeverity.Warning => LogLevel.Warning,
            InfoQueueMessageSeverity.Info => LogLevel.Information,
            InfoQueueMessageSeverity.Message => LogLevel.Debug,
            _ => LogLevel.Trace,
        };

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _dxgiInfoQueue?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}