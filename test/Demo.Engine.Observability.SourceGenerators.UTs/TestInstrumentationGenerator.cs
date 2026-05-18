// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Observability.SourceGenerators.UTs;

[Property(TestTypes.TEST_TYPE, TestTypes.SNAPSHOT_TEST)]
public class TestInstrumentationGenerator
{
    [Test]
    public Task ValueResultSourceGenerator_BindExtensions_Success()
        => TestHelper.VerifyValueResultSourceGenerator<InstrumentationGenerator>(
            """
            using Demo.Engine.Observability.Abstractions;

            namespace Demo.Engine.TestAssembly;

            [Instrumentation<InstrumentationDX12>(
                name: "DirectX12",
                sourceName: "Demo.Engine.Platform.DirectX12")]
            public sealed partial class InstrumentationDX12
            {

            }
            """,
            allowFilenames: ["InstrumentationDX12.g.cs"]);
}