// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.ValueObjects;

public readonly record struct Height
    : IEquatable<int>
{
    public int Value { get; }

    public Height(
        int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public Height()
        : this(0)
    {
    }

    public static implicit operator Height(
        int value)
        => new(value);

    public static bool operator ==(Height left, int right)
        => left.Value == right;

    public static bool operator !=(Height left, int right)
        => left.Value != right;

    public static bool operator ==(int left, Height right)
        => left == right.Value;

    public static bool operator !=(int left, Height right)
        => left != right.Value;

    public static Height operator *(Height left, int right)
    => left.Value * right;

    public static Height operator /(Height left, int right)
        => left.Value / right;

    public static Height operator *(int left, Height right)
        => left * right.Value;

    public static Height operator /(int left, Height right)
        => left / right.Value;

    public static float operator /(Height left, Width right)
        => left.Value / ((float)right.Value);

    public bool Equals(int other)
        => Value.Equals(other);

    public override int GetHashCode()
        => Value;

    /// <inheritdoc cref="int.ToString()"/>
    public override string ToString()
        => Value.ToString();

    /// <inheritdoc cref="int.ToString(IFormatProvider?)"/>
    public string ToString(
        IFormatProvider formatProvider)
        => Value.ToString(formatProvider);

    /// <inheritdoc cref="int.ToString(string?)"/>
    public string ToString(
        string format)
        => Value.ToString(format);

    /// <inheritdoc cref="int.ToString(string?, IFormatProvider?)"/>
    public string ToString(string format, IFormatProvider formatProvider)
        => Value.ToString(format, formatProvider);
}