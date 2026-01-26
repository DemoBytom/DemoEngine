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
                    BindExtensionGenerator.GenerateExtensions)
                .AddSource(
                    "MapExtensions.g.cs",
                    MapExtensionGenerator.GenerateExtensions)
                .AddSource(
                    "MatchExtensions.g.cs",
                    MatchExtensionsGenerator.GenerateExtensions)
                .AddSource(
                    "LogAndReturnExtensions.g.cs",
                    LogAndReturnExtensionsGenerator.GenerateExtensions,
                    numberOfGenericParameters: NUMBER_OF_GENERIC_PARAMETERS_FOR_LOGGING)
                .AddSource(
                    "TapExtensions.g.cs",
                    TapExtensionsGenerator.GenerateExtensions)
                .AddSource(
                    "EnsureExtensions.g.cs",
                    EnsureExtensionsGenerator.GenerateExtensions)
        );
}