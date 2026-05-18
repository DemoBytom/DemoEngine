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
    internal const string GENERIC_INSTRUMENTATION_ATTRIBUTE_NAME = "Demo.Engine.Observability.Abstractions.InstrumentationAttribute`1";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var instrumentationsToGenerate = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                GENERIC_INSTRUMENTATION_ATTRIBUTE_NAME,
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    //todo
                    var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                    var containingNamespace = typeSymbol.ContainingNamespace;

                    var className = typeSymbol.Name;

                    var attributeData = ctx.Attributes[0];
                    var attributeClass = (INamedTypeSymbol)attributeData.AttributeClass!;
                    var genericArgument = attributeClass.TypeArguments[0];
                    var genericParam = ctx.SemanticModel.Compilation
                        .GetTypeByMetadataName(genericArgument.ToDisplayString());
                    var genericParamName = genericParam is not null
                        ? $"global::{genericParam.ContainingNamespace}.{genericParam.Name}"
                        : null;

                    return new InstrumentationInfo(
                        className: className,
                        containingNamespace: containingNamespace.ToString(),
                        name: "name",
                        sourceName: "sourceName",
                        genericParamName: genericParamName);
                })
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
            SourceText.From(GenerateSource(info), Encoding.UTF8));
    }

    public readonly record struct InstrumentationInfo
    {
        public string ClassName { get; }
        public string ContainingNamespace { get; }
        public string Name { get; }
        public string? SourceName { get; }
        public string? GenericParamName { get; }

        public InstrumentationInfo(
            string className,
            string containingNamespace,
            string name,
            string? sourceName,
            string? genericParamName)
        {
            ClassName = className;
            ContainingNamespace = containingNamespace;
            Name = name;
            SourceName = sourceName;
            GenericParamName = genericParamName;
        }
    }

    private static string GenerateSource(InstrumentationInfo info)
    {
        return
            $$"""
            namespace global::{{info.ContainingNamespace}};

            public partial class {{info.ClassName}}
                : global::Demo.Engine.Observability.Abstractions.IInstrumentation
            {
                public static string VERSION => typeof({{info.GenericParamName}})
                    .Assembly
                    .GetCustomAttribute<global::System.Reflection.AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion
                    ?? "0.0.0";
            }
            """;
    }
}