// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System;
using Demo.Engine.Core.Exceptions;

namespace Demo.Engine.Core.Interfaces;

/// <summary>
/// A logger used to read the messages from the graphics debug layer
/// </summary>
public interface IDebugLayerLogger
{
    /// <summary>
    /// Logs messages stored in the graphic's message queue.
    /// Messages are removied from the queue after logging them.
    /// </summary>
    public void LogMessages();

    /// <summary>
    /// Allows reading the messages from the graphic's message queue.
    /// Messages are not removed from the queue after reading them.
    /// </summary>
    /// <param name="readFrom">Position in the queue from which reading should begin</param>
    /// <returns>List of messages or empty array</returns>

    public DebugLayerMessage[] ReadMessages(ulong readFrom = 0);

    /// <summary>
    /// Returns amount of previously stored messages in the queue, allowing reading from that point only
    /// </summary>
    /// <returns></returns>
    public ulong MessageQueuePosition();

    /// <summary>
    /// Wraps the call in a message queue context, allowing for reading messages produced during this call in case of an error.
    /// In case of an error an exception with read messages is thrown.
    /// </summary>
    /// <typeparam name="T">Type of the resource returned by the func</typeparam>
    /// <param name="func">Resource creation function</param>
    /// <exception cref="GraphicsException">Exception thrown if an error occurs during the call</exception>
    /// <returns>Created resource</returns>
    public T WrapCallInMessageExceptionHandler<T>(Func<T> func);

    /// <summary>
    /// Wraps the call in a message queue context, allowing for reading messages produced during this call in case of an error.
    /// In case of an error an exception with read messages is thrown.
    /// </summary>
    /// <param name="act">Action performed</param>
    /// <exception cref="GraphicsException">Exception thrown if an error occurs during the call</exception>
    public void WrapCallInMessageExceptionHandler(Action act);
}

/// <summary>
/// A single message from the debug layer
/// </summary>
/// <param name="Category">A category of the message, like device creation or destruction</param>
/// <param name="Id">Entity that produced the log message</param>
/// <param name="Message">Logged message</param>
public record DebugLayerMessage(string Category, string Id, string Message);