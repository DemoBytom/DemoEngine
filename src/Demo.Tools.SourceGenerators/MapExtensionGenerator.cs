// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal static class MapExtensionGenerator
{
    internal static string GenerateMapExtensions(ushort numberOfGenericParams)
    {
        IndentedTextWriter itw = new(new StringWriter(), "    ");

        itw.WriteLine($"#nullable enable");
        itw.WriteLine();
        itw.WriteLine($"namespace {DEFAULT_NAMESPACE};");
        itw.WriteLine();
        itw.WriteLine("public static class MapExtensions");
        itw.WriteLine("{");
        itw.Indent++;
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateDelegate);
        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateMapMethod);
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
        itw.WriteLine($"/// Map delegate with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public delegate TValue2 MapFunc<TValue1, TValue2");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write("scoped in TValue1 value");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(")");
        itw.WriteLine($"where TValue1 : allows ref struct");
        itw.Write($"where TValue2 : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine(";");
        itw.Indent--;

    }

    private static void GenerateMapMethod(
        this IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        itw.WriteLine();
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// Map extension method with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public static global::{DEFAULT_NAMESPACE}.ValueResult<TValue2, TError> Map<TValue1, TValue2, TError");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine(">(");
        itw.Indent++;
        itw.Write($"this scoped in global::{DEFAULT_NAMESPACE}.ValueResult<TValue1, TError> result");
        itw.WriteTParamsInParams(currentAmountOfGenericParams);
        itw.WriteLine(",");
        itw.Write("MapFunc<TValue1, TValue2");
        itw.WriteTParamGenericParams(currentAmountOfGenericParams);
        itw.WriteLine("> map)");
        itw.WriteLine($"where TError : global::{DEFAULT_NAMESPACE}.IError, allows ref struct");
        itw.WriteLine("where TValue1 : allows ref struct");
        itw.Write("where TValue2 : allows ref struct");
        itw.WriteTParamConstraints(currentAmountOfGenericParams);
        itw.WriteLine();
        itw.WriteLine("=> result.IsSuccess");
        itw.Indent++;
        itw.Write("? global::" +
            $"{DEFAULT_NAMESPACE}.ValueResult<TValue2, TError>.Success(map(result.Value");
        itw.WriteInParams(currentAmountOfGenericParams);
        itw.WriteLine("))");
        itw.WriteLine($": global::{DEFAULT_NAMESPACE}.ValueResult<TValue2, TError>.Failure(result.Error);");
        itw.Indent--;
        itw.Indent--;
    }
}