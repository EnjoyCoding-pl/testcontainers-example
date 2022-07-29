using Microsoft.EntityFrameworkCore;
using TestcontainersExample.Api.Context;
using TestcontainersExample.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContextPool<PostgresDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("postgres"), options => options.EnableRetryOnFailure(3));
        options.UseSnakeCaseNamingConvention();
    }
);
builder.Services.AddTransient<IBeerRepository, PostgresBeerRepository>();
var app = builder.Build();
app.MapGet("/", () => "Hello World!");

app.Run();