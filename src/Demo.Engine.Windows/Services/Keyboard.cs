using System.Collections.Generic;
using System.Text;
using Demo.Engine.Windows.Common.Helpers;
using Demo.Engine.Windows.Platform.Netstandard;

namespace Demo.Engine.Windows.Services
{
    public sealed class Keyboard
    {
        private readonly bool[] _keysPressed = new bool[256];
        private readonly Queue<char> _chars = new Queue<char>();
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

        public void ClearState()
        {
            for (var i = 0; i < _keysPressed.Length; ++i)
            {
                _keysPressed[i] = false;
            }
        }

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

        private void _renderingForm_LostFocus(object? sender, System.EventArgs e)
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