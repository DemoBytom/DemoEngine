// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Core.Interfaces;

namespace Demo.Engine.Core.Exceptions;

public class GraphicsException : Exception
{
    public DebugLayerMessage[] LoggedErrors;

    public GraphicsException(DebugLayerMessage[] loggedErrors)
        => LoggedErrors = loggedErrors;

    public GraphicsException(string? message, DebugLayerMessage[] loggedErrors)
        : base(message)
        => LoggedErrors = loggedErrors;

    public GraphicsException(string? message, DebugLayerMessage[] loggedErrors, Exception? innerException)
        : base(message, innerException)
        => LoggedErrors = loggedErrors;

    public GraphicsException()
        : this(Array.Empty<DebugLayerMessage>())
    {
    }

    public GraphicsException(string? message)
        : this(message, Array.Empty<DebugLayerMessage>())
    {
    }

    public GraphicsException(string? message, Exception? innerException)
    : this(message, Array.Empty<DebugLayerMessage>(), innerException)
    {
    }
}