namespace ChessWar.Application.DTOs;

/// <summary>
/// Статистика игрока
/// </summary>
public class PlayerStatsDto
{
    /// <summary>
    /// ID игрока
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Имя игрока
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Общее количество игр
    /// </summary>
    public int TotalGames { get; set; }

    /// <summary>
    /// Количество побед
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Количество поражений
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Процент побед
    /// </summary>
    public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;

    /// <summary>
    /// Общее время игры (в минутах)
    /// </summary>
    public int TotalPlayTimeMinutes { get; set; }

    /// <summary>
    /// Рейтинг игрока
    /// </summary>
    public int Rating { get; set; }
}

/// <summary>
/// Результат игры для статистики
/// </summary>
public class GameResultDto
{
    /// <summary>
    /// Режим игры
    /// </summary>
    public string GameMode { get; set; } = string.Empty;

    /// <summary>
    /// Результат игры
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Длительность игры (в минутах)
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Дата завершения
    /// </summary>
    public DateTime CompletedAt { get; set; }
}

