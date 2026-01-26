// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal static class MatchExtensionsGenerator
{
    internal static string GenerateExtensions(ushort numberOfGenericParams)
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
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"scoped in {firstGenericParam} {firstGenericParamName}");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.Write($"where {firstGenericParam} : ");
        if (firstGenericParamConstraint is not null)
        {
            itw.Write($"{firstGenericParamConstraint}, ");
        }
        itw.WriteLine("allows ref struct");
        itw.Write($"where TResult : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
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
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"scoped in {firstGenericParam} {firstGenericParamName}");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.Write($"where {firstGenericParam} : ");
        if (firstGenericParamConstraint is not null)
        {
            itw.Write($"{firstGenericParamConstraint}, ");
        }
        itw.Write("allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
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
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"this scoped in global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError> result");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(",");
        itw.Write("OnSuccessFunc<TValue, TResult");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> onSuccess,");
        itw.Write("OnFailureFunc<TError, TResult");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> onFailure)");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.WriteLine("where TValue : allows ref struct");
        itw.Write("where TResult : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine();
        itw.WriteLine("=> result.IsSuccess");
        itw.Indent++;
        itw.Write("? onSuccess(result.Value");

        itw.WriteInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.Write(": onFailure(result.Error");

        itw.WriteInParams(currentAmountOfGenericParams);
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
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"this scoped in global::{DEFAULT_NAMESPACE}.ValueResult<TValue, TError> result");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(",");
        itw.Write("OnSuccessAction<TValue");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> onSuccess,");
        itw.Write("OnFailureAction<TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> onFailure)");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.Write("where TValue : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.Indent--;
        itw.WriteLine();
        itw.WriteLine('{');
        itw.Indent++;
        itw.WriteLine("if(result.IsSuccess)");
        itw.WriteLine('{');
        itw.Indent++;
        itw.Write("onSuccess(result.Value");
        itw.WriteInParams(currentAmountOfGenericParams);
        itw.WriteLine(");");
        itw.Indent--;
        itw.WriteLine('}');
        itw.WriteLine("else");
        itw.WriteLine('{');
        itw.Indent++;
        itw.Write("onFailure(result.Error");
        itw.WriteInParams(currentAmountOfGenericParams);
        itw.WriteLine(");");
        itw.Indent--;
        itw.WriteLine('}');
        itw.Indent--;
        itw.WriteLine('}');
    }
}