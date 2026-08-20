// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
                    var accessibility = typeSymbol.DeclaredAccessibility;

                    var attributeData = ctx.Attributes[0];
                    var attributeClass = (INamedTypeSymbol)attributeData.AttributeClass!;
                    var genericArgument = attributeClass.TypeArguments[0];
                    var genericParam = ctx.SemanticModel.Compilation
                        .GetTypeByMetadataName(genericArgument.ToDisplayString());
                    var genericParamName = genericParam is not null
                        ? $"global::{genericParam.ContainingNamespace}.{genericParam.Name}"
                        : null;

                    var classDeclaration = ctx.TargetNode as ClassDeclarationSyntax;

                    var name = "empty!";
                    string? sourceName = null;

                    var firstArgument = attributeData.ConstructorArguments.ElementAtOrDefault(0);
                    if (firstArgument.Kind == TypedConstantKind.Primitive
                        && firstArgument.Value is string value1)
                    {
                        name = value1;
                    }

                    var secondArgument = attributeData.ConstructorArguments.ElementAtOrDefault(1);
                    if (secondArgument.Kind == TypedConstantKind.Primitive
                        && secondArgument.Value is string value2)
                    {
                        sourceName = value2;
                    }

                    return new InstrumentationInfo(
                        accessibility: accessibility,
                        className: className,
                        containingNamespace: containingNamespace.ToString(),
                        name: name,
                        sourceName: sourceName,
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
        public Accessibility Accessibility { get; }
        public string ClassName { get; }
        public string ContainingNamespace { get; }
        public string Name { get; }
        public string? SourceName { get; }
        public string? GenericParamName { get; }

        public InstrumentationInfo(
            Accessibility accessibility,
            string className,
            string containingNamespace,
            string name,
            string? sourceName,
            string? genericParamName)
        {
            Accessibility = accessibility;
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
            namespace {{info.ContainingNamespace}};

            {{SyntaxFacts.GetText(info.Accessibility)}} partial class {{info.ClassName}}
                : global::Demo.Engine.Observability.Abstractions.IInstrumentation
            {
                public static string INSTRUMENTATION_SOURCE_NAME => "{{info.SourceName ?? info.ContainingNamespace}}";
                
                public static string VERSION
                    => global::System.Reflection.CustomAttributeExtensions.GetCustomAttribute<global::System.Reflection.AssemblyInformationalVersionAttribute>(
                        typeof({{info.GenericParamName}})
                            .Assembly)?
                    .InformationalVersion
                    ?? "0.0.0";

                public static global::System.Diagnostics.Metrics.Meter Meter { get; } = new global::System.Diagnostics.Metrics.Meter(
                    name: INSTRUMENTATION_SOURCE_NAME,
                    version: VERSION);

                public static global::System.Diagnostics.ActivitySource ActivitySource { get; } = new global::System.Diagnostics.ActivitySource(
                    name: INSTRUMENTATION_SOURCE_NAME,
                    version: VERSION);
            }
            """;
    }
}