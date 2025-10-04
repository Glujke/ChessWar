namespace ChessWar.Domain.Entities.Config;

/// <summary>
/// Базовые характеристики фигуры из конфигурации баланса
/// </summary>
public sealed class PieceStats
{
    public int Hp { get; init; }
    public int Atk { get; init; }
    public int Range { get; init; }
    public int Movement { get; init; }
    public int XpToEvolve { get; init; }
    
    /// <summary>
    /// Максимальное значение энергетического щита для данного типа фигуры
    /// King=400, Queen=150, Rook=100, Bishop/Knight=80, Pawn=50
    /// </summary>
    public int MaxShieldHP { get; init; }
}



