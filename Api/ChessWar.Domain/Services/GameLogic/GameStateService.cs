using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Services.GameLogic;

public class GameStateService : IGameStateService
{
    public GameResult? CheckVictory(GameSession session)
    {
        var p1KingAlive = session.Player1.Pieces.Any(p => p.Type == PieceType.King && p.IsAlive);
        var p2KingAlive = session.Player2.Pieces.Any(p => p.Type == PieceType.King && p.IsAlive);

        if (!p2KingAlive && p1KingAlive) return GameResult.Player1Victory;
        if (!p1KingAlive && p2KingAlive) return GameResult.Player2Victory;
        return null;
    }
}


