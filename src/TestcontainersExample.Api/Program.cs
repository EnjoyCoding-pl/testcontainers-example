using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TestcontainersExample.Api.Context;
using TestcontainersExample.Api.Decorators;
using TestcontainersExample.Api.Models;
using TestcontainersExample.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextPool<PostgresDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("postgres"), options => options.EnableRetryOnFailure(3));
        options.UseSnakeCaseNamingConvention();
    }
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.InstanceName = "TestcontainersExample";
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});
builder.Services.AddTransient<PostgresBeerRepository>();
builder.Services.AddScoped<IBeerRepository>(serviceProvider =>
{
    var cache = serviceProvider.GetRequiredService<IDistributedCache>();
    var repository = serviceProvider.GetRequiredService<PostgresBeerRepository>();
    return new BeerRepositoryCacheDecorator(repository, cache);
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PostgresDbContext>().Database.MigrateAsync();
}

app.UseSwagger();
app.MapGet("/beers/{id:int}", async (int id, IBeerRepository repository, CancellationToken token) => await repository.GetById(id, token));
app.MapPost("/beers", async (Beer beer, IBeerRepository repository, CancellationToken token) => await repository.Add(beer, token));
app.MapDelete("/beers/{id:int}", async (int id, IBeerRepository repository, CancellationToken token) => await repository.Delete(id, token));
app.UseSwaggerUI();
app.Run();