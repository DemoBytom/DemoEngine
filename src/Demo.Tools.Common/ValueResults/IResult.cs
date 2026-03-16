// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Demo.Tools.Common.ValueResults;

public interface IResult<out TError>
        where TError : IError, allows ref struct
{
    [MemberNotNullWhen(false, nameof(Error))]
    bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    bool IsError { get; }

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

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    new bool IsError { get; }

    TValue? Value { get; }
    new TError? Error { get; }
}