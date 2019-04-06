﻿using Grains.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Grains.Tests
{
    [Collection(nameof(ClusterCollection))]
    public class UserGrainTests
    {
        private readonly ClusterFixture _fixture;

        public UserGrainTests(ClusterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SetInfoAsync_Saves_State()
        {
            // arrange
            var key = Guid.NewGuid();
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IUserGrain>(key);
            var info = new UserInfo(key, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // act
            await grain.SetInfoAsync(info);

            // assert
            var context = _fixture.SiloServiceProvider.GetService<RegistryContext>();
            var state = await context.Users.FindAsync(key);
            Assert.Equal(info, state);
        }

        [Fact]
        public async Task GetInfoAsync_Gets_State()
        {
            // arrange
            var context = _fixture.SiloServiceProvider.GetService<RegistryContext>();
            var key = Guid.NewGuid();
            var info = new UserInfo(key, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            context.Users.Add(info);
            await context.SaveChangesAsync();
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IUserGrain>(key);

            // act
            var state = await grain.GetInfoAsync();

            // assert
            Assert.Equal(info, state);
        }

        [Fact]
        public async Task TellAsync_Saves_State()
        {
            // arrange
            var key = Guid.NewGuid();
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IUserGrain>(key);
            var message = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), key, Guid.NewGuid().ToString(), DateTime.Now);
            await grain.SetInfoAsync(new UserInfo(key, Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

            // act
            await grain.TellAsync(message);

            // assert
            var context = _fixture.SiloServiceProvider.GetService<RegistryContext>();
            var state = await context.Messages.FindAsync(message.Id);
            Assert.Equal(message, state);
        }

        [Fact]
        public async Task GetLatestMessagesAsync_Returns_Tells()
        {
            // arrange
            var key = Guid.NewGuid();
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IUserGrain>(key);
            await grain.SetInfoAsync(new UserInfo(key, Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));
            var message1 = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), key, Guid.NewGuid().ToString(), DateTime.Now);
            var message2 = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), key, Guid.NewGuid().ToString(), DateTime.Now);
            var message3 = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), key, Guid.NewGuid().ToString(), DateTime.Now);

            // act
            await grain.TellAsync(message1);
            await grain.TellAsync(message2);
            await grain.TellAsync(message3);
            var state = await grain.GetLatestMessagesAsync();

            // assert
            Assert.Collection(state,
                m => Assert.Equal(message1, m),
                m => Assert.Equal(message2, m),
                m => Assert.Equal(message3, m));
        }

        [Fact]
        public async Task GetLatestMessagesAsync_Returns_State()
        {
            // arrange
            var key = Guid.NewGuid();
            var grain = _fixture.Cluster.GrainFactory.GetGrain<IUserGrain>(key);
            var message1 = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), key, Guid.NewGuid().ToString(), DateTime.Now);
            var message2 = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), key, Guid.NewGuid().ToString(), DateTime.Now);
            var message3 = new Message(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), key, Guid.NewGuid().ToString(), DateTime.Now);
            var context = _fixture.SiloServiceProvider.GetService<RegistryContext>();
            context.Messages.AddRange(message1, message2, message3);
            await context.SaveChangesAsync();

            // act
            var state = await grain.GetLatestMessagesAsync();

            // assert
            Assert.Collection(state,
                m => Assert.Equal(message1, m),
                m => Assert.Equal(message2, m),
                m => Assert.Equal(message3, m));
        }
    }
}
