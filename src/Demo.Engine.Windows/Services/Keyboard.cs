using System;
using System.Text;
using Demo.Engine.Windows.Platform.Netstandard;
using Demo.Tools.Common.Collections;
using Demo.Tools.Common.Sys;

namespace Demo.Engine.Windows.Services
{
    public sealed class Keyboard
    {
        private readonly bool[] _keysPressed = new bool[256];
        private readonly CircularQueue<char> _chars = new CircularQueue<char>(16);
        private readonly IRenderingForm _renderingForm;

        public bool KeyPressed(char keyCode) => _keysPressed[keyCode];

        public Keyboard(in IRenderingForm renderingForm)
        {
            _renderingForm = renderingForm;
            _renderingForm.KeyDown += _renderingForm_KeyDown;
            _renderingForm.KeyUp += _renderingForm_KeyUp;
            _renderingForm.LostFocus += _renderingForm_LostFocus;
            _renderingForm.Char += _renderingForm_Char;
        }

        public void ClearState() =>
            Array.Clear(
                _keysPressed,
                0,
                _keysPressed.Length);

        public string ReadChars()
        {
            var sb = new StringBuilder();
            while (_chars.TryDequeue(out var c))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        private void _renderingForm_Char(object? sender, EventArgs<char> e)
        {
            _chars.Enqueue(e.Value);
        }

        private void _renderingForm_LostFocus(object? sender, EventArgs e)
        {
            ClearState();
        }

        private void _renderingForm_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            _keysPressed[(char)e.KeyCode] = false;
        }

        private void _renderingForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            _keysPressed[(char)e.KeyCode] = true;
        }
    }
}