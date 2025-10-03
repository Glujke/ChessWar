using ChessWar.Domain.Entities;

namespace ChessWar.Domain.ValueObjects;

/// <summary>
/// Ход в игре
/// </summary>
public class Turn
{
    public int Number { get; private set; }
    public Participant ActiveParticipant { get; private set; }
    public Piece? SelectedPiece { get; private set; }
    public List<TurnAction> Actions { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int RemainingMP { get; private set; }

    public Turn(int number, Participant activeParticipant)
    {
        if (number <= 0)
            throw new ArgumentException("Turn number must be positive", nameof(number));

        Number = number;
        ActiveParticipant = activeParticipant ?? throw new ArgumentNullException(nameof(activeParticipant));
        SelectedPiece = null;
        Actions = new List<TurnAction>();
        CreatedAt = DateTime.UtcNow;
        RemainingMP = activeParticipant.MP;
    }

    /// <summary>
    /// Выбрать фигуру для хода
    /// </summary>
    public void SelectPiece(Piece piece)
    {
        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        if (piece.Owner?.Id != ActiveParticipant.Id)
            throw new InvalidOperationException("Cannot select piece that doesn't belong to active participant");

        SelectedPiece = piece;
    }

    /// <summary>
    /// Добавить действие в ход
    /// </summary>
    public void AddAction(TurnAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        Actions.Add(action);
    }

    /// <summary>
    /// Проверить, выбрана ли фигура
    /// </summary>
    public bool HasSelectedPiece()
    {
        return SelectedPiece != null;
    }

    /// <summary>
    /// Получить действия по типу
    /// </summary>
    public List<TurnAction> GetActionsByType(string actionType)
    {
        return Actions.Where(a => a.ActionType == actionType).ToList();
    }

    /// <summary>
    /// Очистить все действия
    /// </summary>
    public void ClearActions()
    {
        Actions.Clear();
    }

    /// <summary>
    /// Проверить, может ли игрок потратить указанное количество маны
    /// </summary>
    public bool CanAfford(int manaCost)
    {
        return RemainingMP >= manaCost;
    }

    /// <summary>
    /// Потратить ману (только если достаточно)
    /// </summary>
    public bool SpendMP(int manaCost)
    {
        if (!CanAfford(manaCost))
            return false;

        RemainingMP -= manaCost;
        return true;
    }

    /// <summary>
    /// Обновить оставшуюся ману из состояния игрока
    /// </summary>
    public void UpdateRemainingMP()
    {
        RemainingMP = ActiveParticipant.MP;
    }
}
