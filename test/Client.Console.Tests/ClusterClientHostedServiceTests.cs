﻿using Grains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Client.Console.Tests
{
    [Collection(nameof(ClusterCollection))]
    public class ClusterClientHostedServiceTests
    {
        public ClusterClientHostedServiceTests()
        {
            logger = Mock.Of<ILogger>();
            loggerProvider = Mock.Of<ILoggerProvider>(_ => _.CreateLogger(It.IsAny<string>()) == logger);
        }

        private readonly ILogger logger;
        private readonly ILoggerProvider loggerProvider;

        [Fact]
        public void RefusesNullConfiguration()
        {
            var loggerProvider = Mock.Of<ILoggerProvider>();
            var error = Assert.Throws<ArgumentNullException>(() =>
            {
                new ClusterClientHostedService(null, loggerProvider);
            });
            Assert.Equal("configuration", error.ParamName);
        }

        [Fact]
        public void RefusesNullLoggerProvider()
        {
            var config = new Mock<IConfiguration>();
            var error = Assert.Throws<ArgumentNullException>(() =>
            {
                new ClusterClientHostedService(config.Object, null);
            });
            Assert.Equal("loggerProvider", error.ParamName);
        }

        [Fact]
        public void BuildsClusterClient()
        {
            // arrange
            var id = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Orleans:Ports:Gateway:Start", "30000" },
                    { "Orleans:Ports:Gateway:End", "30000" },
                    { "Orleans:ClusterId", "dev" },
                    { "Orleans:ServiceId", "dev" },
                    { "Orleans:Providers:Clustering:Provider", "Localhost" }
                })
                .Build();

            // act
            var service = new ClusterClientHostedService(config, loggerProvider);

            // assert
            Assert.NotNull(service.ClusterClient);
            Assert.False(service.ClusterClient.IsInitialized);
        }

        [Fact]
        public async Task ConnectsClusterClient()
        {
            // arrange
            var id = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Orleans:Ports:Gateway:Start", "30000" },
                    { "Orleans:Ports:Gateway:End", "30000" },
                    { "Orleans:ClusterId", "dev" },
                    { "Orleans:ServiceId", "dev" },
                    { "Orleans:Providers:Clustering:Provider", "Localhost" }
                })
                .Build();

            // act
            var service = new ClusterClientHostedService(config, loggerProvider);
            await service.StartAsync(default(CancellationToken));

            // assert
            Assert.NotNull(service.ClusterClient);
            Assert.True(service.ClusterClient.IsInitialized);
            Assert.Equal(id, await service.ClusterClient.GetGrain<ITestGrain>(id).GetKeyAsync());
        }

        [Fact]
        public async Task FailsToConnectClusterClient()
        {
            // arrange
            var id = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Orleans:Ports:Gateway:Start", "12345" },
                    { "Orleans:Ports:Gateway:End", "12345" },
                    { "Orleans:ClusterId", "dev" },
                    { "Orleans:ServiceId", "dev" },
                    { "Orleans:Providers:Clustering:Provider", "Localhost" }
                })
                .Build();

            // act and assert
            var service = new ClusterClientHostedService(config, loggerProvider);
            await Assert.ThrowsAsync<OrleansMessageRejectionException>(async () =>
            {
                await service.StartAsync(default(CancellationToken));
            });
        }

        [Fact]
        public async Task DisconnectsClusterClient()
        {
            // arrange
            var id = Guid.NewGuid();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Orleans:Ports:Gateway:Start", "30000" },
                    { "Orleans:Ports:Gateway:End", "30000" },
                    { "Orleans:ClusterId", "dev" },
                    { "Orleans:ServiceId", "dev" },
                    { "Orleans:Providers:Clustering:Provider", "Localhost" }
                })
                .Build();

            // act
            var service = new ClusterClientHostedService(config, loggerProvider);
            await service.StartAsync(default(CancellationToken));
            await service.StopAsync(default(CancellationToken));

            // assert
            Assert.False(service.ClusterClient.IsInitialized);
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            {
                await service.ClusterClient.GetGrain<ITestGrain>(id).GetKeyAsync();
            });
        }
    }
}
