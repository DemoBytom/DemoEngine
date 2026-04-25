// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Runtime.InteropServices;
using Demo.Engine.Core.Features.StaThread;
using Demo.Engine.Core.Maths.Interop;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Demo.Engine.Platform.Windows;

public sealed class WindowsMessagePump
    : BackgroundService
    , IAsyncDisposable
{
    private bool _disposedValue;
    private readonly Thread _thread;
    private readonly ILogger<WindowsMessagePump> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IStaThreadReader _staThreadReader;
    private readonly CancellationTokenSource _cancelStaRequestHandler;
    private readonly Task _staRequestHandler;

    private readonly TaskCompletionSource<Control> _marshallingControl = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _staThreadJob = new(
        TaskCreationOptions.RunContinuationsAsynchronously);

    public WindowsMessagePump(
        ILogger<WindowsMessagePump> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IStaThreadReader staThreadReader)
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
        _staThreadReader = staThreadReader;
        _cancelStaRequestHandler = CancellationTokenSource.CreateLinkedTokenSource(hostApplicationLifetime.ApplicationStopping);

        _staRequestHandler = RunStaRequestHandler();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _thread.Start();

        return _staThreadJob.Task;
    }

    private async Task RunStaRequestHandler()
    {
        var cancellationToken = _cancelStaRequestHandler.Token;
        try
        {
            var marshallingControl = await _marshallingControl.Task.WaitAsync(cancellationToken);

            await _staThreadReader.Invoke(
                marshallingControl.InvokeAsync,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private unsafe void ThreadInner()
    {
        try
        {
            var cancellationToken = _hostApplicationLifetime.ApplicationStopping;
            var marshallingControl = new MarshallingControl();
            _marshallingControl.SetResult(marshallingControl);

            using var tokenRegistration = cancellationToken.Register(
                () => marshallingControl.Invoke(
                    () => User32.PostQuitMessage(0)));

            NativeMessage msg;
            RawBool getMessageW;

            while (getMessageW = User32
                .GetMessageW(
                    lpMsg: &msg,
                    hWnd: IntPtr.Zero,
                    wMsgFilterMin: 0,
                    wMsgFilterMax: 0)
                == true)
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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error in Windows Message Pump processing Windows messages!");
        }
        finally
        {
            _ = _staThreadJob.TrySetResult();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposedValue)
        {
            _cancelStaRequestHandler.Cancel();
            _cancelStaRequestHandler.Dispose();

            await AwaitAndLogAnyErrors(_staRequestHandler.WaitAsync)
                .ConfigureAwait(false);

            _thread.Join();

            await AwaitAndLogAnyErrors(async ct =>
            {
                var marshallingCtrl = await _marshallingControl.Task
                    .WaitAsync(ct)
                    .ConfigureAwait(false);

                marshallingCtrl.Dispose();
            }).ConfigureAwait(false);

            _hostApplicationLifetime.StopApplication();
        }
        _disposedValue = true;
        GC.SuppressFinalize(this);

        return;

        /* local function */
        async ValueTask AwaitAndLogAnyErrors(
            Func<CancellationToken, Task> callback)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await callback(cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while disposing WindowsMessagePump!");
            }
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
}