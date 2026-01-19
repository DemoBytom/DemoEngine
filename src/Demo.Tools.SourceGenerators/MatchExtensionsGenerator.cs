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

        //MatchWithDelegateFunc
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateMatchWithFuncMethod);

        itw.WriteLine();

        //MatchWithDelegateAction
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateMatchWithActionMethod);

        itw.WriteLine();

        // Delegates, equivalent to:
        // - Func<TValue, TResult...> onSuccess and Func<TError, TResult...> onFailure
        // - Action<TValue..> onSuccess and Action<TError...> onFailure
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
        itw.GenerateFuncDelegateInner(
            methodName: "OnSuccess",
            firstGenericParam: "TValue",
            firstGenericParamName: "value",
            firstGenericParamConstraint: null,
            currentAmountOfGenericParams: currentAmountOfGenericParams);

        itw.WriteLine();

        //OnFailureFunc
        itw.GenerateFuncDelegateInner(
            methodName: "OnFailure",
            firstGenericParam: "TError",
            firstGenericParamName: "error",
            firstGenericParamConstraint: $"global::{DEFAULT_NAMESPACE}.IError",
            currentAmountOfGenericParams: currentAmountOfGenericParams);

        itw.WriteLine();

        //OnSuccessAction
        itw.GenerateActionDelegateInner(
            methodName: "OnSuccess",
            firstGenericParam: "TValue",
            firstGenericParamName: "value",
            firstGenericParamConstraint: null,
            currentAmountOfGenericParams: currentAmountOfGenericParams);

        itw.WriteLine();

        //OnFailureAction
        itw.GenerateActionDelegateInner(
            methodName: "OnFailure",
            firstGenericParam: "TError",
            firstGenericParamName: "error",
            firstGenericParamConstraint: $"global::{DEFAULT_NAMESPACE}.IError",
            currentAmountOfGenericParams: currentAmountOfGenericParams);
    }

    private static void GenerateFuncDelegateInner(
        this IndentedTextWriter itw,
        string methodName,
        string firstGenericParam,
        string firstGenericParamName,
        string? firstGenericParamConstraint,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Match {methodName}Func delegate with {currentAmountOfGenericParams} extra parameters");
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

    private static void GenerateActionDelegateInner(
        this IndentedTextWriter itw,
        string methodName,
        string firstGenericParam,
        string firstGenericParamName,
        string? firstGenericParamConstraint,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Match {methodName}Action delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate void {methodName}Action<{firstGenericParam}");
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
        itw.Write("allows ref struct");
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

    private static void GenerateMatchWithFuncMethod(
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

    private static void GenerateMatchWithActionMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine();

        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Match extension method with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public static void Match<TValue, TError");
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
        itw.Write("OnSuccessAction<TValue");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", TParam{currentParam}"));
        itw.WriteLine("> onSuccess,");
        itw.Write("OnFailureAction<TError");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", TParam{currentParam}"));
        itw.WriteLine("> onFailure)");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.WriteLine("where TValue : allows ref struct");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.WriteLine($"where TParam{currentParam} : allows ref struct"));

        itw.Indent--;
        itw.WriteLine('{');
        itw.Indent++;
        itw.WriteLine("if(result.IsSuccess)");
        itw.WriteLine('{');
        itw.Indent++;
        itw.Write("onSuccess(result.Value");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", in param{currentParam}"));
        itw.WriteLine(");");
        itw.Indent--;
        itw.WriteLine('}');
        itw.WriteLine("else");
        itw.WriteLine('{');
        itw.Indent++;
        itw.Write("onFailure(result.Error");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) => itw.Write($", in param{currentParam}"));
        itw.WriteLine(");");
        itw.Indent--;
        itw.WriteLine('}');
        itw.Indent--;
        itw.WriteLine('}');
    }
}