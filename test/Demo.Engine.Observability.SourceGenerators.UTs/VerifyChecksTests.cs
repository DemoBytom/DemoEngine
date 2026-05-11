// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
namespace Demo.Engine.Observability.SourceGenerators.UTs;

public class VerifyChecksTests
{
    [Test]
    public Task Run()
        => VerifyChecks.Run();
}