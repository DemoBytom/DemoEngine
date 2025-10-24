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
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs",
            SourceText.From(SourceGenerationHelper.ATTRIBUTE, Encoding.UTF8)));
    }
}

internal static class SourceGenerationHelper
{
    public const string ATTRIBUTE =
        """
        namespace Demo.Tools.Temp
        {
            [System.AttributeUsage(System.AttributeTargets.Enum)]
            public class TestAttribute : global::System.Attribute
            {
            }
        }
        """;
}