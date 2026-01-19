// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Demo.Tools.SourceGenerators;

internal static class HelperExtensions
{
    internal static IncrementalGeneratorPostInitializationContext AddSource(
        this IncrementalGeneratorPostInitializationContext ctx,
        string hintName,
        Func<ushort, string> generatorMethod,
        ushort numberOfGenericParameters = NUMBER_OF_GENERIC_PARAMETERS)
    {
        ctx.AddSource(
            hintName,
            SourceText.From(
                generatorMethod(numberOfGenericParameters),
                Encoding.UTF8));

        return ctx;
    }

    internal static void WriteInLoopFor(
        this IndentedTextWriter itw,
        (int startValue, int times) range,
        Action<IndentedTextWriter, int> action)
    {
        for (var i = range.startValue; i <= range.times; i++)
        {
            action(itw, i);
        }
    }

    internal static void WriteTParamsInParams(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) =>
            {
                itw.WriteLine(",");
                itw.Write($"scoped in TParam{currentParam} param{currentParam}");
            });

    internal static void WriteTParamConstraints(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) =>
            {
                itw.WriteLine();
                itw.Write($"where TParam{currentParam} : allows ref struct");
            });

    internal static void WriteTParamGenericParams(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.Write($", TParam{currentParam}"));

    internal static void WriteInParams(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.Write($", in param{currentParam}"));
}