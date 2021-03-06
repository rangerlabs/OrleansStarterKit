﻿using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Silo
{
    public static class Program
    {
        public static IHost Host { get; private set; }

        private static CancellationToken _cancellationToken = default;

        public static Task MainForTesting(string[] args, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return Main(args);
        }

        public static async Task Main(string[] args)
        {
            Host = new HostBuilder()
                .UseSharedConfiguration(args)
                .ConfigureServices((hosting, services) =>
                {
                    // helps discover free ports
                    services.AddSingleton<INetworkPortFinder, NetworkPortFinder>();

                    // add the silo hosted service and the services it makes available
                    services.AddSingleton<SiloHostedService>();
                    services.AddSingleton<IHostedService>(_ => _.GetService<SiloHostedService>());
                    services.AddSingleton(_ => _.GetService<SiloHostedService>().ClusterClient);

                    // add the back-end api service
                    services.AddSingleton<SupportApiHostedService>();
                    services.AddSingleton<IHostedService>(_ => _.GetService<SupportApiHostedService>());
                })
                .ConfigureLogging((hosting, configure) =>
                {
                    configure.AddSerilog(new LoggerConfiguration()
                        .WriteTo.Console(
                            restrictedToMinimumLevel: hosting.Configuration.GetValue<LogEventLevel>("Serilog:Console:RestrictedToMinimumLevel"))
                        .WriteTo.MSSqlServer(
                            connectionString: hosting.Configuration.GetConnectionString("Orleans"),
                            schemaName: hosting.Configuration["Serilog:MSSqlServer:SchemaName"],
                            tableName: hosting.Configuration["Serilog:MSSqlServer:TableName"],
                            restrictedToMinimumLevel: hosting.Configuration.GetValue<LogEventLevel>("Serilog:MSSqlServer:RestrictedToMinimumLevel"))
                        .CreateLogger());
                })
                .UseConsoleLifetime()
                .Build();

            // write the port configuration on the console title
            var silo = Host.Services.GetService<SiloHostedService>();
            var api = Host.Services.GetService<SupportApiHostedService>();
            Console.Title = $"{nameof(Silo)}: {{ Silo: {silo.SiloPort}, Gateway: {silo.GatewayPort}, Dashboard: {silo.DashboardPort}, Api: {api.Port} }}";

            await Host.RunAsync(_cancellationToken);
        }
    }
}