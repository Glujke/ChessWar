using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Application.Interfaces.Pieces;
using Microsoft.Extensions.Caching.Memory;

namespace ChessWar.Infrastructure.Services;

public class PieceConfigService : IPieceConfigService
{
  private readonly IBalanceVersionRepository _repository;
  private readonly IMemoryCache _cache;

  private const string ActiveVersionCacheKey = "piece-config:active";

  public PieceConfigService(IBalanceVersionRepository repository, IMemoryCache cache)
  {
    _repository = repository;
    _cache = cache;
  }

  public async Task<BalanceVersion?> GetActiveAsync(CancellationToken cancellationToken = default)
  {
    if (_cache.TryGetValue(ActiveVersionCacheKey, out BalanceVersion? cached))
    {
      return cached;
    }

    var active = await _repository.GetActiveAsync(cancellationToken);

    if (active != null)
    {
      _cache.Set(ActiveVersionCacheKey, active, TimeSpan.FromMinutes(5));
    }

    return active;
  }
}


