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
        ushort numberOfGenericParameters = DEFAULT_NUMBER_OF_GENERIC_PARAMETERS)
    {
        ctx.AddSource(
            hintName,
            SourceText.From(
                generatorMethod(numberOfGenericParameters),
                Encoding.UTF8));

        return ctx;
    }

    extension(IndentedTextWriter itw)
    {
        internal void WriteInLoopFor(
            (int startValue, int times) range,
            Action<IndentedTextWriter, int> action)
        {
            for (var i = range.startValue; i <= range.times; i++)
            {
                action(itw, i);
            }
        }

        internal void WriteInLoopFor<TParam1>(
            TParam1 param1,
            (int startValue, int times) range,
            Action<IndentedTextWriter, int, TParam1> action)
        {
            for (var i = range.startValue; i <= range.times; i++)
            {
                action(itw, i, param1);
            }
        }

        internal void WriteInLoopFor(
            (int startValue, int times1, int times2) range,
            Action<IndentedTextWriter, int, int> action)
        {
            for (var i = range.startValue; i <= range.times1; i++)
            {
                for (var j = range.startValue; j <= range.times2; j++)
                {
                    action(itw, i, j);
                }
            }
        }

        internal void WriteTParamsInParams(
            int currentAmountOfGenericParams,
            bool includeStartingComma = true)
            => itw.WriteInLoopFor(
                param1: includeStartingComma,
                (1, currentAmountOfGenericParams),
                static (itw, currentParam, includeStartingComma) =>
                {
                    if (includeStartingComma || currentParam > 1)
                    {
                        itw.WriteLine(",");
                    }
                    itw.Write($"scoped in TParam{currentParam} param{currentParam}");
                });

        internal void WriteTParamConstraints(
            int currentAmountOfGenericParams,
            bool includeStartingWriteLine = true)
            => itw.WriteInLoopFor(
                includeStartingWriteLine,
                (1, currentAmountOfGenericParams),
                static (itw, currentParam, includeStartingWriteLine) =>
                {
                    if (includeStartingWriteLine || currentParam > 1)
                    {
                        itw.WriteLine();
                    }
                    itw.Write($"where TParam{currentParam} : allows ref struct");
                });

        internal void WriteTParamGenericParams(
            int currentAmountOfGenericParams,
            bool includeStartingComma = true)
            => itw.WriteInLoopFor(
                includeStartingComma,
                (1, currentAmountOfGenericParams),
                static (itw, currentParam, includeStartingComma)
                =>
                {
                    if (includeStartingComma || currentParam > 1)
                    {
                        itw.Write(", ");
                    }
                    itw.Write($"TParam{currentParam}");
                });

        internal void WriteInParams(
            int currentAmountOfGenericParams)
            => itw.WriteInLoopFor(
                (1, currentAmountOfGenericParams),
                static (itw, currentParam)
                => itw.Write($", in param{currentParam}"));
    }
}