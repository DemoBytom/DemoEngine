// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SharpGen.Runtime;

namespace Demo.Engine.Platform.DirectX12;

internal static class ResultHelper
{
    /// <summary>
    /// Method verifies if given <see cref="Result.Success"/> == <see langword="true"/>. If so, it marks the <paramref name="outResource"/> as not null for the NRT analyzers.
    /// <see langword="ref"/> <see langword="readonly"/> is used to make sure no defensive copies are made, when passing the reference, including if a <see langword="readonly"/> <see langword="struct"/> was passed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Success<T>(
        this Result result,
#pragma warning disable RCS1163 // Unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        [NotNullWhen(true)] ref readonly T? outResource)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter
        => result.Success;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Failure<T>(
        this Result result,
#pragma warning disable RCS1163 // Unused parameter
#pragma warning disable IDE0060 // Remove unused parameter
        [NotNullWhen(false)] ref readonly T? outResource,
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore RCS1163 // Unused parameter
        [NotNullWhen(true)] out int? errorCode,
        [NotNullWhen(true)] out string? errorMessage)
    {
        (errorCode, errorMessage) = result.Failure ? ((int?)result.Code, result.Description) : (null, null);
        return result.Failure;
    }
}