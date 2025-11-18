// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.
using System.Runtime.CompilerServices;

namespace Demo.Tools.SourceGenerators.UTs;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
        => VerifySourceGenerators.Initialize();
}