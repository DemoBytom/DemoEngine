// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.CodeAnalysis;

namespace Demo.Tools.SourceGenerators;

[Generator]
public class ValueResultSourceGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => context.RegisterPostInitializationOutput(ctx
            => ctx
                .AddSource(
                    "BindExtensions.g.cs",
                    BindExtensionGenerator.GenerateBindExtensions)
                .AddSource(
                    "MapExtensions.g.cs",
                    MapExtensionGenerator.GenerateMapExtensions)
                .AddSource(
                    "MatchExtensions.g.cs",
                    MatchExtensionsGenerator.GenerateMatchExtensions)
                .AddSource(
                    "LogAndReturnExtensions.g.cs",
                    LogAndReturnExtensionsGenerator.GenerateExtensions,
                    numberOfGenericParameters: NUMBER_OF_GENERIC_PARAMETERS_FOR_LOGGING)
        );
}