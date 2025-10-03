using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Pieces;

public interface IPieceConfigService
{
    Task<BalanceVersion?> GetActiveAsync(CancellationToken cancellationToken = default);
}


