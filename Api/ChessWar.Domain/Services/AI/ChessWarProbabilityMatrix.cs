using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Services.AI.Math;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Матрица вероятностей для Chess War на основе марковских цепей
/// </summary>
public class ChessWarProbabilityMatrix : IProbabilityMatrix
{
    private readonly Dictionary<string, double> _transitionProbabilities = new();
    private readonly Dictionary<string, double> _rewardValues = new();
    private readonly Dictionary<string, double> _policyValues = new();
    private readonly IGameStateEvaluator _evaluator;
    
    public ChessWarProbabilityMatrix(IGameStateEvaluator evaluator)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }
    
    public double GetTransitionProbability(GameSession from, GameAction action, GameSession to)
    {
        var key = CreateTransitionKey(from, action, to);
        
        if (_transitionProbabilities.TryGetValue(key, out var cached))
        {
            return cached;
        }
        
        var probability = CalculateTransitionProbability(from, action, to);
        _transitionProbabilities[key] = probability;
        
        return probability;
    }
    
    public double GetReward(GameSession session, GameAction action)
    {
        var key = CreateRewardKey(session, action);
        
        if (_rewardValues.TryGetValue(key, out var cached))
        {
            return cached;
        }
        
        var reward = CalculateReward(session, action);
        _rewardValues[key] = reward;
        
        return reward;
    }
    
    public void UpdatePolicy(GameSession session, GameAction action, double probability)
    {
        var key = CreatePolicyKey(session, action);
        _policyValues[key] = ProbabilityMath.Clamp(probability, 0.0, 1.0);
    }
    
    public double GetActionProbability(GameSession session, GameAction action)
    {
        var key = CreatePolicyKey(session, action);
        return _policyValues.GetValueOrDefault(key, 0.0);
    }
    
    private double CalculateTransitionProbability(GameSession from, GameAction action, GameSession to)
    {
        var baseProbability = GetBaseActionProbability(action);
        
        var positionModifier = GetPositionModifier(from, action);
        var defenseModifier = GetDefenseModifier(from, action);
        var pieceTypeModifier = GetPieceTypeModifier(action);
        
        var finalProbability = baseProbability * positionModifier * defenseModifier * pieceTypeModifier;
        
        return ProbabilityMath.Clamp(finalProbability, 0.0, 1.0);
    }
    
    private double CalculateReward(GameSession session, GameAction action)
    {
        var killReward = CalculateKillReward(session, action);
        var defenseReward = CalculateDefenseReward(session, action);
        var positionReward = CalculatePositionReward(session, action);
        var experienceReward = CalculateExperienceReward(session, action);
        
        return killReward + defenseReward + positionReward + experienceReward;
    }
    
    public double GetBaseActionProbability(GameAction action)
    {
        return action.Type switch
        {
            "Move" => 0.9,     
            "Attack" => 0.7,   
            "Ability" => 0.8,  
            "Evolve" => 1.0,    
            _ => 0.5
        };
    }
    
    private double GetPositionModifier(GameSession session, GameAction action)
    {
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null) return 0.0;
        
        var centerDistance = ProbabilityMath.ChebyshevDistance(piece.Position.X, piece.Position.Y, 3.5, 3.5);
        var centerBonus = System.Math.Max(0, 4 - centerDistance) * 0.1 + 0.8; 
        
        return ProbabilityMath.Clamp(centerBonus, 0.1, 1.5);
    }
    
    private double GetDefenseModifier(GameSession session, GameAction action)
    {
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null) return 0.0;
        
        var enemies = session.GetAllPieces()
            .Where(p => p.Owner?.Id != piece.Owner?.Id && p.IsAlive)
            .ToList();
        
        var threatLevel = 0.0;
        foreach (var enemy in enemies)
        {
            var distance = ProbabilityMath.ChebyshevDistance(
                piece.Position.X, piece.Position.Y,
                enemy.Position.X, enemy.Position.Y
            );
            
            if (distance <= 2)
            {
                threatLevel += 0.2;
            }
        }
        
        return ProbabilityMath.Clamp(1.0 - threatLevel, 0.3, 1.0);
    }
    
    private double GetPieceTypeModifier(GameAction action)
    {
        return action.Type switch
        {
            "Move" => 1.0,    
            "Attack" => 1.0,   
            "Ability" => 0.9,  
            "Evolve" => 1.0,   
            _ => 1.0
        };
    }
    
    private double CalculateKillReward(GameSession session, GameAction action)
    {
        if (action.Type != "Attack") return 0.0;
        
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null) return 0.0;
        
        var target = session.GetPieceAtPosition(action.TargetPosition);
        if (target == null || target.Owner?.Id == piece.Owner?.Id) return 0.0;
        
        var targetValue = target.Type switch
        {
            Enums.PieceType.Pawn => 1.0,
            Enums.PieceType.Knight => 3.0,
            Enums.PieceType.Bishop => 3.0,
            Enums.PieceType.Rook => 5.0,
            Enums.PieceType.Queen => 9.0,
            Enums.PieceType.King => 100.0,
            _ => 1.0
        };
        
        return targetValue * 10; 
    }
    
    private double CalculateDefenseReward(GameSession session, GameAction action)
    {
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null) return 0.0;
        
        var owner = piece.Owner;
        if (owner == null) return 0.0;
        
        var king = owner.Pieces.FirstOrDefault(p => p.Type == Enums.PieceType.King && p.IsAlive);
        if (king == null) return 0.0;
        
        var kingDistance = ProbabilityMath.ChebyshevDistance(
            piece.Position.X, piece.Position.Y,
            king.Position.X, king.Position.Y
        );
        
        if (kingDistance <= 2)
        {
            return 5.0 * (3 - kingDistance); 
        }
        
        return 0.0;
    }
    
    private double CalculatePositionReward(GameSession session, GameAction action)
    {
        if (action.Type != "Move") return 0.0;
        
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null) return 0.0;
        
        var currentPosition = piece.Position;
        var newPosition = action.TargetPosition;
        
        var currentCenterDistance = ProbabilityMath.ChebyshevDistance(currentPosition.X, currentPosition.Y, 3.5, 3.5);
        var newCenterDistance = ProbabilityMath.ChebyshevDistance(newPosition.X, newPosition.Y, 3.5, 3.5);
        
        if (newCenterDistance < currentCenterDistance)
        {
            return 2.0; 
        }
        
        return 0.0;
    }
    
    private double CalculateExperienceReward(GameSession session, GameAction action)
    {
        if (action.Type != "Attack") return 0.0;
        
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null) return 0.0;
        
        var target = session.GetPieceAtPosition(action.TargetPosition);
        if (target == null || target.Owner?.Id == piece.Owner?.Id) return 0.0;
        
        var experienceBonus = piece.XP / 100.0; 
        return experienceBonus * 3.0;
    }
    
    private string CreateTransitionKey(GameSession from, GameAction action, GameSession to)
    {
        return $"T_{from.Id}_{action.Type}_{action.PieceId}_{to.Id}";
    }
    
    private string CreateRewardKey(GameSession session, GameAction action)
    {
        return $"R_{session.Id}_{action.Type}_{action.PieceId}";
    }
    
    private string CreatePolicyKey(GameSession session, GameAction action)
    {
        return $"P_{session.Id}_{action.Type}_{action.PieceId}";
    }
}
