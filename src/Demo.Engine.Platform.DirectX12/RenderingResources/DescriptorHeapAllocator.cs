// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Demo.Tools.Common.ValueResults;
using Microsoft.Extensions.Logging;
using Vortice.Direct3D12;
using static Demo.Engine.Platform.DirectX12.RenderingResources.DescriptorHeapAllocatorLoggerExtensions;

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
    private readonly ID3D12RenderingEngine _d3D12RenderingEngine = d3D12RenderingEngine;

    public ValueResult<TDescriptorHeapAllocator, ValueError> Initialize(
        uint capacity,
        bool isShaderVisible)
    {
        lock (_lock)
        {

            if (SetCapacity(capacity)
                is { IsSuccess: false } capacitySetResult)
            {
                _logger.LogFailedToSetCapacityForDescriptorHeapAllocator(
                    heapType: HeapType,
                    errorMessage: capacitySetResult.Error.Message);

                return ValueResult.Failure<TDescriptorHeapAllocator>(capacitySetResult.Error);
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

            return ValueResult.Success<TDescriptorHeapAllocator>(this);
        }
    }

    private ValueResult<ValueError> SetCapacity(
        uint capacity)
        => ValueResultExtensions
            .ErrorIfZero(
                value: capacity,
                logger: _logger,
                logOnFailure: LogCapacityCannotBeZero)
            .ErrorIfGreaterThen((uint)D3D12.MaxShaderVisibleDescriptorHeapSizeTier2, nameof(capacity))
            .ValidateCapacityForSamplerHeap(HeapType)
            .SetCapacity(capacity
                => Capacity = capacity)
            .Match(
                onSuccess: _
                    => ValueResult.Success(),
                onFailure: failure
                    => ValueResult.Failure(failure))
            ;

    public DescriptorHandle<TDescriptorHeapAllocator> Allocate()
    {
        lock (_lock)
        {
            ValidateHeapIsNotNull();
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(
                value: Size,
                other: Capacity);

            var index = _freeHandles[Size];

            ++Size;

            return new DescriptorHandle<TDescriptorHeapAllocator>(
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
        in DescriptorHandle<TDescriptorHeapAllocator> descriptorHandle)
    {
        lock (_lock)
        {
            ArgumentOutOfRangeException.ThrowIfZero(Size);

            ValidateDescriptorHandle(
                _logger,
                descriptorHeapAllocator: this,
                descriptorHandle: descriptorHandle)
            .CalculateIndex()
            .ValidateHeapDescriptorIndex(_logger)
            .AddIndex(
                frameIndex: _d3D12RenderingEngine.CurrentFrameIndex(),
                deferredFreeIndices: _deferredFreeIndices)
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

internal readonly struct DescriptorHandle<TDescriptorHeapAllocator>
    : IDisposable
    where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
{
    public CpuDescriptorHandle? CPU { get; }
    public GpuDescriptorHandle? GPU { get; }
    public TDescriptorHeapAllocator Container { get; }
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
        TDescriptorHeapAllocator heapAlocator,
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

internal static class DescriptorHeapAllocatorExtensions
{
    public readonly ref struct DescriptorHeapAllocatorsBuilderResult<T1, T2>(
        T1 allocator1,
        T2 allocator2)
        : IDescriptorHeapAllocatorsBuilderResult<
            DescriptorHeapAllocatorsBuilderResult<T1, T2>,
            T1,
            T2>
        where T1 : DescriptorHeapAllocator<T1>
        where T2 : DescriptorHeapAllocator<T2>
    {
        public T1 Allocator1 { get; } = allocator1;
        public T2 Allocator2 { get; } = allocator2;

        public void Deconstruct(
            out T1 allocator1,
            out T2 allocator2)
        {
            allocator1 = Allocator1;
            allocator2 = Allocator2;
        }

        public static DescriptorHeapAllocatorsBuilderResult<T1, T2> Create(
            T1 param1,
            T2 param2)
            => new(
                allocator1: param1,
                allocator2: param2);
    }

    public readonly ref struct DescriptorHeapAllocatorsBuilderResult<T1, T2, T3>(
        T1 allocator1,
        T2 allocator2,
        T3 allocator3)
        : IDescriptorHeapAllocatorsBuilderResult<
            DescriptorHeapAllocatorsBuilderResult<T1, T2, T3>,
            DescriptorHeapAllocatorsBuilderResult<T1, T2>,
            T3>
        where T1 : DescriptorHeapAllocator<T1>
        where T2 : DescriptorHeapAllocator<T2>
        where T3 : DescriptorHeapAllocator<T3>
    {
        public T1 Allocator1 { get; } = allocator1;
        public T2 Allocator2 { get; } = allocator2;
        public T3 Allocator3 { get; } = allocator3;

        public void Deconstruct(
            out T1 allocator1,
            out T2 allocator2,
            out T3 allocator3)
        {
            allocator1 = Allocator1;
            allocator2 = Allocator2;
            allocator3 = Allocator3;
        }

        public static DescriptorHeapAllocatorsBuilderResult<T1, T2, T3> Create(
            DescriptorHeapAllocatorsBuilderResult<T1, T2> param1,
            T3 param2)
            => new(
                allocator1: param1.Allocator1,
                allocator2: param1.Allocator2,
                allocator3: param2);
    }

    public readonly ref struct DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4>(
        T1 allocator1,
        T2 allocator2,
        T3 allocator3,
        T4 allocator4)
        : IDescriptorHeapAllocatorsBuilderResult<
            DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4>,
            DescriptorHeapAllocatorsBuilderResult<T1, T2, T3>,
            T4>
        where T1 : DescriptorHeapAllocator<T1>
        where T2 : DescriptorHeapAllocator<T2>
        where T3 : DescriptorHeapAllocator<T3>
        where T4 : DescriptorHeapAllocator<T4>
    {
        public T1 Allocator1 { get; } = allocator1;
        public T2 Allocator2 { get; } = allocator2;
        public T3 Allocator3 { get; } = allocator3;
        public T4 Allocator4 { get; } = allocator4;

        public void Deconstruct(
            out T1 allocator1,
            out T2 allocator2,
            out T3 allocator3,
            out T4 allocator4)
        {
            allocator1 = Allocator1;
            allocator2 = Allocator2;
            allocator3 = Allocator3;
            allocator4 = Allocator4;
        }

        public static DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4> Create(
            DescriptorHeapAllocatorsBuilderResult<T1, T2, T3> param1,
            T4 param2)
            => new(
                allocator1: param1.Allocator1,
                allocator2: param1.Allocator2,
                allocator3: param1.Allocator3,
                allocator4: param2);
    }

    extension<TResult, TAllocators, TParam>(TResult result)
        where TResult : IDescriptorHeapAllocatorsBuilderResult<TResult, TAllocators, TParam>, allows ref struct
        where TParam : DescriptorHeapAllocator<TParam>
        where TAllocators : allows ref struct
    {

        public static ValueResult<TResult, ValueError> BindResult(
            scoped in TAllocators allocators,
            scoped in ID3D12RenderingEngine re,
            scoped in Func<DescHeapAllocatorBuilder, ValueResult<TParam, ValueError>> action)
            => action
                .Invoke(
                    new DescHeapAllocatorBuilder(re))
                .Map(
                    param1: allocators,
                    map: static (scoped in allocator, scoped in allocators)
                        => TResult.Create(allocators, allocator))
            ;
    }

    public static ValueResult<DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4>, ValueError> CreateDescriptorHeaps<T1, T2, T3, T4>(
        this ID3D12RenderingEngine renderingEngine,
        Func<DescHeapAllocatorBuilder, ValueResult<T1, ValueError>> action1,
        Func<DescHeapAllocatorBuilder, ValueResult<T2, ValueError>> action2,
        Func<DescHeapAllocatorBuilder, ValueResult<T3, ValueError>> action3,
        Func<DescHeapAllocatorBuilder, ValueResult<T4, ValueError>> action4)
        where T1 : DescriptorHeapAllocator<T1>
        where T2 : DescriptorHeapAllocator<T2>
        where T3 : DescriptorHeapAllocator<T3>
        where T4 : DescriptorHeapAllocator<T4>
        => action1
            .Invoke(
                new DescHeapAllocatorBuilder(renderingEngine))
            .Bind(
                param1: renderingEngine,
                param2: action2,
                bind: static (scoped in allocators, scoped in renderingEngine, scoped in action2)
                    => DescriptorHeapAllocatorsBuilderResult<T1, T2>.BindResult(allocators, renderingEngine, action2))
            .Bind(
                param1: renderingEngine,
                param2: action3,
                bind: static (scoped in allocators, scoped in renderingEngine, scoped in action3)
                    => DescriptorHeapAllocatorsBuilderResult<T1, T2, T3>.BindResult(allocators, renderingEngine, action3))
            .Bind(
                param1: renderingEngine,
                param2: action4,
                bind: static (scoped in allocators, scoped in renderingEngine, scoped in action4)
                    => DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4>.BindResult(allocators, renderingEngine, action4))
            ;

    internal interface IDescriptorHeapAllocatorsBuilderResult<TResult, TAllocators, TParam>
        where TResult : IDescriptorHeapAllocatorsBuilderResult<TResult, TAllocators, TParam>, allows ref struct
        where TAllocators : allows ref struct
        where TParam : DescriptorHeapAllocator<TParam>
    {
        static abstract TResult Create(TAllocators param1, TParam param2);
    }

    extension(DescHeapAllocatorBuilder builder)
    {
        public ValueResult<RTVDescriptorHeapAllocator, ValueError> RTV(
            uint capacity,
            bool isShaderVisible)
            => builder
                .CreateDescriptorHeapAllocator(
                    capacity,
                    isShaderVisible,
                    createAllocator: renderingEngine
                        => new RTVDescriptorHeapAllocator(renderingEngine));

        public ValueResult<DSVDescriptorHeapAllocator, ValueError> DSV(
            uint capacity,
            bool isShaderVisible)
            => builder
                .CreateDescriptorHeapAllocator(
                    capacity,
                    isShaderVisible,
                    createAllocator: renderingEngine
                        => new DSVDescriptorHeapAllocator(renderingEngine));

        public ValueResult<SRVDescriptorHeapAllocator, ValueError> SRV(
            uint capacity,
            bool isShaderVisible)
            => builder
                .CreateDescriptorHeapAllocator(
                    capacity,
                    isShaderVisible,
                    createAllocator: renderingEngine
                        => new SRVDescriptorHeapAllocator(renderingEngine));

        public ValueResult<UAVDescriptorHeapAllocator, ValueError> UAV(
            uint capacity,
            bool isShaderVisible)
            => builder
                .CreateDescriptorHeapAllocator(
                    capacity,
                    isShaderVisible,
                    createAllocator: static renderingEngine
                        => new UAVDescriptorHeapAllocator(renderingEngine));

        private ValueResult<TDescriptorHeapAllocator, ValueError> CreateDescriptorHeapAllocator<TDescriptorHeapAllocator>(
            uint capacity,
            bool isShaderVisible,
            Func<ID3D12RenderingEngine, TDescriptorHeapAllocator> createAllocator)
            where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
            => createAllocator(builder.RenderingEngine)
                .Initialize(capacity, isShaderVisible)
            ;
    }

    internal readonly ref struct DescHeapAllocatorBuilder(
        ID3D12RenderingEngine renderingEngine)
    {
        public ID3D12RenderingEngine RenderingEngine { get; } = renderingEngine;
    }
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
                    ? ValueResultExtensions.ErrorIfGreaterThen(
                        capacity,
                        (uint)D3D12.MaxShaderVisibleSamplerHeapSize)
                    : ValueResult.Success(capacity)
                )
        ;

    internal static ValueResult<uint, ValueError> SetCapacity(
        this ValueResult<uint, ValueError> capacityResult,
        scoped in Action<uint> setCapacity)
        => capacityResult
            .Bind(
                param1: setCapacity,
                bind: static (scoped in capacity, scoped in setCapacity)
                =>
                {
                    setCapacity(capacity);
                    return ValueResult.Success<uint, ValueError>(capacity);
                })
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
            .Bind(
                param1: logger,
                bind: static (
                    scoped in indexDescriptorHandle,
                    scoped in logger)
            => indexDescriptorHandle.DescriptorHandle.Index == indexDescriptorHandle.Index
                ? ValueResult.Success<IndexDescriptorHandle<TDescriptorHeapAllocator>, TypedValueError>(indexDescriptorHandle)
                : logger
                    .LogAndReturn(logger => LogInvalidHeapDescriptorIndex(logger))
                    .InvalidOperation<IndexDescriptorHandle<TDescriptorHeapAllocator>>(
                        errorMessage: "Invalid heap descriptor index!"))
        ;

    public static ValueResult<int, TypedValueError> AddIndex<TDescriptorHeapAllocator>(
        this ValueResult<IndexDescriptorHandle<TDescriptorHeapAllocator>, TypedValueError> indexDescriptorHandleResult,
        scoped in uint frameIndex,
        scoped in IList<int>[] deferredFreeIndices)
        where TDescriptorHeapAllocator : DescriptorHeapAllocator<TDescriptorHeapAllocator>
        => indexDescriptorHandleResult
            .Bind(
                param1: frameIndex,
                param2: deferredFreeIndices,
                bind: static (
                    scoped in idh,
                    scoped in frameIndex,
                    scoped in deferredFreeIndices)
                =>
                {
                    var index = (int)idh.Index;
                    deferredFreeIndices[frameIndex].Add(index);

                    return ValueResult.Success<int, TypedValueError>(index);
                })
                ;
}