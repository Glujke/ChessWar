namespace ChessWar.Domain.Entities.Config;

/// <summary>
/// Конфигурация системы "Коллективный Щит"
/// </summary>
public sealed class ShieldSystemConfig
{
    /// <summary>
    /// Конфигурация щита короля
    /// </summary>
    public KingShieldConfig King { get; init; } = new();

    /// <summary>
    /// Конфигурация щита обычных фигур
    /// </summary>
    public AllyShieldConfig Ally { get; init; } = new();
}

/// <summary>
/// Конфигурация щита короля
/// </summary>
public sealed class KingShieldConfig
{
    /// <summary>
    /// Базовая регенерация щита короля за ход (без союзников)
    /// </summary>
    public int BaseRegen { get; init; }

    /// <summary>
    /// Бонусы от союзников в радиусе ≤1 клетки
    /// Ключ: тип фигуры (King, Queen, Rook, Bishop, Knight, Pawn)
    /// Значение: бонус к регенерации
    /// </summary>
    public Dictionary<string, int> ProximityBonus1 { get; init; } = new();

    /// <summary>
    /// Бонусы от союзников в радиусе =2 клетки (специальный бонус)
    /// Ключ: тип фигуры (King, Queen, Rook, Bishop, Knight, Pawn)
    /// Значение: бонус к регенерации
    /// </summary>
    public Dictionary<string, int> ProximityBonus2 { get; init; } = new();
}

/// <summary>
/// Конфигурация щита обычных фигур (от соседей)
/// </summary>
public sealed class AllyShieldConfig
{
    /// <summary>
    /// Вклад каждого типа соседа в щит фигуры (радиус ≤1 клетка)
    /// Ключ: тип фигуры (King, Queen, Rook, Bishop, Knight, Pawn)
    /// Значение: вклад в Shield HP
    /// </summary>
    public Dictionary<string, int> NeighborContribution { get; init; } = new();
}

