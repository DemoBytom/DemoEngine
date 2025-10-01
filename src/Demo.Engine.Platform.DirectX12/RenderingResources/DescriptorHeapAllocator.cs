// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Tools.Common.ValueResults;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;
using static Demo.Engine.Platform.DirectX12.RenderingResources.DescriptorHeapAllocatorLoggerExtensions;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal sealed class RTVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator(
        d3D12RenderingEngine.GetRequiredService<ILogger<RTVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.RenderTargetView);

internal sealed class DSVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator(
        d3D12RenderingEngine.GetRequiredService<ILogger<DSVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.DepthStencilView);

internal sealed class SRVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator(
        d3D12RenderingEngine.GetRequiredService<ILogger<SRVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

internal sealed class UAVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator(
        d3D12RenderingEngine.GetRequiredService<ILogger<UAVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

internal abstract class DescriptorHeapAllocator
    : IDisposable
{
    public CpuDescriptorHandle CPU_Start { get; private set; }

    public GpuDescriptorHandle? GPU_Start { get; private set; }

    /// <summary>
    /// Size of the heap
    /// </summary>
    public uint Capacity { get; private set; }

    /// <summary>
    /// How many descriptors have been allocated
    /// </summary>
    public uint Size { get; private set; } = 0;

    /// <summary>
    /// How big a descriptor is for the heap on this system
    /// </summary>
    public uint DescriptorSize { get; private set; } = 0;

    public DescriptorHeapType HeapType { get; }

    public ID3D12DescriptorHeap? DescriptorHeap { get; private set; }
    private bool _disposedValue;

    private int[] _freeHandles = [];

    private readonly List<int>[] _deferredFreeIndices;

    [MemberNotNullWhen(true, nameof(GPU_Start))]
    public bool IsShaderVisible { get; private set; }

    private readonly Lock _lock = new();
    private readonly ILogger<DescriptorHeapAllocator> _logger;
    private readonly ID3D12RenderingEngine _d3D12RenderingEngine;

    protected DescriptorHeapAllocator(
        ILogger<DescriptorHeapAllocator> logger,
        ID3D12RenderingEngine d3D12RenderingEngine,
        DescriptorHeapType heapType)
    {
        _logger = logger;
        _d3D12RenderingEngine = d3D12RenderingEngine;
        HeapType = heapType;
        _deferredFreeIndices = new List<int>[Common.FRAME_BUFFER_COUNT];
        for (var i = 0; i < _deferredFreeIndices.Length; ++i)
        {
            _deferredFreeIndices[i] = [];
        }
    }

    public bool Initialize(
        uint capacity,
        bool isShaderVisible)
    {
        lock (_lock)
        {
            Capacity = ValueResultExtensions
               .ErrorIfZero(capacity)
               .ErrorIfGreaterThen((uint)D3D12.MaxShaderVisibleDescriptorHeapSizeTier2)
               .ValidateCapacityForSamplerHeap(HeapType)
               .Match(
                   capacity => capacity,
                   error => throw new ArgumentOutOfRangeException(error.Message))
               ;

            if (HeapType
                is DescriptorHeapType.DepthStencilView
                or DescriptorHeapType.RenderTargetView)
            {
                isShaderVisible = false;
            }

            Size = 0;

            DescriptorHeap?.Dispose();
            DescriptorHeap = null;

            var descriptorHeapDescription = new DescriptorHeapDescription(
                type: HeapType,
                descriptorCount: Capacity,
                flags: isShaderVisible
                    ? DescriptorHeapFlags.ShaderVisible
                    : DescriptorHeapFlags.None,
                nodeMask: 0);

            var result = _d3D12RenderingEngine.Device.CreateDescriptorHeap(
                descriptorHeapDescription,
                out var descriptorHeap);

            if (result
                is { Failure: true }
                || descriptorHeap is null)
            {
                //todo: log
                throw new InvalidOperationException("Error creating descriptor heap!");
            }
            descriptorHeap.NameObject(
                $"DescriptorHeap {HeapType}",
                _logger);

            DescriptorHeap = descriptorHeap;

            _freeHandles = new int[Capacity];
            for (var i = 0; i < Capacity; ++i)
            {
                _freeHandles[i] = i;
            }

            for (var i = 0; i < _deferredFreeIndices.Length; ++i)
            {
                if (_deferredFreeIndices[i].Count > 0)
                {
                    throw new InvalidOperationException(
                        "Not all resources were freed before reinitializing descriptor heap!");
                }
            }

            DescriptorSize = _d3D12RenderingEngine.Device.GetDescriptorHandleIncrementSize(
                HeapType);
            CPU_Start = DescriptorHeap.GetCPUDescriptorHandleForHeapStart();
            GPU_Start = isShaderVisible
                ? DescriptorHeap.GetGPUDescriptorHandleForHeapStart()
                : null;

            IsShaderVisible = isShaderVisible;

            return true;
        }
    }

    public DescriptorHandle Allocate()
    {
        lock (_lock)
        {
            ValidateHeapIsNotNull();
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
                value: Size,
                other: Capacity);

            var index = _freeHandles[Size];

            ++Size;

            return new DescriptorHandle(
                cpu: new CpuDescriptorHandle(
                    other: CPU_Start,
                    offsetInDescriptors: index,
                    descriptorIncrementSize: DescriptorSize),
                gpu: IsShaderVisible
                    ? new GpuDescriptorHandle(
                        other: GPU_Start.Value,
                        offsetInDescriptors: index,
                        descriptorIncrementSize: DescriptorSize)
                    : null,
                heapAlocator: this,
                index: index);
        }
    }

    public void Free(
        in DescriptorHandle descriptorHandle)
    {
        lock (_lock)
        {
            ArgumentOutOfRangeException.ThrowIfZero(Size);

            var index =
                ValidateDescriptorHandle(
                    _logger,
                    descriptorHeapAllocator: this,
                    descriptorHandle: descriptorHandle)
                .Bind(
                    CalculateIndex)
                .Bind(
                    param1: _logger,
                    bind: static (scoped in indexDescriptorHandle, scoped in logger)
                    => ValidateHeapDescriptorIndex(indexDescriptorHandle, logger))
                .Match(
                    onSuccess: idh => idh.Index,
                    onFailure: error => error.ErrorType switch
                    {
                        TypedValueError.ErrorTypes.InvalidOperation
                            => throw new InvalidOperationException(error.Message),

                        TypedValueError.ErrorTypes.OutOfRange
                            => throw new ArgumentOutOfRangeException(error.Message),

                        _
                            => throw new Exception(error.Message),
                    })
                ;

            var frameIndex = _d3D12RenderingEngine.CurrentFrameIndex();
            _deferredFreeIndices[frameIndex].Add((int)index);
        }

        static ValueResult<IndexDescriptorHandle, TypedValueError> CalculateIndex(
            scoped in DescriptorHandleValidationContext descriptorHandleValidationContext)
            => descriptorHandleValidationContext is
            {
                Allocator: var allocator,
                DescriptorHandle: var descriptorHandle
                    and { CPU.Ptr: var descriptorHandlePointer },
            }
                ? ValueResult
                    .Success<IndexDescriptorHandle, TypedValueError>(
                        new(
                            index: (uint)(descriptorHandlePointer - allocator.CPU_Start.Ptr) / allocator.DescriptorSize,
                            descriptorHandle: descriptorHandle))
                : TypedValueError.Unreachable<IndexDescriptorHandle>("Unreachable state!")
                ;
    }

    private static ValueResult<DescriptorHandleValidationContext, TypedValueError> ValidateDescriptorHandle(
        in ILogger logger,
        scoped in DescriptorHeapAllocator descriptorHeapAllocator,
        scoped in DescriptorHandle descriptorHandle)
        => descriptorHandle switch
        {
            { Container: var container } when container != descriptorHeapAllocator
                => logger.LogAndReturnInvalidOperation<DescriptorHandleValidationContext>(
                    LogDescriptorHandleWasntAllocatedByThisHeapAllocator,
                    errorMessage: "Given descriptor handle wasn't allocated by this heap allocator!"),

            { IsValid: false }
                => logger.LogAndReturnInvalidOperation<DescriptorHandleValidationContext>(
                    LogInvalidDescriptorHandle,
                    errorMessage: "Invalid descriptor handle!"),

            { CPU.Ptr: var cpuPtr } when cpuPtr < descriptorHeapAllocator.CPU_Start.Ptr
                => logger.LogAndReturnInvalidOperation<DescriptorHandleValidationContext>(
                    LogInvalidCpuPointer,
                    errorMessage: "Invalid CPU pointer!"),

            { CPU.Ptr: var cpuPtr } when (cpuPtr - descriptorHeapAllocator.CPU_Start.Ptr) % descriptorHeapAllocator.DescriptorSize != 0
                => logger.LogAndReturnInvalidOperation<DescriptorHandleValidationContext>(
                    LogInvalidCpuPointerOffset,
                    errorMessage: "Invalid CPU pointer offset!"),

            //{ CPU.Ptr: var cpuPtr } when (cpuPtr - descriptorHeapAllocator.CPU_Start.Ptr) % descriptorHeapAllocator.DescriptorSize != 0
            //    => TypedValueError.InvalidOperation<DescriptorHandleValidationContext>(
            //        "Invalid CPU pointer offset!"),

            { Index: var index } when index > descriptorHeapAllocator.Capacity
                => logger.LogAndReturnOutOfRange<DescriptorHandleValidationContext, int, uint>(
                    logAction: (LogIndexCannotBeGreaterThanCapacity, index, descriptorHeapAllocator.Capacity),
                    parameterName: nameof(descriptorHandle.Index),
                    "Index cannot be greater than the capacity!"),
            _
                => ValueResult.Success<DescriptorHandleValidationContext, TypedValueError>(
                    new(
                        descriptorHeapAllocator,
                        descriptorHandle)),
        };

    private readonly ref struct DescriptorHandleValidationContext(
        DescriptorHeapAllocator allocator,
        DescriptorHandle descriptorHandle)
    {
        public DescriptorHeapAllocator Allocator { get; } = allocator;
        public DescriptorHandle DescriptorHandle { get; } = descriptorHandle;
    }

    private readonly ref struct IndexDescriptorHandle(
        uint index,
        DescriptorHandle descriptorHandle)
    {
        public uint Index { get; } = index;
        public DescriptorHandle DescriptorHandle { get; } = descriptorHandle;
    }

    private static ValueResult<IndexDescriptorHandle, TypedValueError> ValidateHeapDescriptorIndex(
        scoped in IndexDescriptorHandle indexDescriptorHandle,
        scoped in ILogger logger)
        => indexDescriptorHandle.DescriptorHandle.Index == indexDescriptorHandle.Index
            ? ValueResult.Success<IndexDescriptorHandle, TypedValueError>(indexDescriptorHandle)
            : logger.LogAndReturnInvalidOperation<IndexDescriptorHandle>(
                LogInvalidHeapDescriptorIndex,
                errorMessage: "Invalid heap descriptor index!");

    private void ValidateHeapIsNotNull()
    {
        if (DescriptorHeap is null)
        {
            throw new InvalidOperationException(
                "Descriptor heap wasn't initialized properly!");
        }
    }

    public void DeferredRelease()
    {
        if (DescriptorHeap is not null)
        {
            _d3D12RenderingEngine.DeferredRelease(
                DescriptorHeap);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                DescriptorHeap?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal void ProcessDeferredFree(uint frameIndex)
    {
        lock (_lock)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
                frameIndex,
                Common.FRAME_BUFFER_COUNT);

            var indices = _deferredFreeIndices[frameIndex];

            foreach (var index in indices)
            {
                // free
                --Size;
                _freeHandles[Size] = index;
            }
            indices.Clear();
        }
    }

    internal readonly struct DescriptorHandle
        : IDisposable
    {
        public CpuDescriptorHandle? CPU { get; }
        public GpuDescriptorHandle? GPU { get; }
        public DescriptorHeapAllocator Container { get; }
        public int Index { get; }

        [MemberNotNullWhen(true, nameof(CPU))]
        public readonly bool IsValid => CPU is not null;

        [MemberNotNullWhen(true, nameof(GPU))]
        public readonly bool IsSHaderVisible => GPU is not null;

        public DescriptorHandle()
            => throw new InvalidOperationException(
                "Descriptor Handle cannot be created without proper descriptor handles!");

        public DescriptorHandle(
            CpuDescriptorHandle cpu,
            GpuDescriptorHandle? gpu,
            DescriptorHeapAllocator heapAlocator,
            int index)
        {
            CPU = cpu;
            GPU = gpu;

            Container = heapAlocator;
            Index = index;
        }

        public void Dispose()
            => Container.Free(this);
    }
}

internal static class DescriptorHeapAllocatorExtensions
{
    public static DescHeapAllocatorBuilder CreateDescriptorHeaps(
        this ID3D12RenderingEngine renderingEngine)
        => new(renderingEngine)
        {
            Initialized = true,
        };

    public static DescHeapAllocatorBuilder RTV(
        this DescHeapAllocatorBuilder builder,
        uint capacity,
        bool isShaderVisible,
        out RTVDescriptorHeapAllocator rtv)
    {
        rtv = new(builder.RenderingEngine);
        var initialized = rtv.Initialize(capacity, isShaderVisible);
        return builder with { Initialized = builder.Initialized && initialized };
    }

    public static DescHeapAllocatorBuilder DSV(
        this DescHeapAllocatorBuilder builder,
        uint capacity,
        bool isShaderVisible,
        out DSVDescriptorHeapAllocator dsv)
    {
        dsv = new(builder.RenderingEngine);
        var initialized = dsv.Initialize(capacity, isShaderVisible);
        return builder with { Initialized = builder.Initialized && initialized };
    }

    public static DescHeapAllocatorBuilder SRV(
        this DescHeapAllocatorBuilder builder,
        uint capacity,
        bool isShaderVisible,
        out SRVDescriptorHeapAllocator srv)
    {
        srv = new(builder.RenderingEngine);
        var initialized = srv.Initialize(capacity, isShaderVisible);
        return builder with { Initialized = builder.Initialized && initialized };
    }

    public static DescHeapAllocatorBuilder UAV(
        this DescHeapAllocatorBuilder builder,
        uint capacity,
        bool isShaderVisible,
        out UAVDescriptorHeapAllocator uav)
    {
        uav = new(builder.RenderingEngine);
        var initialized = uav.Initialize(capacity, isShaderVisible);
        return builder with { Initialized = builder.Initialized && initialized };
    }

    /// <summary>
    /// Verifies that the heap allocators created by this builder were initialized properly. If not an exception is thrown.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static void VerifyAllDescriptorsCreatedProperly(
        this DescHeapAllocatorBuilder builder)
    {
        if (!builder.Initialized)
        {
            throw new InvalidOperationException(
                "Couldn't create descriptor heap allocator!");
        }
    }

    internal readonly ref struct DescHeapAllocatorBuilder(
        ID3D12RenderingEngine renderingEngine)
    {
        public bool Initialized { get; init; }
        public ID3D12RenderingEngine RenderingEngine { get; } = renderingEngine;
    }
}

static file class DescriptorHeapAllocatorValidationExtensions
{
    internal static ValueResult<uint, ValueError> ValidateCapacityForSamplerHeap(
        this ValueResult<uint, ValueError> capacityResult,
        DescriptorHeapType heapType)
    {

        return capacityResult
            .Bind(ValidateCapacityForSamplerHeapInner);

        ValueResult<uint, ValueError> ValidateCapacityForSamplerHeapInner(
            scoped in uint capacity)
            => heapType is DescriptorHeapType.Sampler
                ? ValueResultExtensions.ErrorIfGreaterThen(
                    capacity,
                    (uint)D3D12.MaxShaderVisibleSamplerHeapSize)
                : ValueResult.Success(capacity);
    }
}

internal partial class DescriptorHeapAllocatorLoggerExtensions
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
}