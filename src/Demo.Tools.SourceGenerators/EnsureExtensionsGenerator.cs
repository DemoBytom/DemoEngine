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
            (0, numberOfGenericParams),
            GenerateEnsureMethod);
        itw.Indent--;
        itw.WriteLine('}');

        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateEnsurePredicateDelegate);

        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateEnsureOnErrorFuncDelegate);

        itw.Indent--;
        itw.WriteLine('}');
        return itw.InnerWriter.ToString();
    }

    private static void GenerateEnsureMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine();
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// <para>Ensure extension method with {currentAmountOfGenericParams} extra parameters</para>");
        itw.WriteLine("/// <para>Validate a successful value.</para>");
        itw.WriteLine("/// </summary>");
        itw.WriteLine("/// <remarks>");
        itw.WriteLine($"/// Result&lt;T, E&gt; → (T → bool, T → E) → Result&lt;T, E&gt;");
        itw.WriteLine("/// </remarks>");
        itw.Write($"public global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError> Ensure");
        if (currentAmountOfGenericParams > 0)
        {
            itw.Write("<");
            itw.WriteInLoopFor(
                (1, currentAmountOfGenericParams),
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
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) =>
            {
                itw.Write($"scoped in TParam{currentParam} param{currentParam}");
                itw.WriteLine(",");
            });
        itw.Write("EnsurePredicate<TValue");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> predicate,");
        itw.Write("EnsureOnErrorFunc<TValue, TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> onError)");
        itw.WriteLine("=> result.IsError");
        itw.Write("|| predicate(result.Value");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.Write($", param{currentParam}"));
        itw.WriteLine(")");
        itw.Indent++;
        itw.WriteLine("? result");
        itw.WriteLine($": global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError>.Failure(");
        itw.Indent++;
        itw.Write("onError(result.Value");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.Write($", param{currentParam}"));
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

    private static void GenerateEnsureOnErrorFuncDelegate(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        if (currentAmountOfGenericParams > 0)
        {
            itw.WriteLine();
        }
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Ensure on error func delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate TError EnsureOnErrorFunc<TValue, TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write("scoped in TValue value");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.WriteLine("where TValue : allows ref struct");
        itw.Write("where TError : global::" + DEFAULT_NAMESPACE + ".IError, allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine(";");
        itw.Indent--;
    }
}