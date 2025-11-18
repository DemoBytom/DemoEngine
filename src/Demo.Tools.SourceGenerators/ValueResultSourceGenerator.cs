// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Demo.Tools.SourceGenerators;

[Generator]
public class ValueResultSourceGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterPostInitializationOutput(ctx
            =>
        {
            ctx.AddSource(
                "BindExtensions.g.cs",
                SourceText.From(
                    BindExtensionGenerator.GenerateBindExtensions(NUMBER_OF_GENERIC_PARAMETERS),
                    Encoding.UTF8));

            ctx.AddSource(
                "MapExtensions.g.cs",
                SourceText.From(
                    MapExtensionGenerator.GenerateMapExtensions(NUMBER_OF_GENERIC_PARAMETERS),
                    Encoding.UTF8));
        });
}