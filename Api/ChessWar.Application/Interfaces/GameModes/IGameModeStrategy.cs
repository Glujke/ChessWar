using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Application.Interfaces.GameModes;

/// <summary>
/// Абстракция для игровых режимов (Strategy pattern)
/// </summary>
public interface IGameModeStrategy
{
    /// <summary>
    /// Создает игровую сессию для данного режима
    /// </summary>
    Task<GameSessionDto> CreateSessionAsync(CreateGameSessionDto dto);

    /// <summary>
    /// Выполняет действие в рамках игровой сессии
    /// </summary>
    Task<GameSessionDto> ExecuteActionAsync(GameSession session, ExecuteActionDto dto);

    /// <summary>
    /// Завершает ход
    /// </summary>
    Task<GameSessionDto> EndTurnAsync(GameSession session);

    /// <summary>
    /// Выполняет ход ИИ (если применимо)
    /// </summary>
    Task<GameSessionDto> MakeAiTurnAsync(GameSession session);

    /// <summary>
    /// Перемещает фигуру
    /// </summary>
    Task<GameSessionDto> MovePieceAsync(GameSession session, string pieceId, PositionDto targetPosition);

    /// <summary>
    /// Получает доступные действия для фигуры
    /// </summary>
    Task<List<PositionDto>> GetAvailableActionsAsync(GameSession session, string pieceId, string actionType, string? abilityName = null);

    /// <summary>
    /// Выполняет способность
    /// </summary>
    Task<GameSessionDto> ExecuteAbilityAsync(GameSession session, AbilityRequestDto request);

    /// <summary>
    /// Выполняет эволюцию
    /// </summary>
    Task<GameSessionDto> ExecuteEvolutionAsync(GameSession session, string pieceId, string targetType);

    /// <summary>
    /// Переход между этапами обучения
    /// </summary>
    Task<GameSessionDto> TutorialTransitionAsync(GameSession session, TutorialTransitionRequestDto request);

    /// <summary>
    /// Получает информацию о сессии
    /// </summary>
    Task<GameSessionDto> GetSessionAsync(Guid sessionId);

    /// <summary>
    /// Запускает игру
    /// </summary>
    Task<GameSessionDto> StartGameAsync(GameSession session);

    /// <summary>
    /// Завершает игру
    /// </summary>
    Task<GameSessionDto> CompleteGameAsync(GameSession session, GameResult result);
}
