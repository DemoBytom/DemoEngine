// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using System.Threading.Channels;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Maths.Interop;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.Windows;

public sealed class WindowsMessagePump
    : BackgroundService
    , IDisposable
{
    private bool _disposedValue;
    private readonly Thread _thread;
    private readonly ILogger<WindowsMessagePump> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ChannelReader<StaThreadRequests> _channelReader;
    private readonly Task _staRequestHandler;

    private readonly TaskCompletionSource<Control> _marshallingControl = new(
        TaskCreationOptions.RunContinuationsAsynchronously);

    public WindowsMessagePump(
        ILogger<WindowsMessagePump> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        ChannelReader<StaThreadRequests> channelReader)
    {
        _thread = new Thread(ThreadInner)
        {
            IsBackground = false,
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Name = "Main STA thread";
        }
        else
        {
            throw new InvalidOperationException(
                "This is Windows Only pump!");
        }

        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;
        _channelReader = channelReader;
        _staRequestHandler = RunStaRequestHandler();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _thread.Start();

        return Task.CompletedTask;
    }

    private async Task RunStaRequestHandler()
    {
        var cancellationToken = _hostApplicationLifetime.ApplicationStopping;
        try
        {
            var marshallingControl = await _marshallingControl.Task.WaitAsync(cancellationToken);

            await foreach (var staAction in _channelReader
                .ReadAllAsync(cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: true)
                .WithCancellation(cancellationToken))

            {
                _ = await marshallingControl.InvokeAsync(
                    staAction.InvokeAsync,
                    cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {

        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }

    private unsafe void ThreadInner()
    {
        var cancellationToken = _hostApplicationLifetime.ApplicationStopping;
        try
        {
            using var marshallingControl = new MarshallingControl();
            _marshallingControl.SetResult(marshallingControl);

            using var tokenRegistration = cancellationToken.Register(
                () => marshallingControl.Invoke(
                    () => User32.PostQuitMessage(0)));

            NativeMessage msg;
            RawBool getMessageW;

            while (getMessageW = User32.GetMessageW(&msg, hWnd: IntPtr.Zero, wMsgFilterMin: 0, wMsgFilterMax: 0) == true
                /* cancellation token is required here in case an ESC is used to exit. 
                 * For now it doesn't seem to properly signal quit message? 
                 * Need to investigate why, because cancelling it should trigger User32.PostQuitMessage and end the pump..
                 * Maybe the StaThreadService is already down and cannot process it anymore? */
                //&& !cancellationToken.IsCancellationRequested
                )
            {
                if ((int)getMessageW == -1)
                {
                    _logger.LogErroInMainLoopProcessingWindowsMessages(
                        Marshal.GetLastWin32Error);

                    _hostApplicationLifetime.StopApplication();
                    return;
                }

                var message = Message.Create(
                    hWnd: msg.HWnd,
                    msg: (int)(nint)msg.Message,
                    wparam: (nint)msg.WParam,
                    lparam: msg.LParam);

                if (!Application.FilterMessage(ref message))
                {
                    _ = User32.TranslateMessage(&msg);
                    _ = User32.DispatchMessageW(&msg);
                }
            }
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }

    /// <summary>
    /// Marshalling control used to dispatch messages to the main STA thread that created it
    /// </summary>
    private sealed class MarshallingControl : Control
    {
#pragma warning disable IDE1006 // Naming Styles
        private static readonly IntPtr HWND_MESSAGE = new(-3);
#pragma warning restore IDE1006 // Naming Styles

        public MarshallingControl()
        {
            Visible = false;
            SetTopLevel(false);
            CreateControl();
            CreateHandle();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;

                // Message only windows are cheaper and have fewer issues than full blown invisible windows.
                cp.Parent = HWND_MESSAGE;
                return cp;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {

        }

        protected override void OnSizeChanged(EventArgs e)
        {
            // Don't do anything here -- small perf game of avoiding layout, etc.
        }
    }
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _thread.Join();
            }

            _disposedValue = true;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}