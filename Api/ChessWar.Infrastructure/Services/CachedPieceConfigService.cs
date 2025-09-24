using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Entities;

namespace ChessWar.Infrastructure.Services;

/// <summary>
/// Кэширующий декоратор для PieceConfigService
/// </summary>
public class CachedPieceConfigService : IPieceConfigService
{
    private readonly PieceConfigService _pieceConfigService;
    private readonly ICacheService _cacheService;
    private readonly string _cacheKey = "piece_config_active";
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

    public CachedPieceConfigService(PieceConfigService pieceConfigService, ICacheService cacheService)
    {
        _pieceConfigService = pieceConfigService ?? throw new ArgumentNullException(nameof(pieceConfigService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<BalanceVersion?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<BalanceVersion>(_cacheKey, cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var active = await _pieceConfigService.GetActiveAsync(cancellationToken);
        if (active != null)
        {
            await _cacheService.SetAsync(_cacheKey, active, _cacheExpiration, cancellationToken);
        }

        return active;
    }
}
