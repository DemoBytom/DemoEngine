// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;

namespace Demo.Tools.Common.Sys;

public class EventArgs<T> : EventArgs
{
    public T Value { get; }

    public EventArgs(T value) => Value = value;
}