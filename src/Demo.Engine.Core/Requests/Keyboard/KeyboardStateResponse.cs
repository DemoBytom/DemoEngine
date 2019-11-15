namespace Demo.Engine.Core.Requests.Keyboard
{
    public class KeyboardStateResponse
    {
        private readonly bool[] _keysPressed;

        public KeyboardStateResponse(
            bool[] keysPressed)
        {
            _keysPressed = keysPressed;
        }

        public bool GetKeyState(char key) => _keysPressed[key];
    }
}