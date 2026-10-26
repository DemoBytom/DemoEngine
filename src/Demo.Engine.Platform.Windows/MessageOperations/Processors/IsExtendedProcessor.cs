// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using static Demo.Engine.Platform.Windows.Helpers;
using static Demo.Engine.Platform.Windows.KeystrokeMessageFlags;

namespace Demo.Engine.Platform.Windows.MessageOperations.Processors;

/// <inheritdoc cref="KF_EXTENDED"/>
internal readonly ref struct IsExtendedProcessor
    : IOperationProcessor<bool>
{
    public bool Process(in Message message)
        => (HIWORD(message.LParam)
        & KF_EXTENDED) != 0;
}