// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using Demo.Tools.Common.ValueResults;

namespace Demo.Engine.Platform.DirectX12.RenderingResources;

internal static class DescHeapAllocatorBuilderExtensions
{
    internal readonly ref struct DescriptorHeapAllocatorsBuilderResult<T1, T2>(
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

    internal readonly ref struct DescriptorHeapAllocatorsBuilderResult<T1, T2, T3>(
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

    internal readonly ref struct DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4>(
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

        private static ValueResult<TResult, ValueError> MapResult(
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

    extension(ID3D12RenderingEngine renderingEngine)
    {
        internal ValueResult<DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4>, ValueError> CreateDescriptorHeaps<T1, T2, T3, T4>(
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
                        => DescriptorHeapAllocatorsBuilderResult<T1, T2>.MapResult(allocators, renderingEngine, action2))
                .Bind(
                    param1: renderingEngine,
                    param2: action3,
                    bind: static (scoped in allocators, scoped in renderingEngine, scoped in action3)
                        => DescriptorHeapAllocatorsBuilderResult<T1, T2, T3>.MapResult(allocators, renderingEngine, action3))
                .Bind(
                    param1: renderingEngine,
                    param2: action4,
                    bind: static (scoped in allocators, scoped in renderingEngine, scoped in action4)
                        => DescriptorHeapAllocatorsBuilderResult<T1, T2, T3, T4>.MapResult(allocators, renderingEngine, action4))
            ;
    }

    extension(DescHeapAllocatorBuilder builder)
    {
        internal ValueResult<RTVDescriptorHeapAllocator, ValueError> RTV(
            uint capacity,
            bool isShaderVisible)
            => builder
                .CreateDescriptorHeapAllocator(
                    capacity,
                    isShaderVisible,
                    createAllocator: renderingEngine
                        => new RTVDescriptorHeapAllocator(renderingEngine));

        internal ValueResult<DSVDescriptorHeapAllocator, ValueError> DSV(
            uint capacity,
            bool isShaderVisible)
            => builder
                .CreateDescriptorHeapAllocator(
                    capacity,
                    isShaderVisible,
                    createAllocator: renderingEngine
                        => new DSVDescriptorHeapAllocator(renderingEngine));

        internal ValueResult<SRVDescriptorHeapAllocator, ValueError> SRV(
            uint capacity,
            bool isShaderVisible)
            => builder
                .CreateDescriptorHeapAllocator(
                    capacity,
                    isShaderVisible,
                    createAllocator: renderingEngine
                        => new SRVDescriptorHeapAllocator(renderingEngine));

        internal ValueResult<UAVDescriptorHeapAllocator, ValueError> UAV(
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

    internal interface IDescriptorHeapAllocatorsBuilderResult<TResult, TAllocators, TParam>
        where TResult : IDescriptorHeapAllocatorsBuilderResult<TResult, TAllocators, TParam>, allows ref struct
        where TAllocators : allows ref struct
        where TParam : DescriptorHeapAllocator<TParam>
    {
        static abstract TResult Create(TAllocators param1, TParam param2);
    }

    internal readonly ref struct DescHeapAllocatorBuilder(
        ID3D12RenderingEngine renderingEngine)
    {
        public ID3D12RenderingEngine RenderingEngine { get; } = renderingEngine;
    }
}