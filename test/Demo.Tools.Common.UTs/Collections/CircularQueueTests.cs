using System;
using Demo.Tools.Common.Collections;
using FluentAssertions;
using Xunit;

namespace Demo.Tools.Common.UTs.Collections
{
    public class CircularQueueTests
    {
        [Fact]
        public void Enqueue_Enqueue_1()
        {
            var circQueue = new CircularQueue<int>(2);
            circQueue.Enqueue(123);
            circQueue.Count.Should().Be(1);

            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(123);
            buffer[1].Should().Be(0);
        }

        [Fact]
        public void Enqueue_Enqueue_2()
        {
            var circQueue = new CircularQueue<int>(2);
            circQueue.Enqueue(123);
            circQueue.Enqueue(456);
            circQueue.Count.Should().Be(2);

            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(123);
            buffer[1].Should().Be(456);
        }

        [Fact]
        public void Enqueue_Enqueue_3()
        {
            var circQueue = new CircularQueue<int>(2);
            circQueue.Enqueue(123);
            circQueue.Enqueue(456);
            circQueue.Enqueue(789);
            circQueue.Count.Should().Be(2);

            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(456);
            buffer[1].Should().Be(789);
        }

        [Fact]
        public void Enqueue_Dequeue_1()
        {
            var circQueue = new CircularQueue<int>(2);
            circQueue.Enqueue(123);
            circQueue.Enqueue(456);
            circQueue.Enqueue(789);
            var dequeued = circQueue.Dequeue();

            circQueue.Count.Should().Be(1);
            dequeued.Should().Be(456);
            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(789);
            buffer[1].Should().Be(0);
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

            circQueue.Count.Should().Be(2);
            dequeued.Should().Be(456);
            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(789);
            buffer[1].Should().Be(159);
        }

        [Fact]
        public void Dequeue_Empty_Throws_Exception()
        {
            var circQueue = new CircularQueue<int>(2);

            Action act = () => circQueue.Dequeue();
            act.Should().Throw<InvalidOperationException>();

            circQueue.Count.Should().Be(0);
            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(0);
            buffer[1].Should().Be(0);
        }

        [Fact]
        public void Enqueue2_Peek()
        {
            var circQueue = new CircularQueue<int>(2);
            circQueue.Enqueue(123);
            circQueue.Enqueue(456);
            circQueue.Enqueue(789);
            var peaked = circQueue.Peek();

            circQueue.Count.Should().Be(2);
            peaked.Should().Be(456);
            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(456);
            buffer[1].Should().Be(789);
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

            circQueue.Count.Should().Be(2);
            peaked1.Should().Be(456);
            peaked2.Should().Be(456);

            var buffer = new int[2];
            circQueue.CopyTo(buffer, 0);
            buffer[0].Should().Be(456);
            buffer[1].Should().Be(789);
        }

        [Fact]
        public void Constructor_negative_capacity()
        {
            Action circQueue = () => new CircularQueue<int>(-2);
            circQueue.Should().Throw<InvalidOperationException>();
        }
    }
}