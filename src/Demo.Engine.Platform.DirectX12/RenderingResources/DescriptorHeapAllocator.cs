// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Demo.Tools.Common.ValueResults;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;
using static Demo.Engine.Platform.DirectX12.RenderingResources.DescriptorHeapAllocatorLoggerExtensions;
using static Demo.Engine.Platform.DirectX12.RenderingResources.DescriptorHeapAllocatorValidationExtensions;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal sealed class RTVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator<RTVDescriptorHeapAllocator>(
        d3D12RenderingEngine.GetRequiredService<ILogger<RTVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.RenderTargetView);

internal sealed class DSVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator<DSVDescriptorHeapAllocator>(
        d3D12RenderingEngine.GetRequiredService<ILogger<DSVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.DepthStencilView);

internal sealed class SRVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator<SRVDescriptorHeapAllocator>(
        d3D12RenderingEngine.GetRequiredService<ILogger<SRVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

internal sealed class UAVDescriptorHeapAllocator(
    ID3D12RenderingEngine d3D12RenderingEngine)
    : DescriptorHeapAllocator<UAVDescriptorHeapAllocator>(
        d3D12RenderingEngine.GetRequiredService<ILogger<UAVDescriptorHeapAllocator>>(),
        d3D12RenderingEngine,
        DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

internal abstract class DescriptorHeapAllocator<TDescriptorHeapAllocator>(
    ILogger<TDescriptorHeapAllocator> logger,
    ID3D12RenderingEngine d3D12RenderingEngine,
    DescriptorHeapType heapType)
    : IDisposable
    where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
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

    public DescriptorHeapType HeapType { get; } = heapType;

    public ID3D12DescriptorHeap? DescriptorHeap { get; private set; }
    private bool _disposedValue;

    private int[] _freeHandles = [];

    private readonly List<int>[] _deferredFreeIndices = InitializeDeferredFreeIndices(Common.FRAME_BUFFER_COUNT);

    [MemberNotNullWhen(true, nameof(GPU_Start))]
    public bool IsShaderVisible { get; private set; }

    private readonly Lock _lock = new();
    private readonly ILogger<TDescriptorHeapAllocator> _logger = logger;

#pragma warning disable CA2213 // Disposable fields should be disposed
    /* This line reports a false positive CA2213 warning if the primary constructor with fields is used
     * A bug report here: https://github.com/dotnet/roslyn-analyzers/issues/7803
     * */
    private readonly ID3D12RenderingEngine _d3D12RenderingEngine = d3D12RenderingEngine;
#pragma warning restore CA2213 // Disposable fields should be disposed

    public ValueResult<TDescriptorHeapAllocator, ValueError> Initialize(
        uint capacity,
        bool isShaderVisible)
    {
        lock (_lock)
        {

            return ValueResult
                .Success<TDescriptorHeapAllocator>(this)
                .SetCapacity(
                    _logger,
                    capacity,
                    SetCapacityInnner)
                .Tap(
                    tap: ClearDescriptorHeap)
                .Bind(
                    param1: _d3D12RenderingEngine,
                    param2: isShaderVisible,
                    param3: _logger,
                    bind: InitializeDescriptorHeap)
                .Bind(
                    param1: _logger,
                    bind: InitializeFreeHandles)
                .Tap(
                    param1: _d3D12RenderingEngine,
                    param2: isShaderVisible,
                    tap: SetupAllocator)
                ;
        }

        // local methods
        static void ClearDescriptorHeap(
            scoped in TDescriptorHeapAllocator allocator)
        {
            allocator.Size = 0;
            allocator.DescriptorHeap?.Dispose();
            allocator.DescriptorHeap = null;
        }

        static ValueResult<TDescriptorHeapAllocator, ValueError> InitializeDescriptorHeap(
            scoped in TDescriptorHeapAllocator allocator,
            scoped in ID3D12RenderingEngine renderingEnggine,
            scoped in bool isShaderVisible,
            scoped in ILogger<TDescriptorHeapAllocator> logger)
        {
            var descriptorHeapDescription = new DescriptorHeapDescription(
                type: allocator.HeapType,
                descriptorCount: allocator.Capacity,
                flags: allocator.GetIsShaderVisible(isShaderVisible)
                    ? DescriptorHeapFlags.ShaderVisible
                    : DescriptorHeapFlags.None,
                nodeMask: 0);

            var result = renderingEnggine.Device.CreateDescriptorHeap(
                descriptorHeapDescription,
                out var descriptorHeap);

            if (result
                is { Failure: true }
                || descriptorHeap is null)
            {
                logger.LogError("Error creating descriptor heap!");
                return ValueResult.Failure<TDescriptorHeapAllocator>("Error creating descriptor heap!");
            }
            descriptorHeap.NameObject(
                $"DescriptorHeap {allocator.HeapType}",
                logger);

            allocator.DescriptorHeap = descriptorHeap;

            return ValueResult.Success(allocator);
        }

        static ValueResult<TDescriptorHeapAllocator, ValueError> InitializeFreeHandles(
            scoped in TDescriptorHeapAllocator allocator,
            scoped in ILogger<TDescriptorHeapAllocator> logger)
        {
            allocator._freeHandles = new int[allocator.Capacity];
            for (var i = 0; i < allocator.Capacity; ++i)
            {
                allocator._freeHandles[i] = i;
            }

            for (var i = 0; i < allocator._deferredFreeIndices.Length; ++i)
            {
                if (allocator._deferredFreeIndices[i].Count > 0)
                {
                    logger.LogError("Not all resources were freed before reinitializing descriptor heap!");
                    return ValueResult.Failure<TDescriptorHeapAllocator>(
                        "Not all resources were freed before reinitializing descriptor heap!");
                }
            }

            return ValueResult.Success(allocator);
        }

        static void SetupAllocator(
            scoped in TDescriptorHeapAllocator allocator,
            scoped in ID3D12RenderingEngine renderingEngine,
            scoped in bool isShaderVisible)
        {
            allocator.DescriptorSize = renderingEngine.Device.GetDescriptorHandleIncrementSize(
                            allocator.HeapType);
            allocator.CPU_Start = allocator.DescriptorHeap!.GetCPUDescriptorHandleForHeapStart();
            allocator.GPU_Start = allocator.GetIsShaderVisible(isShaderVisible)
                ? allocator.DescriptorHeap.GetGPUDescriptorHandleForHeapStart()
                : null;

            allocator.IsShaderVisible = allocator.GetIsShaderVisible(isShaderVisible);
        }
    }

    public DescriptorHandle<TDescriptorHeapAllocator> Allocate()
    {
        lock (_lock)
        {
            return ValueResult.Success<TDescriptorHeapAllocator>(this)
                .Ensure(
                    param1: _logger,
                    predicate: DescriptorHeapExists,
                    onError: DescriptorHeapDoesNotExist)
                .Ensure(
                    param1: _logger,
                    predicate: SizeIsBelowCapacity,
                    onError: CapacityTooLow)
                .Map(
                    param1: _freeHandles,
                    map: static (scoped in dha, scoped in freeHandles)
                        => new DescriptorHandle<TDescriptorHeapAllocator>(
                            heapAllocator: dha,
                            index: freeHandles[dha.Size++]))
                .Match(
                    onSuccess: static (scoped in handle)
                        => handle,
                    onFailure: static (scoped in error)
                        => throw new InvalidOperationException(error.Message))
                ;
        }

        static bool DescriptorHeapExists(
            scoped in TDescriptorHeapAllocator dha)
            => dha.DescriptorHeap is not null;

        static bool SizeIsBelowCapacity(
            scoped in TDescriptorHeapAllocator dha)
            => dha.Size < dha.Capacity;

        static ValueError DescriptorHeapDoesNotExist(
            scoped in ILogger<TDescriptorHeapAllocator> logger)
        {
            logger.LogDescriptorHeapWasntInitializedProperly();
            return new ValueError("Descriptor heap wasn't initialized properly!");
        }

        static ValueError CapacityTooLow(
            scoped in TDescriptorHeapAllocator dha,
            scoped in ILogger<TDescriptorHeapAllocator> logger)
        {
            logger.LogDescriptorHeapCapacityReached(dha.HeapType, dha.Capacity);
            return new ValueError($"Descriptor heap capacity reached ({dha.Capacity})");
        }
    }

    public void Free(
        in DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)
    {
        lock (_lock)
        {
            ValidateDescriptorHandle(
                _logger,
                descriptorHeapAllocator: this,
                descriptorHandle: descriptorHandle)
            .CalculateIndex()
            .ValidateHeapDescriptorIndex(_logger)
            .Tap(
                param1: _d3D12RenderingEngine,
                param2: _deferredFreeIndices,
                tap: AddIndex)
            .MatchFailure(
                onFailure: error =>
                {
                    if (error is { InnerError: IThrowableError throwableError })
                    {
                        throwableError.ThrowAsException();
                    }
                    else
                    {
                        throw new Exception(error.Message);
                    }
                })
            ;
        }
    }

    private static void SetCapacityInnner<TDescriptorHeapAllcoator>(
        scoped in TDescriptorHeapAllcoator @this,
        scoped in uint capacity)
        where TDescriptorHeapAllcoator : DescriptorHeapAllocator<TDescriptorHeapAllcoator>
        => @this.Capacity = capacity;

    private bool GetIsShaderVisible(bool isShaderVisible)
        => isShaderVisible
        && HeapType is not (DescriptorHeapType.DepthStencilView or DescriptorHeapType.RenderTargetView);

    private static ValueResult<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle), TypedValueError> ValidateDescriptorHandle(
        in ILogger logger,
        scoped in TDescriptorHeapAllocator descriptorHeapAllocator,
        scoped in DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)
        => descriptorHandle switch
        {
            { Container.Capacity: 0 }
                => logger
                    .LogAndReturn(LogCapacityCannotBeZero)
                    .OutOfRange<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)>(
                        parameterName: nameof(Capacity),
                        errorMessage: "Capacity cannot be 0"),

            { Container: var container } when container != descriptorHeapAllocator
                => logger
                    .LogAndReturn(LogDescriptorHandleWasntAllocatedByThisHeapAllocator)
                    .InvalidOperation<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)>(
                        errorMessage: "Given descriptor handle wasn't allocated by this heap allocator!"),

            { IsValid: false }
                => logger
                    .LogAndReturn(LogInvalidDescriptorHandle)
                    .InvalidOperation<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)>(
                        errorMessage: "Invalid descriptor handle!"),

            { CPU.Ptr: var cpuPtr } when cpuPtr < descriptorHeapAllocator.CPU_Start.Ptr
                => logger
                    .LogAndReturn(LogInvalidCpuPointer)
                    .InvalidOperation<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)>(
                        errorMessage: "Invalid CPU pointer!"),

            { CPU.Ptr: var cpuPtr } when (cpuPtr - descriptorHeapAllocator.CPU_Start.Ptr) % descriptorHeapAllocator.DescriptorSize != 0
                => logger
                    .LogAndReturn(LogInvalidCpuPointerOffset)
                    .InvalidOperation<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)>(
                        errorMessage: "Invalid CPU pointer offset!"),

            { Index: var index } when index > descriptorHeapAllocator.Capacity
                => logger
                    .LogAndReturn(
                        LogIndexCannotBeGreaterThanCapacity,
                        index,
                        descriptorHeapAllocator.Capacity)
                    .OutOfRange<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)>(
                        nameof(descriptorHandle.Index),
                        "Index cannot be greater than the capacity!"),
            _
                => ValueResult.Success<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle), TypedValueError>(
                    new(
                        descriptorHeapAllocator,
                        descriptorHandle)),
        };

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

    private static List<int>[] InitializeDeferredFreeIndices(
        ushort frameBufferCount)
    {
        var deferredFreeIndices = new List<int>[frameBufferCount];
        for (var i = 0; i < deferredFreeIndices.Length; ++i)
        {
            deferredFreeIndices[i] = [];
        }
        return deferredFreeIndices;
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

    public static implicit operator TDescriptorHeapAllocator(
        DescriptorHeapAllocator<TDescriptorHeapAllocator> allocator)
        => allocator as TDescriptorHeapAllocator
        ?? throw new UnreachableException();
}

