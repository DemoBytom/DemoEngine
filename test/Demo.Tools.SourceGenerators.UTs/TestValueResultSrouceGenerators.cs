// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
namespace Demo.Tools.SourceGenerators.UTs;

[Trait(TestTypes.TEST_TYPE, TestTypes.SNAPSHOT_TEST)]
public class TestValueResultSrouceGenerators
{
    [Fact(DisplayName =
        """
        GIVEN: ValueResult Source Generator is executed
        WHEN: Source Generator runs
        THEN: Proper Bind extension methods are generated
        """)]
    public Task ValueResultSourceGenerator_BindExtensions_Success()
        => TestHelper
            .VerifyValueResultSourceGenerator(
                allowFilenames: ["BindExtensions.g.cs"]);

    [Fact(DisplayName =
        """
        GIVEN: ValueResult Source Generator is executed
        WHEN: Source Generator runs
        THEN: Proper Map extension methods are generated
        """)]
    public Task ValueResultSourceGenerator_MapExtensions_Success()
        => TestHelper
            .VerifyValueResultSourceGenerator(
                allowFilenames: ["MapExtensions.g.cs"]);
}