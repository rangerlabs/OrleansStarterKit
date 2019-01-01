﻿using Orleans.Concurrency;
using System;

namespace Grains.Models
{
    [Immutable]
    public class Message
    {
        public Message()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        public Guid Id { get; }
        public DateTime Timestamp { get; }
    }
}
