using Microsoft.EntityFrameworkCore;
using TestcontainersExample.Api.Context;
using TestcontainersExample.Api.Models;

namespace TestcontainersExample.Api.Repositories;

public class PostgresBeerRepository : IBeerRepository
{
    private readonly PostgresDbContext _postgresDbContext;

    public PostgresBeerRepository(PostgresDbContext postgresDbContext)
    {
        _postgresDbContext = postgresDbContext;
    }

    public async Task<Beer> GetById(int id, CancellationToken cancellationToken = default)
    {
        return await _postgresDbContext.Beers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task Add(Beer beer, CancellationToken cancellationToken = default)
    {
        _postgresDbContext.Beers.Add(beer);
        await _postgresDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        _postgresDbContext.Beers.Remove(new Beer { Id = id });
        await _postgresDbContext.SaveChangesAsync(cancellationToken);
    }
}