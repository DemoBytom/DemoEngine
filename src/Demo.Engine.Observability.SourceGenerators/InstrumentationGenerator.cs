// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Demo.Engine.Observability.SourceGenerators;

[Generator]
public sealed class InstrumentationGenerator
    : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var instrumentationsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Demo.Engine.Observability.Abstractions.InstrumentationAttribute`1",
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    //todo
                    return new InstrumentationInfo("className", "name", "sourceName");
                });
        ;

        context.RegisterSourceOutput(
            instrumentationsToGenerate,
            static (ctx, info)
            => Execute(ctx, info));
    }

    private static void Execute(
        SourceProductionContext context,
        InstrumentationInfo info)
    {
        context.AddSource(
            $"InstrumentationDX12.g.cs",
            SourceText.From("//todo", Encoding.UTF8));
    }

    public readonly record struct InstrumentationInfo
    {
        public string ClassName { get; }
        public readonly string Name;
        public readonly string? SourceName;

        public InstrumentationInfo(
            string className,
            string name,
            string? sourceName)
        {
            ClassName = className;
            Name = name;
            SourceName = sourceName;
        }
    }
}