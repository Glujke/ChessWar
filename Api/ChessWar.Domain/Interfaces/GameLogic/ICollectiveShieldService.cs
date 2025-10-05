using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Interfaces.GameLogic;

/// <summary>
/// Интерфейс для сервиса управления системой "Коллективный Щит"
/// </summary>
public interface ICollectiveShieldService
{
    /// <summary>
    /// Регенерирует щит короля на основе близости союзников
    /// </summary>
    /// <param name="king">Король</param>
    /// <param name="allyPieces">Список союзных фигур</param>
    /// <returns>Количество восстановленных HP щита</returns>
    int RegenerateKingShield(Piece king, List<Piece> allyPieces);

    /// <summary>
    /// Пересчитывает щит обычной фигуры на основе текущих соседей
    /// </summary>
    /// <param name="ally">Обычная фигура (не King)</param>
    /// <param name="neighbors">Список всех фигур на доске для поиска соседей</param>
    /// <returns>Изменение щита (может быть отрицательным)</returns>
    int RecalculateAllyShield(Piece ally, List<Piece> neighbors);
}
