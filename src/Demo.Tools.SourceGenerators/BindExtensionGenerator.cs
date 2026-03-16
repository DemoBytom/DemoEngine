// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal static class BindExtensionGenerator
{
    internal static string GenerateExtensions(ushort numberOfGenericParams)
    {

        IndentedTextWriter itw = new(new StringWriter(), "    ");

        itw.WriteLine($"#nullable enable");
        itw.WriteLine();
        itw.WriteLine($"namespace {DEFAULT_NAMESPACE};");
        itw.WriteLine();
        itw.WriteLine("public static class BindExtensions");
        itw.WriteLine("{");
        itw.Indent++;
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateDelegate);
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateBinMethod);
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

        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Bind delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate global::{DEFAULT_NAMESPACE}.ValueResult<TValue2, TError> BindFunc<TValue1, TValue2, TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write("scoped in TValue1 value");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.WriteLine("where TValue1 : allows ref struct");
        itw.Write("where TValue2 : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine(";");
        itw.Indent--;
    }

    private static void GenerateBinMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine();
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Bind extension method with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public static global::{DEFAULT_NAMESPACE}.ValueResult<TValue2, TError> Bind<TValue1, TValue2, TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"this scoped in global::{DEFAULT_NAMESPACE}.ValueResult<TValue1, TError> result");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(",");
        itw.Write("BindFunc<TValue1, TValue2, TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> bind)");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.WriteLine("where TValue1 : allows ref struct");
        itw.Write("where TValue2 : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine();
        itw.WriteLine("=> result.IsSuccess");
        itw.Indent++;
        itw.Write("? bind(result.Value");
        itw.WriteInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.WriteLine($": global::{DEFAULT_NAMESPACE}.ValueResult<TValue2, TError>.Failure(result.Error);");
        itw.Indent--;
        itw.Indent--;
    }
}