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

            if (SetCapacity(capacity)
                is { IsError: true } capacitySetResult)
            {
                return capacitySetResult
                    .TapError(
                        param1: _logger,
                        param2: HeapType,
                        static (scoped in error, scoped in logger, scoped in heapType) =>
                            logger.LogFailedToSetCapacityForDescriptorHeapAllocator(
                                heapType,
                                error.Message))
                    .Match(
                        static (scoped in _) => throw new UnreachableException(), //this won't ever happen, since we're in IsError context already
                        static (scoped in error) => ValueResult.Failure<TDescriptorHeapAllocator>(error))
                    ;
            }

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
                return ValueResult.Failure<TDescriptorHeapAllocator>("Error creating descriptor heap!");
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
                    return ValueResult.Failure<TDescriptorHeapAllocator>(
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

            return ValueResult.Success<TDescriptorHeapAllocator>(this);
        }
    }

    private ValueResult<uint, ValueError> SetCapacity(
        uint capacity)
        => ValueResult
            .ErrorIfZero(
                val: capacity,
                logger: _logger,
                logOnFailure: LogCapacityCannotBeZero)
            .ErrorIfGreaterThen((uint)D3D12.MaxShaderVisibleDescriptorHeapSizeTier2, nameof(capacity))
            .ValidateCapacityForSamplerHeap(HeapType)
            .Tap(
                param1: this,
                tap: static (scoped in capacity, scoped in @this)
                => @this.Capacity = capacity)
            ;

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
                param1: _d3D12RenderingEngine.CurrentFrameIndex(),
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
    internal static ValueResult<uint, ValueError> ValidateCapacityForSamplerHeap(
        this ValueResult<uint, ValueError> capacityResult,
        DescriptorHeapType heapType)
        => capacityResult
            .Bind(
                param1: heapType,
                bind: static (scoped in capacity, scoped in heapType)
                => heapType is DescriptorHeapType.Sampler
                    ? ValueResult.ErrorIfGreaterThen(
                        capacity,
                        (uint)D3D12.MaxShaderVisibleSamplerHeapSize)
                    : ValueResult.Success(capacity)
                )
        ;

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
            => descriptorHandleValidationContext is
            {
                allocator: var allocator,
                descriptorHandle: var descriptorHandle
                and { CPU.Ptr: var descriptorHandlePointer },
            }
                ? ValueResult
                    .Success<IndexDescriptorHandle<TDescriptorHeapAllocator>, TypedValueError>(
                        new(
                            index: (uint)(descriptorHandlePointer - allocator.CPU_Start.Ptr) / allocator.DescriptorSize,
                            descriptorHandle: descriptorHandle))
                : TypedValueError.Unreachable<IndexDescriptorHandle<TDescriptorHeapAllocator>>("Unreachable state!"))
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
        scoped in uint frameIndex,
        scoped in List<int>[] deferredFreeIndices)
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
        => deferredFreeIndices[frameIndex].Add((int)idh.Index);
}