// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
namespace Demo.Tools.SourceGenerators.UTs;

[Property(TestTypes.TEST_TYPE, TestTypes.SNAPSHOT_TEST)]
public class TestValueResultSrouceGenerators
{
    [Test]
    public Task ValueResultSourceGenerator_BindExtensions_Success()
        => TestHelper.VerifyValueResultSourceGenerator(allowFilenames: ["BindExtensions.g.cs"]);

    [Test]
    public Task ValueResultSourceGenerator_MapExtensions_Success()
        => TestHelper.VerifyValueResultSourceGenerator(allowFilenames: ["MapExtensions.g.cs"]);

    [Test]
    public Task ValueResultSourceGenerator_MatchExtensions_Success()
        => TestHelper.VerifyValueResultSourceGenerator(allowFilenames: ["MatchExtensions.g.cs"]);

    [Test]
    public Task ValueResultSourceGenerator_LogAndReturnExtensions_Success()
        => TestHelper.VerifyValueResultSourceGenerator(allowFilenames: ["LogAndReturnExtensions.g.cs"]);

    [Test]
    public Task ValueResultSourceGenerator_Tap_Success()
        => TestHelper.VerifyValueResultSourceGenerator(allowFilenames: ["TapExtensions.g.cs"]);

    [Test]
    public Task ValueResultSourceGenerator_Ensure_Success()
        => TestHelper.VerifyValueResultSourceGenerator(allowFilenames: ["EnsureExtensions.g.cs"]);
}