using ChessWar.Application.Interfaces.AI;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.DataAccess;

namespace ChessWar.Application.Services.AI;

/// <summary>
/// Сервис выполнения ходов ИИ
/// </summary>
public class AITurnService : IAITurnService
{
    private readonly IAIService _aiService;
    private readonly IGameSessionRepository _sessionRepository;

    public AITurnService(
        IAIService aiService,
        IGameSessionRepository sessionRepository)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public async Task<bool> MakeAiTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession.Mode != "AI" && gameSession.Mode != "Tutorial" && gameSession.Mode != "Test") return false;
        
        var success = await _aiService.MakeAiTurnAsync(gameSession, cancellationToken);
        
        if (!success)
        {
            gameSession.CompleteGame(Domain.Enums.GameResult.Player1Victory);
        }
        
        await _sessionRepository.SaveAsync(gameSession, cancellationToken);
        return success;
    }
}
