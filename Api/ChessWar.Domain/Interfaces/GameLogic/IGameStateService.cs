using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Interfaces.GameLogic;

public interface IGameStateService
{
    GameResult? CheckVictory(GameSession session);
}


