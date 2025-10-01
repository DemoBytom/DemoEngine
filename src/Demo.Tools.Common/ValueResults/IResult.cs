// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Demo.Tools.Common.ValueResults;

public interface IResult<out TError>
        where TError : IError, allows ref struct
{
    [MemberNotNullWhen(false, nameof(Error))]
    bool IsSuccess { get; }

    TError? Error { get; }
}
public interface IResult<out TValue, out TError>
    : IResult<TError>
    where TError : IError, allows ref struct
    where TValue : allows ref struct
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    new bool IsSuccess { get; }

    TValue? Value { get; }
    new TError? Error { get; }
}