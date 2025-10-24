// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
namespace Demo.Tools.SourceGenerators.UTs;

public class VerifyChecksTests
{
    [Fact]
    public Task Run()
        => VerifyChecks.Run();
}