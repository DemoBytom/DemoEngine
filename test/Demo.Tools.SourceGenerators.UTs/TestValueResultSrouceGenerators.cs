// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Demo.Tools.SourceGenerators.UTs;

public class TestValueResultSrouceGenerators
{
    [Fact]
    public Task Test1()
    {
        return TestHelper.Verify();
    }
}

public static class TestHelper
{
    public static Task Verify()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests")
            ;

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ValueResultSourceGenerator());

        driver = driver.RunGenerators(compilation);

        return Verifier.Verify(driver);
    }
}