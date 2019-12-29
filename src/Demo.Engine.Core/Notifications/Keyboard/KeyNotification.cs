using Demo.Engine.Core.Platform;
using MediatR;

namespace Demo.Engine.Core.Notifications.Keyboard
{
    public class KeyNotification : INotification
    {
        public KeyNotification(VirtualKeys key, bool down)
        {
            Key = key;
            Down = down;
        }

        public VirtualKeys Key { get; }
        public bool Down { get; }
    }
}