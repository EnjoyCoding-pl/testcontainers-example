using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TestcontainersExample.Api.Models;
using TestcontainersExample.Api.Repositories;

namespace TestcontainersExample.Api.Decorators;

public class BeerRepositoryCacheDecorator : IBeerRepository
{
    private readonly IBeerRepository _beerRepository;
    private readonly IDistributedCache _distributedCache;
    private const string _keyPrefix = "beer_";

    public BeerRepositoryCacheDecorator(IBeerRepository beerRepository, IDistributedCache distributedCache)
    {
        _beerRepository = beerRepository;
        _distributedCache = distributedCache;
    }

    public async Task<Beer> GetById(int id, CancellationToken cancellationToken = default)
    {
        var cachedBeer = await _distributedCache.GetAsync($"{_keyPrefix}{id}", cancellationToken);
        if (cachedBeer is { Length: > 0 }) return JsonSerializer.Deserialize<Beer>(cachedBeer);
        var dbBeer = await _beerRepository.GetById(id, cancellationToken);
        await _distributedCache.SetAsync(id.ToString(), JsonSerializer.SerializeToUtf8Bytes(dbBeer), cancellationToken);
        return dbBeer;
    }

    public async Task Add(Beer beer, CancellationToken cancellationToken = default)
    {
        await _beerRepository.Add(beer, cancellationToken);
        await _distributedCache.SetAsync($"{_keyPrefix}{beer.Id}", JsonSerializer.SerializeToUtf8Bytes(beer), cancellationToken);
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        await _beerRepository.Delete(id, cancellationToken);
        await _distributedCache.RemoveAsync($"{_keyPrefix}{id}", cancellationToken);
    }
}