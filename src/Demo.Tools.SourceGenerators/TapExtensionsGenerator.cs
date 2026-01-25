// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal static class TapExtensionsGenerator
{
    internal static string GenerateExtensions(ushort numberOfGenericParams)
    {
        IndentedTextWriter itw = new(new StringWriter(), "    ");
        itw.WriteLine($"#nullable enable");
        itw.WriteLine();
        itw.WriteLine($"namespace {DEFAULT_NAMESPACE};");
        itw.WriteLine();
        itw.WriteLine("public static class TapExtensions");
        itw.WriteLine("{");

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
            GenerateTapMethod);
        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateTapErrorMethod);
        itw.Indent--;
        itw.WriteLine('}');
        itw.WriteLine();

        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateTapDelegte);
        itw.WriteLine();
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateTapErrorDelegte);
        itw.Indent--;

        itw.WriteLine("}");
        return itw.InnerWriter.ToString();
    }

    private static void GenerateTapMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => GenerateTapMethodInternal(itw, currentAmountOfGenericParams,
            tapName: "tap",
            valueResultParameter: "Value",
            valueResultConditionParameter: "IsSuccess",
            summaryRemarkChar: 'T');

    private static void GenerateTapErrorMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
        => GenerateTapMethodInternal(itw, currentAmountOfGenericParams,
            tapName: "tapError",
            valueResultParameter: "Error",
            valueResultConditionParameter: "IsError",
            summaryRemarkChar: 'E');

    private static void GenerateTapMethodInternal(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams,
        string tapName,
        string valueResultParameter,
        string valueResultConditionParameter,
        char summaryRemarkChar)
    {
        var tapLowercase = tapName;
        var tapFirstLetterUppercase = char.ToUpperInvariant(tapLowercase[0])
            + tapLowercase.AsSpan().Slice(1).ToString();

        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// <para>{tapFirstLetterUppercase} extension method with {currentAmountOfGenericParams} extra parameters.</para>");
        itw.WriteLine("/// <para>Runs side-effects without changing the result.</para>");
        itw.WriteLine("/// </summary>");
        itw.WriteLine("/// <remarks>");
        itw.WriteLine($"/// ValueResult&lt;T, E&gt; → ({summaryRemarkChar} → void) → ValueResult&lt;T, E&gt;");
        itw.WriteLine("/// </remarks>");

        itw.Write($"public global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError> {tapFirstLetterUppercase}");
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
                        itw.Write(',');
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
        itw.Write($"{tapFirstLetterUppercase}Action<T{valueResultParameter}");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.Write($"> {tapLowercase})");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine();
        itw.WriteLine('{');
        itw.Indent++;

        itw.WriteLine($"if (result.{valueResultConditionParameter})");
        itw.WriteLine('{');
        itw.Indent++;
        itw.Write($"{tapLowercase}(result.{valueResultParameter}");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.Write($", param{currentParam}"));
        itw.WriteLine(");");
        itw.Indent--;
        itw.WriteLine('}');
        itw.WriteLine("return result;");
        itw.Indent--;
        itw.WriteLine('}');
        itw.Indent--;
        itw.WriteLine();
    }

    private static void GenerateTapDelegte(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        if (currentAmountOfGenericParams > 0)
        {
            itw.WriteLine();
        }

        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Tap delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate void TapAction<TValue");
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

    private static void GenerateTapErrorDelegte(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        if (currentAmountOfGenericParams > 0)
        {
            itw.WriteLine();
        }

        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// TapError delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate void TapErrorAction<TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write("scoped in TError error");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.Write($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine(";");
        itw.Indent--;
    }
}