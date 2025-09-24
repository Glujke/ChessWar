using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Domain.Enums;

namespace ChessWar.Application.Services.Tutorial;

/// <summary>
/// Сервис для управления подсказками обучения
/// </summary>
public class TutorialHintService : ITutorialHintService
{
    private readonly TutorialHintHelper _hintHelper;

    public TutorialHintService()
    {
        _hintHelper = new TutorialHintHelper();
    }

    public async Task<List<string>> GetHintsForStageAsync(TutorialStage stage, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_hintHelper.GetHintsForStage(stage));
    }

    public async Task<List<string>> GetContextualHintsAsync(Guid sessionId, string gameState, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new List<string>
        {
            "Следуйте подсказкам на экране",
            "Изучайте интерфейс игры"
        });
    }
}
