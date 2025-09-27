using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Провайдер уровней сложности ИИ
/// </summary>
public class AIDifficultyProvider : IAIDifficultyLevel
{
    private readonly Dictionary<Guid, AIDifficultyLevel> _playerDifficulties = new();
    
    public AIDifficultyLevel GetDifficultyLevel(Participant participant)
    {
        return _playerDifficulties.GetValueOrDefault(participant.Id, AIDifficultyLevel.Medium);
    }
    
    public double GetTemperature(AIDifficultyLevel level)
    {
        return level switch
        {
            AIDifficultyLevel.Easy => 2.0,    
            AIDifficultyLevel.Medium => 1.0,
            AIDifficultyLevel.Hard => 0.5,   
            _ => 1.0
        };
    }
    
    public int GetPlanningDepth(AIDifficultyLevel level)
    {
        return level switch
        {
            AIDifficultyLevel.Easy => 1,    
            AIDifficultyLevel.Medium => 3,   
            AIDifficultyLevel.Hard => 5,    
            _ => 1
        };
    }
    
    public double GetDiscountFactor(AIDifficultyLevel level)
    {
        return level switch
        {
            AIDifficultyLevel.Easy => 0.7,   
            AIDifficultyLevel.Medium => 0.9, 
            AIDifficultyLevel.Hard => 0.95,  
            _ => 0.9
        };
    }
    
    /// <summary>
    /// Установить уровень сложности для участника
    /// </summary>
    public void SetDifficultyLevel(Participant participant, AIDifficultyLevel level)
    {
        _playerDifficulties[participant.Id] = level;
    }
    
    /// <summary>
    /// Получить все настроенные уровни сложности
    /// </summary>
    public Dictionary<Guid, AIDifficultyLevel> GetAllDifficulties()
    {
        return new Dictionary<Guid, AIDifficultyLevel>(_playerDifficulties);
    }
}
