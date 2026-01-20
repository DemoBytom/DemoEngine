// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal class LogAndReturnExtensionsGenerator
{
    internal static string GenerateExtensions(ushort numberOfGenericParams)
    {
        IndentedTextWriter itw = new(new StringWriter(), "    ");
        itw.WriteLine($"#nullable enable");
        itw.WriteLine();
        itw.WriteLine($"namespace {DEFAULT_NAMESPACE};");
        itw.WriteLine();
        itw.WriteLine("public static class LogAndReturnExtensions");
        itw.WriteLine("{");
        itw.Indent++;
        itw.WriteLine("extension<TLogger>(TLogger? logger)");
        itw.Indent++;
        itw.WriteLine("where TLogger : global::Microsoft.Extensions.Logging.ILogger");
        itw.Indent--;
        itw.WriteLine('{');
        itw.Indent++;

        itw.WriteInLoopFor(
            (0, numberOfGenericParams),
            GenerateLogAndReturnMethod);

        itw.Indent--;
        itw.WriteLine('}');

        itw.WriteLine();
        itw.WriteLine("public readonly ref struct LogAndReturnResultCallContext()");
        itw.WriteLine('{');
        itw.WriteLine('}');
        itw.Indent--;
        itw.WriteLine("}");
        return itw.InnerWriter.ToString();
    }

    private static void GenerateLogAndReturnMethod(
        IndentedTextWriter itw,
        int currentAmountOfGenericParams)
    {
        if (currentAmountOfGenericParams > 0)
        {
            itw.WriteLine();
        }
        itw.WriteLine("/// <summary>");
        itw.WriteLine($"/// LogAndReturn method with {currentAmountOfGenericParams} extra parameters");
        itw.WriteLine("/// </summary>");
        itw.Write($"public global::{DEFAULT_NAMESPACE}.LogAndReturnExtensions.LogAndReturnResultCallContext LogAndReturn");

        if (currentAmountOfGenericParams > 0)
        {
            itw.Write("<TLogValue1");
            itw.WriteInLoopFor(
                (2, currentAmountOfGenericParams),
                static (itw, currentParam)
                => itw.Write($", TLogValue{currentParam}"));
            itw.Write(">");
        }
        itw.WriteLine("(");
        itw.Indent++;
        itw.Write("Action<TLogger");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.Write($", TLogValue{currentParam}"));
        itw.Write("> logAction");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam) =>
            {
                itw.WriteLine(',');
                itw.Write($"TLogValue{currentParam} value{currentParam}");
            });
        itw.WriteLine(")");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.WriteLine($"where TLogValue{currentParam} : allows ref struct"));
        itw.Indent--;
        itw.WriteLine('{');
        itw.Indent++;

        itw.WriteLine("if (logger is not null)");
        itw.WriteLine('{');
        itw.Indent++;
        itw.Write("logAction(logger");
        itw.WriteInLoopFor(
            (1, currentAmountOfGenericParams),
            static (itw, currentParam)
            => itw.Write($", value{currentParam}"));
        itw.WriteLine(");");
        itw.Indent--;
        itw.WriteLine('}');

        itw.WriteLine($"return new global::{DEFAULT_NAMESPACE}.LogAndReturnExtensions.LogAndReturnResultCallContext();");
        itw.Indent--;
        itw.WriteLine('}');
    }
}