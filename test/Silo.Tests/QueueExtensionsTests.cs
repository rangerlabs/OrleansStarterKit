﻿using Grains;
using System;
using System.Collections.Generic;
using Xunit;

namespace Silo.Tests
{
    public class QueueExtensionsTests
    {
        [Fact]
        public void Queue_Enqueue_Respects_Capacity()
        {
            // arrange
            var queue = new Queue<int>();
            var capacity = 3;

            // act
            queue.Enqueue(101, capacity);
            queue.Enqueue(102, capacity);
            queue.Enqueue(103, capacity);
            queue.Enqueue(104, capacity);

            // assert
            var count = queue.Count;
            var items = new List<int>();
            while (queue.Count > 0)
            {
                items.Add(queue.Dequeue());
            }

            Assert.Equal(capacity, count);
            Assert.Equal(capacity, items.Count);
            Assert.Equal(102, items[0]);
            Assert.Equal(103, items[1]);
            Assert.Equal(104, items[2]);
        }

        [Fact]
        public void Queue_Enqueue_Refuses_Null()
        {
            // assert
            Assert.Throws<ArgumentNullException>(() => ((Queue<int>)null).Enqueue(1, 1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Queue_Enqueue_Refuses_Low_Capacity(int capacity)
        {
            // arrange
            var queue = new Queue<int>();

            // assert
            Assert.Throws<ArgumentOutOfRangeException>(() => queue.Enqueue(1, capacity));
        }
    }
}
