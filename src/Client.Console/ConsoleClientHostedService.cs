﻿using Microsoft.Extensions.Hosting;
using Orleans;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Console
{
    public class ConsoleClientHostedService : IHostedService
    {
        public ConsoleClientHostedService(IClusterClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        private readonly IClusterClient _client;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => RunShellAsync()).Ignore();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private Task RunShellAsync()
        {
            return Task.CompletedTask;
        }
    }
}
