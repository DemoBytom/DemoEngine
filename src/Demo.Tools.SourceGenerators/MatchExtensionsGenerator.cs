// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal static class MatchExtensionsGenerator
{
    internal static string GenerateMatchExtensions(ushort numberOfGenericParams)
    {
        IndentedTextWriter itw = new(new StringWriter(), "    ");
        itw.WriteLine($"#nullable enable");
        itw.WriteLine();
        itw.WriteLine($"namespace {DEFAULT_NAMESPACE};");
        itw.WriteLine();
        itw.WriteLine("public static class MatchExtensions");
        itw.WriteLine("{");
        itw.Indent++;

        //MatchWithDelegate
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateMatchMethod);

        itw.WriteLine();

        // Delegates, equivalent to Func<TValue, TResult...> onSuccess and Func<TError, TResult...> onFailure
        // Still remaining methods and delegates for Action<TValue..> onSuccess and Action<TError...> onFailure
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateDelegate);
        itw.Indent--;
        itw.WriteLine("}");
        return itw.InnerWriter.ToString();
    }

    private static void GenerateDelegate(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        if (currentAmountOfGenericParams > 0)
        {
            itw.WriteLine();
        }

        //OnSuccessFunc
        itw.GenerateDelegateInner(
            methodName: "OnSuccess",
            firstGenericParam: "TValue",
            firstGenericParamName: "value",
            firstGenericParamConstraint: null,
            currentAmountOfGenericParams: currentAmountOfGenericParams);

        itw.WriteLine();

        //OnFailureFunc
        itw.GenerateDelegateInner(
            methodName: "OnFailure",
            firstGenericParam: "TError",
            firstGenericParamName: "error",
            firstGenericParamConstraint: $"global::{DEFAULT_NAMESPACE}.IError",
            currentAmountOfGenericParams: currentAmountOfGenericParams);
    }

    private static void GenerateDelegateInner(
        this IndentedTextWriter itw,
        string methodName,
        string firstGenericParam,
        string firstGenericParamName,
        string? firstGenericParamConstraint,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Match {methodName} delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate TResult {methodName}Func<{firstGenericParam}, TResult");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", TParam{currentParam}"));
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"scoped in {firstGenericParam} {firstGenericParamName}");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) =>
            {
                itw.WriteLine(",");
                itw.Write($"scoped in TParam{currentParam} param{currentParam}");
            });
        itw.WriteLine(")");
        itw.Write($"where {firstGenericParam} : ");
        if (firstGenericParamConstraint is not null)
        {
            itw.Write($"{firstGenericParamConstraint}, ");
        }
        itw.WriteLine("allows ref struct");
        itw.Write($"where TResult : allows ref struct");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) =>
            {
                itw.WriteLine();
                itw.Write($"where TParam{currentParam} : allows ref struct");
            });
        itw.WriteLine(";");
        itw.Indent--;
    }

    private static void GenerateMatchMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine();

        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Match extension method with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public static TResult Match<TValue, TError, TResult");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", TParam{currentParam}"));
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"this scoped in global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError> result");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) =>
            {
                itw.WriteLine(",");
                itw.Write($"scoped in TParam{currentParam} param{currentParam}");
            });
        itw.WriteLine(",");
        itw.Write("OnSuccessFunc<TValue, TResult");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", TParam{currentParam}"));
        itw.WriteLine("> onSuccess,");
        itw.Write("OnFailureFunc<TError, TResult");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", TParam{currentParam}"));
        itw.WriteLine("> onFailure)");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.WriteLine("where TValue : allows ref struct");
        itw.WriteLine("where TResult : allows ref struct");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.WriteLine($"where TParam{currentParam} : allows ref struct"));

        itw.WriteLine("=> result.IsSuccess");
        itw.Indent++;
        itw.Write("? onSuccess(result.Value");

        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", in param{currentParam}"));
        itw.WriteLine(")");
        itw.Write(": onFailure(result.Error");

        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", in param{currentParam}"));
        itw.WriteLine(");");
        itw.Indent--;
        itw.Indent--;
    }
}