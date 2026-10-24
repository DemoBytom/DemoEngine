// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Platform.Windows.ValueStructs;

namespace Demo.Engine.Platform.Windows;

internal static class MessageExtensions
{
    extension(in Message msg)
    {
        internal KeyOperation AsKeyOperation()
            => new(in msg);
    }
}