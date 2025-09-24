using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Interfaces.GameLogic;

public interface IEvolutionService
{
    bool CanEvolve(Piece piece);
    Piece EvolvePiece(Piece piece, PieceType targetType);
    List<PieceType> GetPossibleEvolutions(PieceType currentType);
    bool MeetsEvolutionRequirements(Piece piece, PieceType targetType);
    List<EvolutionRecord> GetEvolutionHistory();
}

public record EvolutionRecord(int PieceId, PieceType NewType, DateTime Timestamp);
