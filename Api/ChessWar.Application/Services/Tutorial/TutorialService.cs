using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Domain.Interfaces.DataAccess; using ChessWar.Domain.Interfaces.Tutorial; using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Entities;

namespace ChessWar.Application.Services.Tutorial;

/// <summary>
/// Сервис для управления обучением
/// </summary>
public class TutorialService : ITutorialService
{
    private readonly IGameModeRepository _modeRepository;
    private readonly ITutorialHintService _hintService;
    private readonly IGameNotificationService _notificationService;

    public TutorialService(IGameModeRepository modeRepository, ITutorialHintService hintService, IGameNotificationService notificationService)
    {
        _modeRepository = modeRepository;
        _hintService = hintService;
        _notificationService = notificationService;
    }

    public async Task<ITutorialMode> StartTutorialAsync(string playerId, CancellationToken cancellationToken = default)
    {
        var player = new Player(playerId, new List<Piece>());
        var tutorialSession = new TutorialSession(player);

        await _modeRepository.SaveModeAsync(tutorialSession, cancellationToken);

        return new TutorialSessionSnapshot(tutorialSession);
    }

    public async Task<ITutorialMode> AdvanceToNextStageAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _modeRepository.GetModeByIdAsync<ITutorialMode>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Tutorial session {sessionId} not found");
        }

        var previousStage = session.CurrentStage;

        session.AdvanceToNextStage();

        await _notificationService.NotifyTutorialAdvancedAsync(sessionId, session.CurrentStage.ToString(), cancellationToken);

        await _modeRepository.SaveModeAsync(session, cancellationToken);

        return new TutorialSessionSnapshot(session);
    }

    public async Task<int> GetTutorialProgressAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _modeRepository.GetModeByIdAsync<ITutorialMode>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Tutorial session {sessionId} not found");
        }

        return session.Progress;
    }

    public async Task<List<string>> GetCurrentHintsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _modeRepository.GetModeByIdAsync<ITutorialMode>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Tutorial session {sessionId} not found");
        }

        return await _hintService.GetHintsForStageAsync(session.CurrentStage, cancellationToken);
    }

    public async Task<ITutorialMode> CompleteTutorialAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _modeRepository.GetModeByIdAsync<ITutorialMode>(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Tutorial session {sessionId} not found");
        }

        session.AdvanceToNextStage(); 

        await _notificationService.NotifyTutorialAdvancedAsync(sessionId, session.CurrentStage.ToString(), cancellationToken);

        await _modeRepository.SaveModeAsync(session, cancellationToken);

        return new TutorialSessionSnapshot(session);
    }
}

internal sealed class TutorialSessionSnapshot : ITutorialMode
{
    public Guid Id { get; }
    public GameMode Mode { get; }
    public ChessWar.Domain.Enums.GameStatus Status { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }
    public TutorialStage CurrentStage { get; }
    public int Progress { get; }
    public bool ShowHints { get; }
    public bool IsCompleted { get; }

    public TutorialSessionSnapshot(ITutorialMode source)
    {
        Id = source.Id;
        Mode = source.Mode;
        Status = source.Status;
        CreatedAt = source.CreatedAt;
        UpdatedAt = source.UpdatedAt;
        CurrentStage = source.CurrentStage;
        Progress = source.Progress;
        ShowHints = source.ShowHints;
        IsCompleted = source.IsCompleted;
    }

    public void AdvanceToNextStage()
    {
    }

    public void SetActiveScenario(IScenario scenario)
    {
    }
}
