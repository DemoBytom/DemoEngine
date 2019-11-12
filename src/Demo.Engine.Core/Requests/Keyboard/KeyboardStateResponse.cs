namespace Demo.Engine.Core.Requests.Keyboard
{
    public class KeyboardStateResponse
    {
        private readonly bool[] _keysPressed;
        private readonly char[] _chars;

        public KeyboardStateResponse(
            bool[] keysPressed,
            char[] chars)
        {
            _keysPressed = keysPressed;
            _chars = chars;
        }

        public bool GetKeyState(char key) => _keysPressed[key];

        public string GetString() => new string(_chars);
    }
}