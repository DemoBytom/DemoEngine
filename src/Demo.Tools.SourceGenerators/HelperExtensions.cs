// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.CodeDom.Compiler;

namespace Demo.Tools.SourceGenerators;

internal static class HelperExtensions
{
    public static void WriteInLoopFor(
        this IndentedTextWriter itw,
        (int startValue, int times) range,
        Action<IndentedTextWriter, int> action)
    {
        for (var i = range.startValue; i <= range.times; i++)
        {
            action(itw, i);
        }
    }
}