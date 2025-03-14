// Copyright © Michał Dembski and contributors.
// Distributed under MIT license. See LICENSE file in the root for more information.

using System.Diagnostics.CodeAnalysis;
using Demo.Tools.Common.Collections;
using Shouldly;
using Xunit;

namespace Demo.Tools.Common.UTs.Collections;

public class CircularQueueTests
{
    [Fact]
    public void Enqueue_Enqueue_1()
    {
        var circQueue = new CircularQueue<int>(2);
        circQueue.Enqueue(123);
        circQueue.Count.ShouldBe(1);

        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(123);
        buffer[1].ShouldBe(0);
    }

    [Fact]
    public void Enqueue_Enqueue_2()
    {
        var circQueue = new CircularQueue<int>(2);
        circQueue.Enqueue(123);
        circQueue.Enqueue(456);
        circQueue.Count.ShouldBe(2);

        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(123);
        buffer[1].ShouldBe(456);
    }

    [Fact]
    public void Enqueue_Enqueue_3()
    {
        var circQueue = new CircularQueue<int>(2);
        circQueue.Enqueue(123);
        circQueue.Enqueue(456);
        circQueue.Enqueue(789);
        circQueue.Count.ShouldBe(2);

        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(456);
        buffer[1].ShouldBe(789);
    }

    [Fact]
    public void Enqueue_Dequeue_1()
    {
        var circQueue = new CircularQueue<int>(2);
        circQueue.Enqueue(123);
        circQueue.Enqueue(456);
        circQueue.Enqueue(789);
        var dequeued = circQueue.Dequeue();

        circQueue.Count.ShouldBe(1);
        dequeued.ShouldBe(456);
        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(789);
        buffer[1].ShouldBe(0);
    }

    [Fact]
    public void Enqueue_3_Dequeue_1_Then_Enqueue_1()
    {
        var circQueue = new CircularQueue<int>(2);
        circQueue.Enqueue(123);
        circQueue.Enqueue(456);
        circQueue.Enqueue(789);
        var dequeued = circQueue.Dequeue();
        circQueue.Enqueue(159);

        circQueue.Count.ShouldBe(2);
        dequeued.ShouldBe(456);
        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(789);
        buffer[1].ShouldBe(159);
    }

    [Fact]
    public void Dequeue_Empty_Throws_Exception()
    {
        var circQueue = new CircularQueue<int>(2);

        Action act = () => circQueue.Dequeue();
        _ = act.ShouldThrow<InvalidOperationException>();

        circQueue.Count.ShouldBe(0);
        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(0);
        buffer[1].ShouldBe(0);
    }

    [Fact]
    public void Dequeue_TryDequeue_On_Empty_Should_Not_Throw_Error()
    {
        var circQueue = new CircularQueue<int>(2);
        var peeked = circQueue.TryDequeue(out var result);

        peeked.ShouldBeFalse();
        result.ShouldBe(default);

        circQueue.Count.ShouldBe(0);
        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(0);
        buffer[1].ShouldBe(0);
    }

    [Fact]
    public void Enqueue2_Peek()
    {
        var circQueue = new CircularQueue<int>(2);
        circQueue.Enqueue(123);
        circQueue.Enqueue(456);
        circQueue.Enqueue(789);
        var peaked = circQueue.Peek();

        circQueue.Count.ShouldBe(2);
        peaked.ShouldBe(456);
        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(456);
        buffer[1].ShouldBe(789);
    }

    [Fact]
    public void Enqueue2_Peek_Peek()
    {
        var circQueue = new CircularQueue<int>(2);
        circQueue.Enqueue(123);
        circQueue.Enqueue(456);
        circQueue.Enqueue(789);
        var peaked1 = circQueue.Peek();
        var peaked2 = circQueue.Peek();

        circQueue.Count.ShouldBe(2);
        peaked1.ShouldBe(456);
        peaked2.ShouldBe(456);

        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(456);
        buffer[1].ShouldBe(789);
    }

    [Fact]
    [SuppressMessage(
        "Performance",
        "CA1806:Do not ignore method results",
        Justification = "Test method testing the constructor creations")]
    public void Constructor_negative_capacity()
    {
        Action circQueue = () => new CircularQueue<int>(-2);
        _ = circQueue.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void Queue_Peek_On_Emplty_Throws_Error()
    {
        var circQueue = new CircularQueue<int>(2);
        Action peek = () => circQueue.Peek();

        _ = peek.ShouldThrow<InvalidOperationException>();

        circQueue.Count.ShouldBe(0);
        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(0);
        buffer[1].ShouldBe(0);
    }

    [Fact]
    public void Queue_TryPeek_On_Empty_Should_Not_Throw_Error()
    {
        var circQueue = new CircularQueue<int>(2);
        var peeked = circQueue.TryPeek(out var result);

        peeked.ShouldBeFalse();
        result.ShouldBe(default);

        circQueue.Count.ShouldBe(0);
        var buffer = new int[2];
        circQueue.CopyTo(buffer, 0);
        buffer[0].ShouldBe(0);
        buffer[1].ShouldBe(0);
    }
}