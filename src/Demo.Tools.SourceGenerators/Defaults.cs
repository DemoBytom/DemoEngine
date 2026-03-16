// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
global using static Demo.Tools.SourceGenerators.Defaults;

namespace Demo.Tools.SourceGenerators;

internal static class Defaults
{
    internal const ushort DEFAULT_NUMBER_OF_GENERIC_PARAMETERS = 8;
    internal const ushort NUMBER_OF_GENERIC_PARAMETERS_FOR_LOGGING = 10;
    internal const string DEFAULT_NAMESPACE = "Demo.Tools.Common.ValueResults";
}