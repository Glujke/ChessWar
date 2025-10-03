namespace ChessWar.Domain.ValueObjects;

/// <summary>
/// Игровое действие для ИИ
/// </summary>
public class GameAction
{
    /// <summary>
    /// Тип действия (Move, Attack, Ability, Evolve)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// ID фигуры, которая выполняет действие
    /// </summary>
    public string PieceId { get; set; } = string.Empty;

    /// <summary>
    /// Целевая позиция для действия
    /// </summary>
    public Position TargetPosition { get; set; } = new Position(0, 0);

    /// <summary>
    /// Название способности (для типа Ability)
    /// </summary>
    public string? AbilityName { get; set; }

    /// <summary>
    /// Дополнительные параметры действия
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    public GameAction()
    {
    }

    public GameAction(string type, string pieceId, Position targetPosition)
    {
        Type = type;
        PieceId = pieceId;
        TargetPosition = targetPosition;
    }
}
