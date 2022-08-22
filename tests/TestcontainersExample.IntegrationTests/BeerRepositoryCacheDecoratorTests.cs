using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using TestcontainersExample.Api.Context;
using TestcontainersExample.Api.Decorators;
using TestcontainersExample.Api.Models;
using TestcontainersExample.Api.Repositories;
using Xunit;

namespace TestcontainersExample.Api.IntegrationTests;

public class BeerRepositoryCacheDecoratorTests : IAsyncLifetime
{
    private PostgresDbContext _sharedDbContext;

    private readonly TestcontainerDatabase _redis = new TestcontainersBuilder<RedisTestcontainer>()
        .WithDatabase(new RedisTestcontainerConfiguration())
        .WithCleanUp(true)
        .Build();

    private readonly TestcontainerDatabase _database = new TestcontainersBuilder<PostgreSqlTestcontainer>()
        .WithCleanUp(true)
        .WithDatabase(new PostgreSqlTestcontainerConfiguration
        {
            Database = "test",
            Password = "postgres",
            Username = "postgres"
        })
        .Build();

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();
        await _database.StartAsync();
        _sharedDbContext = new PostgresDbContext(new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql(_database.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options);
        await _sharedDbContext.Database.MigrateAsync();
    }

    private BeerRepositoryCacheDecorator Decorator => new BeerRepositoryCacheDecorator(
        new PostgresBeerRepository(_sharedDbContext),
        new RedisCache(new RedisCacheOptions { Configuration = _redis.ConnectionString, InstanceName = "TestcontainersExample" })
    );

    [Fact]
    public async Task Add_ReturnsCachedBeer()
    {
        var decorator = Decorator;

        await decorator.Add(new Beer { Id = 1, Name = "Beer" });

        var beer = await decorator.GetById(1);

        beer.Id.Should().Be(1);
        beer.Name.Should().Be("Beer");
    }

    [Fact]
    public async Task Delete_ReturnsNull()
    {
        var decorator = Decorator;

        await decorator.Add(new Beer { Id = 2, Name = "Beer" });
        await decorator.Delete(2);

        var beer = await decorator.GetById(2);
        beer.Should().BeNull();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await _redis.DisposeAsync();
    }
}