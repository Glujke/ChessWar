namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Композитный интерфейс для управления ходами (объединяет все аспекты)
/// </summary>
public interface ITurnService : ITurnManager, ITurnActionExecutor, ITurnActionQuery
{
}