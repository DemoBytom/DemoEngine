// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Vortice.Direct3D12;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

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
    public readonly bool IsShaderVisible => GPU is not null;

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

    internal DescriptorHandle(
        TDescriptorHeapAllocator heapAllocator,
        int index)
        : this(
            cpu: new CpuDescriptorHandle(
                other: heapAllocator.CPU_Start,
                offsetInDescriptors: index,
                descriptorIncrementSize: heapAllocator.DescriptorSize),
            gpu: heapAllocator.IsShaderVisible
                ? new GpuDescriptorHandle(
                    other: heapAllocator.GPU_Start.Value,
                    offsetInDescriptors: index,
                    descriptorIncrementSize: heapAllocator.DescriptorSize)
                : null,
            heapAlocator: heapAllocator,
            index: index)
    {
    }

    public void Dispose()
        => Container.Free(this);
}