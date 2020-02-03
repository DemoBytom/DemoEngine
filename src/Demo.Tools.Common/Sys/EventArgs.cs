using System;

namespace Demo.Tools.Common.Sys
{
    public class EventArgs<T> : EventArgs
    {
        public T Value { get; }

        public EventArgs(T value) => Value = value;
    }
}