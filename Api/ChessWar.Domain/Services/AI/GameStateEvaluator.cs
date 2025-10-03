using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Services.AI.Math;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Оценщик состояния игры для ИИ
/// </summary>
public class GameStateEvaluator : IGameStateEvaluator
{
    private readonly Dictionary<PieceType, double> _pieceValues = new()
    {
        { PieceType.Pawn, 1.0 },
        { PieceType.Knight, 3.0 },
        { PieceType.Bishop, 3.0 },
        { PieceType.Rook, 5.0 },
        { PieceType.Queen, 9.0 },
        { PieceType.King, 100.0 }
    };

    public double EvaluateGameState(GameSession session, Participant participant)
    {
        if (participant == null) return 0;

        var materialScore = EvaluateMaterialAdvantage(session, participant);
        var positionScore = EvaluatePositionAdvantage(session, participant);
        var kingSafetyScore = EvaluateKingThreat(session, participant);

        return materialScore + positionScore + kingSafetyScore;
    }

    public double EvaluatePiecePosition(Piece piece, GameSession session)
    {
        var baseValue = _pieceValues.GetValueOrDefault(piece.Type, 1.0);
        var positionBonus = GetPositionBonus(piece);
        var mobilityBonus = GetMobilityBonus(piece, session);

        return baseValue + positionBonus + mobilityBonus;
    }

    public double EvaluateKingThreat(GameSession session, Participant participant)
    {
        if (participant == null) return 0;

        var king = participant.Pieces.FirstOrDefault(p => p.Type == PieceType.King && p.IsAlive);
        if (king == null) return -1000;

        var threatLevel = CalculateThreatLevel(king, session);
        return -threatLevel * 10;
    }

    public double EvaluateMaterialAdvantage(GameSession session, Participant participant)
    {
        if (participant == null) return 0;

        var playerValue = participant.Pieces
            .Where(p => p.IsAlive)
            .Sum(p => _pieceValues.GetValueOrDefault(p.Type, 1.0));

        var enemyValue = session.GetAllPieces()
            .Where(p => p.Owner?.Id != participant.Id && p.IsAlive)
            .Sum(p => _pieceValues.GetValueOrDefault(p.Type, 1.0));

        return playerValue - enemyValue;
    }

    private double EvaluatePositionAdvantage(GameSession session, Participant participant)
    {
        if (participant == null) return 0;

        var score = 0.0;

        foreach (var piece in participant.Pieces.Where(p => p.IsAlive))
        {
            score += GetPositionBonus(piece);
            score += GetMobilityBonus(piece, session);
        }

        return score;
    }

    private double GetPositionBonus(Piece piece)
    {
        var centerDistance = ProbabilityMath.ChebyshevDistance(piece.Position.X, piece.Position.Y, 3.5, 3.5);
        var centerBonus = System.Math.Max(0, 4 - centerDistance) * 0.1;

        var pawnAdvanceBonus = 0.0;
        if (piece.Type == PieceType.Pawn)
        {
            pawnAdvanceBonus = piece.Position.Y * 0.2;
        }

        return centerBonus + pawnAdvanceBonus;
    }

    private double GetMobilityBonus(Piece piece, GameSession session)
    {
        var mobility = 0.0;

        switch (piece.Type)
        {
            case PieceType.Pawn:
                mobility = 2;
                break;
            case PieceType.Knight:
                mobility = 8;
                break;
            case PieceType.Bishop:
                mobility = 14;
                break;
            case PieceType.Rook:
                mobility = 14;
                break;
            case PieceType.Queen:
                mobility = 28;
                break;
            case PieceType.King:
                mobility = 8;
                break;
        }

        return mobility * 0.05;
    }

    private double CalculateThreatLevel(Piece king, GameSession session)
    {
        var threatLevel = 0.0;
        var enemies = session.GetAllPieces()
            .Where(p => p.Owner?.Id != king.Owner?.Id && p.IsAlive)
            .ToList();

        foreach (var enemy in enemies)
        {
            var distance = ProbabilityMath.ChebyshevDistance(
                king.Position.X, king.Position.Y,
                enemy.Position.X, enemy.Position.Y
            );

            if (distance <= 2)
            {
                threatLevel += _pieceValues.GetValueOrDefault(enemy.Type, 1.0) / (distance + 1);
            }
        }

        return threatLevel;
    }
}
