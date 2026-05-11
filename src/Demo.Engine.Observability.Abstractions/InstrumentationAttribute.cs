// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

namespace Demo.Engine.Observability.Abstractions;

[AttributeUsage(AttributeTargets.Class)]
public sealed class InstrumentationAttribute
    : Attribute
{
    public string Name { get; }
    public string? SourceName { get; }

    public InstrumentationAttribute(
        string name)
        => Name = name;

    public InstrumentationAttribute(
        string name,
        string sourceName)
    {
        Name = name;
        SourceName = sourceName;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class InstrumentationAttribute<TAssemblyMarker>
    : Attribute
{
    public string Name { get; }
    public string? SourceName { get; }

    public InstrumentationAttribute(
        string name)
        => Name = name;

    public InstrumentationAttribute(
        string name,
        string sourceName)
    {
        Name = name;
        SourceName = sourceName;
    }
}