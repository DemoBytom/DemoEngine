using MediatR;

namespace Demo.Engine.Core.Notifications.Keyboard
{
    public class KeyNotification : INotification
    {
        public KeyNotification(char key, bool down)
        {
            Key = key;
            Down = down;
        }

        public char Key { get; }
        public bool Down { get; }
    }
}
