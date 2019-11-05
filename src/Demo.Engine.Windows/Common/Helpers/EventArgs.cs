using System;

namespace Demo.Engine.Windows.Common.Helpers
{
    public class EventArgs<T> : EventArgs
    {
        public T Value { get; }

        public EventArgs(T value) => Value = value;
    }
}