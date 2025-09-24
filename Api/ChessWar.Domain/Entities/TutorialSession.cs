using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Tutorial;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Entities;

/// <summary>
/// Сессия обучения с пошаговым прохождением
/// </summary>
public class TutorialSession : ITutorialMode
{
    public Guid Id { get; private set; }
    public GameMode Mode { get; private set; }
    public Enums.GameStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    public TutorialStage CurrentStage { get; private set; }
    public int Progress { get; private set; }
    public bool ShowHints { get; }
    public bool IsCompleted { get; private set; }
    public ScenarioType CurrentScenario { get; private set; }

    public TutorialSession(Player player, bool showHints = true)
    {
        Id = Guid.NewGuid();
        Mode = GameMode.Tutorial;
        Status = Enums.GameStatus.Waiting;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        ShowHints = showHints;
        CurrentStage = TutorialStage.Introduction;
        Progress = 0;
        IsCompleted = false;
        CurrentScenario = ScenarioType.Battle;
    }

    public void AdvanceToNextStage()
    {
        if (IsCompleted) return;

        CurrentStage = CurrentStage switch
        {
            TutorialStage.Introduction => TutorialStage.Battle1,
            TutorialStage.Battle1 => TutorialStage.Battle2,
            TutorialStage.Battle2 => TutorialStage.Boss,
            TutorialStage.Boss => TutorialStage.Completed,
            _ => CurrentStage
        };

        Progress = CurrentStage switch
        {
            TutorialStage.Introduction => 0,
            TutorialStage.Battle1 => 25,
            TutorialStage.Battle2 => 50,
            TutorialStage.Boss => 75,
            TutorialStage.Completed => 100,
            _ => Progress
        };

        UpdatedAt = DateTime.UtcNow;

        if (CurrentStage == TutorialStage.Completed)
        {
            IsCompleted = true;
            Status = Enums.GameStatus.Finished;
        }
    }

    public void SetActiveScenario(IScenario scenario)
    {
    }
}
