namespace ChessWar.Application.Interfaces.GameModes;

/// <summary>
/// Фабрика для выбора стратегии игрового режима
/// </summary>
public interface IGameModeStrategyFactory
{
    /// <summary>
    /// Получает стратегию по режиму игры
    /// </summary>
    /// <param name="mode">Режим игры (Tutorial, AI, Local, Online)</param>
    /// <returns>Стратегия для данного режима</returns>
    IGameModeStrategy GetStrategy(string mode);

    /// <summary>
    /// Получает стратегию по режиму игры с проверкой существования
    /// </summary>
    /// <param name="mode">Режим игры</param>
    /// <returns>Стратегия для данного режима или null, если режим не поддерживается</returns>
    IGameModeStrategy? TryGetStrategy(string mode);

    /// <summary>
    /// Проверяет, поддерживается ли режим игры
    /// </summary>
    /// <param name="mode">Режим игры</param>
    /// <returns>True, если режим поддерживается</returns>
    bool IsModeSupported(string mode);

    /// <summary>
    /// Получает список поддерживаемых режимов
    /// </summary>
    /// <returns>Список поддерживаемых режимов</returns>
    IEnumerable<string> GetSupportedModes();
}

