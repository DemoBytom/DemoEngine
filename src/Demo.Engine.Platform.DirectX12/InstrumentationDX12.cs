// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Engine.Observability.Abstractions;

namespace Demo.Engine.Platform.DirectX12;

[Instrumentation<InstrumentationDX12>(
    name: "DirectX12",
    sourceName: "Demo.Engine.Platform.DirectX12")]
internal sealed partial class InstrumentationDX12
    : IInstrumentation
{
}