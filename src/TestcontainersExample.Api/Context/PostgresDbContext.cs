using Microsoft.EntityFrameworkCore;
using TestcontainersExample.Api.Context.Configurations;
using TestcontainersExample.Api.Models;

namespace TestcontainersExample.Api.Context;

public class PostgresDbContext:DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options):base(options)
    {
        
    }
    public DbSet<Beer> Beers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BeerConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}