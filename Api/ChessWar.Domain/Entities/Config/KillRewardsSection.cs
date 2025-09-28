using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Entities.Config;

public sealed class KillRewardsSection
{
    public int Pawn { get; init; }
    public int Knight { get; init; }
    public int Bishop { get; init; }
    public int Rook { get; init; }
    public int Queen { get; init; }
    public int King { get; init; }

    public int GetRewardForPieceType(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Pawn => Pawn,
            PieceType.Knight => Knight,
            PieceType.Bishop => Bishop,
            PieceType.Rook => Rook,
            PieceType.Queen => Queen,
            PieceType.King => King,
            _ => 0
        };
    }
}
