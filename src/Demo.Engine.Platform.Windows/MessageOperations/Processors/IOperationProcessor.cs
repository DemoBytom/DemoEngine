// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Platform.Windows.MessageOperations.Processors;

internal interface IOperationProcessor<TReturn>
{
    TReturn Process(in Message message);
}