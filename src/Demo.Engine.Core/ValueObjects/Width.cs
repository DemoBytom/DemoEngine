// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Core.ValueObjects;

public readonly record struct Width
    : IEquatable<int>
{
    public int Value { get; }

    public Width(
        int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public Width()
        : this(0)
    {
    }

    public static implicit operator Width(int value)
        => new(value);

    public static bool operator ==(Width left, int right)
        => left.Value == right;

    public static bool operator !=(Width left, int right)
        => left.Value != right;

    public static bool operator ==(int left, Width right)
        => left == right.Value;

    public static bool operator !=(int left, Width right)
        => left != right.Value;

    public static Width operator *(Width left, int right)
        => left.Value * right;

    public static Width operator /(Width left, int right)
        => left.Value / right;

    public static Width operator *(int left, Width right)
        => left * right.Value;

    public static Width operator /(int left, Width right)
        => left / right.Value;

    public static float operator /(Width left, Height right)
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