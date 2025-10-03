using ChessWar.Domain.Enums;
using System.Text.Json.Serialization;

namespace ChessWar.Application.DTOs;

/// <summary>
/// Запрос на создание сессии обучения
/// </summary>
public class CreateTutorialSessionDto
{
    /// <summary>
    /// ID игрока
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;
}

/// <summary>
/// Запрос на создание сетевой сессии
/// </summary>
public class CreateOnlineSessionDto
{
    /// <summary>
    /// ID игрока-создателя
    /// </summary>
    public string HostPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Максимальное количество игроков
    /// </summary>
    public int MaxPlayers { get; set; } = 2;
}

/// <summary>
/// Запрос на создание локальной сессии
/// </summary>
public class CreateLocalSessionDto
{
    /// <summary>
    /// Имя первого игрока
    /// </summary>
    public string Player1Name { get; set; } = string.Empty;

    /// <summary>
    /// Имя второго игрока
    /// </summary>
    public string Player2Name { get; set; } = string.Empty;
}

/// <summary>
/// Запрос на создание сессии с ИИ
/// </summary>
public class CreateAiSessionDto
{
    /// <summary>
    /// ID игрока
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Сложность ИИ
    /// </summary>
    public AiDifficulty Difficulty { get; set; } = AiDifficulty.Medium;
}

/// <summary>
/// Ответ с информацией о сессии обучения
/// </summary>
public class TutorialSessionDto
{
    [JsonIgnore]
    public Guid Id { get; set; }
    [JsonPropertyName("sessionId")]
    public Guid SessionId => Id;
    public string Mode { get; set; } = "Tutorial";
    public string Status { get; set; } = string.Empty;
    public ScenarioType CurrentScenario { get; set; }
    public TutorialStage CurrentStage { get; set; }
    [JsonPropertyName("stage")]
    public string Stage => CurrentStage.ToString();
    public int Progress { get; set; }
    public bool IsCompleted { get; set; }
    public bool ShowHints { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string SignalRUrl { get; set; } = string.Empty;

    public TutorialScenarioDto Scenario { get; set; } = new();
    public TutorialBoardDto Board { get; set; } = new();
    public List<PieceDto> Pieces { get; set; } = new();
}

public class TutorialScenarioDto
{
    public string Type { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
}

public class TutorialBoardDto
{
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Ответ с информацией о сетевой сессии
/// </summary>
public class OnlineSessionDto
{
    public Guid Id { get; set; }
    public string Mode { get; set; } = "Online";
    public string Status { get; set; } = string.Empty;
    public string HostPlayerId { get; set; } = string.Empty;
    public string? ConnectedPlayerId { get; set; }
    public int MaxPlayers { get; set; }
    public bool IsReadyToStart { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Ответ с информацией о сессии с ИИ
/// </summary>
public class AiSessionDto
{
    public Guid Id { get; set; }
    public string Mode { get; set; } = "Ai";
    public string Status { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string PlayerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

