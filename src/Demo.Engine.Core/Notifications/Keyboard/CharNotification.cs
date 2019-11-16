using MediatR;

namespace Demo.Engine.Core.Notifications.Keyboard
{
    public class CharNotification : INotification
    {
        public CharNotification(char @char) => Char = @char;

        public char Char { get; }
    }
}