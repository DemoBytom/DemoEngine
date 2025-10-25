// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using Microsoft.CodeAnalysis.CSharp;

namespace Demo.Tools.SourceGenerators.UTs;

public static class TestHelper
{
    public static Task VerifyValueResultSourceGenerator(
        params string[] allowFilenames)
    {
        var compilation = CSharpCompilation
            .Create(
                assemblyName: "Tests");

        var driver = CSharpGeneratorDriver
            .Create(
                new ValueResultSourceGenerator());

        var returnTask = Verify(driver
            .RunGenerators(compilation));

        return allowFilenames.Length > 0
            ? returnTask
                .IgnoreGeneratedResult(
                generatedSourceResult
                => !allowFilenames.Contains(generatedSourceResult.HintName))
            : returnTask;
    }
}