// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal static class EnsureExtensionsGenerator
{
    internal static string GenerateExtensions(ushort numberOfGenericParams)
    {
        IndentedTextWriter itw = new(new StringWriter(), "    ");
        itw.WriteLine($"#nullable enable");
        itw.WriteLine();
        itw.WriteLine($"namespace {DEFAULT_NAMESPACE};");
        itw.WriteLine();
        itw.WriteLine("public static class EnsureExtensions");
        itw.WriteLine('{');
        itw.Indent++;
        itw.WriteLine($"extension<TValue, TError>(scoped in global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError> result)");
        itw.Indent++;
        itw.WriteLine("where TValue : allows ref struct");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.Indent--;
        itw.WriteLine('{');
        itw.Indent++;

        itw.WriteInLoopFor(
            (0, numberOfGenericParams, numberOfGenericParams),
            GenerateEnsureMethodWithValueOnError);

        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams, numberOfGenericParams),
            GenerateEnsureMethodWithBothNoValueOnError);

        itw.Indent--;
        itw.WriteLine('}');

        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateEnsurePredicateDelegate);

        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateEnsureOnErrorFuncDelegateWithValueIncluded);

        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateEnsureOnErrorFuncDelegateWithNoValueIncluded);

        itw.Indent--;
        itw.WriteLine('}');
        return itw.InnerWriter.ToString();
    }

    private static void GenerateEnsureMethodWithValueOnError(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams1,
        int currentAmountOfGenericParams2)
        => GenerateEnsureMethod(
            itw,
            currentAmountOfGenericParams1,
            currentAmountOfGenericParams2,
            valueInOnError: true);

    private static void GenerateEnsureMethodWithBothNoValueOnError(
    this IndentedTextWriter itw,
    int currentAmountOfGenericParams1,
    int currentAmountOfGenericParams2)
    => GenerateEnsureMethod(
        itw,
        currentAmountOfGenericParams1,
        currentAmountOfGenericParams2,
        valueInOnError: false);

    private static void GenerateEnsureMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams1,
        int currentAmountOfGenericParams2,
        bool valueInOnError)
    {
        var maxGenericParams = Math.Max(currentAmountOfGenericParams1, currentAmountOfGenericParams2);
        itw.WriteLine();
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// <para>Ensure extension method with {maxGenericParams} extra parameters</para>");
        itw.WriteLine("/// <para>Validate a successful value.</para>");
        itw.WriteLine("/// </summary>");
        itw.WriteLine("/// <remarks>");
        if (valueInOnError)
        {
            itw.WriteLine($"/// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;");
        }
        else
        {
            itw.WriteLine($"/// Result&lt;T, E&gt; → (T → bool, void → E) → Result&lt;T, E&gt;");
        }
        itw.WriteLine("/// </remarks>");
        itw.Write($"public global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError> Ensure");
        if (currentAmountOfGenericParams1 > 0 || currentAmountOfGenericParams2 > 0)
        {
            itw.Write("<");
            itw.WriteInLoopFor(
                (1, maxGenericParams),
                static (itw, currentParam)
                =>
                {
                    if (currentParam > 1)
                    {
                        itw.Write(", ");
                    }
                    itw.Write($"TParam{currentParam}");
                });
            itw.Write('>');
        }
        itw.WriteLine('(');
        itw.Indent++;
        itw.WriteInLoopFor(
            (1, maxGenericParams),
            static (itw, currentParam) =>
            {
                itw.Write($"scoped in TParam{currentParam} param{currentParam}");
                itw.WriteLine(",");
            });
        itw.Write("EnsurePredicate<TValue");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams1);
        itw.WriteLine("> predicate,");
        if (valueInOnError)
        {

            itw.Write("EnsureOnErrorFunc<TValue, TError");
        }
        else
        {
            itw.Write("EnsureOnErrorNoValueFunc<TError");
        }
        itw.WriteTParamGenericParams(currentAmountOfGenericParams2);
        itw.WriteLine("> onError)");
        itw.WriteLine("=> result.IsError");
        itw.Write("|| predicate(result.Value");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams1),
            static (itw, currentParam)
            => itw.Write($", param{currentParam}"));
        itw.WriteLine(")");
        itw.Indent++;
        itw.WriteLine("? result");
        itw.WriteLine($": global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError>.Failure(");
        itw.Indent++;

        if (valueInOnError)
        {
            itw.Write("onError(result.Value");
        }
        else
        {
            itw.Write("onError(");
        }

        itw.WriteInLoopFor(
            valueInOnError,
            (1, currentAmountOfGenericParams2),
            static (itw, currentParam, includeStartingComma)
            =>
            {
                if (includeStartingComma || currentParam > 1)
                {
                    itw.Write(", ");
                }
                itw.Write($"param{currentParam}");
            });

        itw.WriteLine("));");
        itw.Indent--;
        itw.Indent--;
        itw.Indent--;
    }

    private static void GenerateEnsurePredicateDelegate(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        if (currentAmountOfGenericParams > 0)
        {
            itw.WriteLine();
        }
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Ensure predicate delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate bool EnsurePredicate<TValue");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write("scoped in TValue value");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.Write("where TValue : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine(";");
        itw.Indent--;
    }

    private static void GenerateEnsureOnErrorFuncDelegateWithValueIncluded(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => GenerateEnsureOnErrorFuncDelegate(
            itw,
            currentAmountOfGenericParams,
            includeValue: true);

    private static void GenerateEnsureOnErrorFuncDelegateWithNoValueIncluded(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => GenerateEnsureOnErrorFuncDelegate(
            itw,
            currentAmountOfGenericParams,
            includeValue: false);

    private static void GenerateEnsureOnErrorFuncDelegate(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams,
        bool includeValue)
    {
        if (currentAmountOfGenericParams > 0)
        {
            itw.WriteLine();
        }
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Ensure on error func delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate TError EnsureOnError");
        if (includeValue == false)
        {
            itw.Write("NoValue");
        }
        itw.Write("Func<");
        if (includeValue)
        {
            itw.Write("TValue, ");
        }
        itw.Write("TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.Write(">(");
        itw.Indent++;
        if (includeValue)
        {
            itw.WriteLine();
            itw.Write("scoped in TValue value");
        }
        itw.WriteTParamsInParams(currentAmountOfGenericParams, includeStartingComma: includeValue);
        itw.WriteLine(")");
        if (includeValue)
        {
            itw.WriteLine("where TValue : allows ref struct");
        }
        itw.Write("where TError : global::" + DEFAULT_NAMESPACE + ".IError, allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine(";");
        itw.Indent--;
    }
}