using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestcontainersExample.Api.Context;
using TestcontainersExample.Api.Models;
using TestcontainersExample.Api.Repositories;
using Xunit;

namespace TestcontainersExample.Api.IntegrationTests;

public class BeerRepositoryTests : IAsyncLifetime
{
    private readonly TestcontainerDatabase _database = new TestcontainersBuilder<PostgreSqlTestcontainer>()
        .WithCleanUp(true)
        .WithDatabase(new PostgreSqlTestcontainerConfiguration
        {
            Database = "test",
            Password = "postgres",
            Username = "postgres"
        })
        .Build();

    private PostgresDbContext _sharedDbContext;

    [Fact]
    public async Task Add_CompleteWithoutError()
    {
        var repository = new PostgresBeerRepository(_sharedDbContext);
        await repository.Add(new Beer { Name = "Paulaner" });
    }

    [Fact]
    public async Task GetById_ReturnsBeer()
    {
        var addedBeer = _sharedDbContext.Add(new Beer { Name = "Tyskie" });
        await _sharedDbContext.SaveChangesAsync();

        var repository = new PostgresBeerRepository(_sharedDbContext);
        var beer = await repository.GetById(addedBeer.Entity.Id);

        beer.Name.Should().Be("Tyskie");
    }

    [Fact]
    public async Task Delete_CompleteWithoutErrors()
    {
        var addedBeer = _sharedDbContext.Add(new Beer { Name = "Heineken" });
        await _sharedDbContext.SaveChangesAsync();

        var repository = new PostgresBeerRepository(_sharedDbContext);
        await repository.Delete(addedBeer.Entity.Id);
    }

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        _sharedDbContext = new PostgresDbContext(new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql(_database.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options);
        await _sharedDbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await _sharedDbContext.DisposeAsync();
    }
}