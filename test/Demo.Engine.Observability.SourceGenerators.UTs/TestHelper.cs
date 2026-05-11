// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using Demo.Engine.Observability.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Demo.Engine.Observability.SourceGenerators.UTs;

public static class TestHelper
{
    public static Task VerifyValueResultSourceGenerator<TGenerator>(
        string source,
        params string[] allowFilenames)
        where TGenerator : IIncrementalGenerator, new()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly
                => !assembly.IsDynamic
                && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(InstrumentationAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(InstrumentationAttribute<>).Assembly.Location),
                ])
            ;

        var compilation = CSharpCompilation
            .Create(
                assemblyName: "Tests",
                syntaxTrees: [syntaxTree],
                references: references);

        var driver = CSharpGeneratorDriver
            .Create(
                new TGenerator());

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