// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal static partial class DescriptorHeapAllocatorLoggerExtensions
{
    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Invalid heap descriptor index!")]
    internal static partial void LogInvalidHeapDescriptorIndex(
        ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Given descriptor handle wasn't allocated by this heap allocator!")]
    internal static partial void LogDescriptorHandleWasntAllocatedByThisHeapAllocator(
        ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Invalid descriptor handle!")]
    internal static partial void LogInvalidDescriptorHandle(
        ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Invalid CPU pointer!")]
    internal static partial void LogInvalidCpuPointer(
        ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Invalid CPU pointer offset!")]
    internal static partial void LogInvalidCpuPointerOffset(
        ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Index {index} cannot be greater than the capacity {capacity}!")]
    internal static partial void LogIndexCannotBeGreaterThanCapacity(
        ILogger logger,
        int index,
        uint capacity);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Capacity cannot be 0")]
    internal static partial void LogCapacityCannotBeZero(
        ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to set capacity for Descriptor Heap Allocator of type {heapType}: {errorMessage}")]
    internal static partial void LogFailedToSetCapacityForDescriptorHeapAllocator(
        this ILogger logger,
        DescriptorHeapType heapType,
        string errorMessage);
}