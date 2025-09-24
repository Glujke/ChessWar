using ChessWar.Domain.Entities.Config;

namespace ChessWar.Domain.Interfaces.Configuration;

public interface IBalanceConfigProvider
{
    BalanceConfig GetActive();
    void Invalidate();
}