static file class DescriptorHeapAllocatorValidationExtensions
{
    internal readonly ref struct IndexDescriptorHandle<TDescriptorHeapAllocator>(
        uint index,
        DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
    {
        public uint Index { get; } = index;
        public DescriptorHandle<TDescriptorHeapAllocator> DescriptorHandle { get; } = descriptorHandle;
    }

    internal static ValueResult<IndexDescriptorHandle<TDescriptorHeapAllocator>, TypedValueError> CalculateIndex<TDescriptorHeapAllocator>(
        this ValueResult<(TDescriptorHeapAllocator allocator, DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle), TypedValueError> validationContext)
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
        => validationContext
            .Bind(
                bind: static (
                    scoped in descriptorHandleValidationContext)
            => descriptorHandleValidationContext switch
            {
                {
                    allocator: var allocator,
                    descriptorHandle: { CPU.Ptr: var descriptorHandlePointer } descriptorHandle,
                }
                => ValueResult
                    .Success<IndexDescriptorHandle<TDescriptorHeapAllocator>, TypedValueError>(
                        new(
                            index: (uint)(descriptorHandlePointer - allocator.CPU_Start.Ptr) / allocator.DescriptorSize,
                            descriptorHandle: descriptorHandle))
            })
        ;

    internal static ValueResult<IndexDescriptorHandle<TDescriptorHeapAllocator>, TypedValueError> ValidateHeapDescriptorIndex<TLogger, TDescriptorHeapAllocator>(
        this ValueResult<IndexDescriptorHandle<TDescriptorHeapAllocator>, TypedValueError> indexDescriptorHandleResult,
        scoped in TLogger logger)
        where TLogger : ILogger
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
        => indexDescriptorHandleResult
            .Ensure(
                predicate: static (scoped in idh)
                    => idh.Index == idh.DescriptorHandle.Index,
                onError: static (scoped in idh)
                    => new(
                        TypedValueError.ErrorTypes.InvalidOperation,
                        new InvalidOperationError("Invalid heap descriptor index!")))
            .TapError(
                param1: logger,
                tapError: static (scoped in _, scoped in logger)
                    => LogInvalidHeapDescriptorIndex(logger))
        ;

    internal static void AddIndex<TDescriptorHeapAllocator>(
        scoped in IndexDescriptorHandle<TDescriptorHeapAllocator> idh,
        scoped in ID3D12RenderingEngine renderingEngine,
        scoped in List<int>[] deferredFreeIndices)
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
        => deferredFreeIndices[renderingEngine.CurrentFrameIndex()].Add((int)idh.Index);

    extension<TDescriptorHeapAllocator>(scoped in ValueResult<TDescriptorHeapAllocator, ValueError> allocatorResult)
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
    {
        internal ValueResult<TDescriptorHeapAllocator, ValueError> SetCapacity(
            scoped in ILogger logger,
            scoped in uint capacity,
            scoped in SetCapacityDelegate<TDescriptorHeapAllocator> setCapacityDelegate) => allocatorResult
                .ErrorIfCapacityZero(logger, capacity)
                .ErrorIfCapacityGreaterThanHeapSize(logger, capacity)
                .ErrorIfCapacityGreaterThanVisibleSamplerHeapSize(logger, capacity)
                .Tap(
                    param1: setCapacityDelegate,
                    param2: capacity,
                    tap: static (scoped in allocator, scoped in setCapacityDelegate, scoped in capacity)
                        => setCapacityDelegate(allocator, capacity))
                ;

        private ValueResult<TDescriptorHeapAllocator, ValueError> ErrorIfCapacityZero(
            scoped in ILogger logger,
            scoped in uint capacity)
            => allocatorResult
                .Ensure(
                    param1: logger,
                    param2: capacity,
                    predicate: static (scoped in _, scoped in _, scoped in capacity) => capacity is not 0,
                    onError: static (scoped in _, scoped in logger) =>
                    {
                        LogCapacityCannotBeZero(logger);
                        return new ValueError("Capacity cannot be 0!");
                    });

        private ValueResult<TDescriptorHeapAllocator, ValueError> ErrorIfCapacityGreaterThanHeapSize(
            scoped in ILogger logger,
            scoped in uint capacity)
            => allocatorResult
                .Ensure(
                    param1: logger,
                    param2: capacity,
                    predicate: static (scoped in _, scoped in _, scoped in capacity) => capacity <= D3D12.MaxShaderVisibleDescriptorHeapSizeTier2,
                    onError: static (scoped in _, scoped in logger, scoped in capacity) =>
                    {
                        logger.LogCapacityCannotBeGreaterThanMaxShaderVisibleDescriptorHeapSizeTier2(capacity, D3D12.MaxShaderVisibleDescriptorHeapSizeTier2);
                        return new ValueError($"Capacity cannot be greater than {D3D12.MaxShaderVisibleDescriptorHeapSizeTier2}!");
                    });

        private ValueResult<TDescriptorHeapAllocator, ValueError> ErrorIfCapacityGreaterThanVisibleSamplerHeapSize(
            scoped in ILogger logger,
            scoped in uint capacity)
            => allocatorResult
                .Ensure(
                    param1: logger,
                    param2: capacity,
                    predicate: static (scoped in allocator, scoped in logger, scoped in capacity)
                        => allocator.HeapType switch
                        {
                            DescriptorHeapType.Sampler
                            when capacity > D3D12.MaxShaderVisibleSamplerHeapSize
                                => false,
                            _
                                => true
                        },
                    onError: static (scoped in allocator, scoped in logger, scoped in capacity)
                        =>
                        {
                            logger.LogCapacityCannotBeGreaterThanMaxShaderVisibleSamplerHeapSize(capacity, D3D12.MaxShaderVisibleSamplerHeapSize, allocator.HeapType);
                            return new ValueError($"Capacity {capacity} cannot be greater than {D3D12.MaxShaderVisibleSamplerHeapSize} for {allocator.HeapType} heap!");
                        })
                 ;
    }

    internal delegate void SetCapacityDelegate<TDescriptorHeapAllocator>(
        scoped in TDescriptorHeapAllocator allocator,
        scoped in uint capacity)
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>;

}