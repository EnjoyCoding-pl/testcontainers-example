using TestcontainersExample.Api.Models;

namespace TestcontainersExample.Api.Repositories;

public interface IBeerRepository
{
    Task<Beer> GetById(int id, CancellationToken cancellationToken = default);
    Task Add(Beer beer, CancellationToken cancellationToken = default);
    Task Delete(int id, CancellationToken cancellationToken = default);
}