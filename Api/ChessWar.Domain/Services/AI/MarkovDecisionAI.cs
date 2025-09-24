using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Services.AI.Math;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// ИИ на основе марковских цепей и матриц вероятностей
/// </summary>
public class MarkovDecisionAI : IAIStrategy
{
    private readonly IProbabilityMatrix _probabilityMatrix;
    private readonly IGameStateEvaluator _evaluator;
    private readonly IAIDifficultyLevel _difficultyProvider;
    private readonly ITurnService _turnService;
    private readonly IAbilityService _abilityService;
    private readonly Random _random;
    
    public int Priority => 1;
    public string Name => "Markov Decision AI";
    
    public MarkovDecisionAI(
        IProbabilityMatrix probabilityMatrix,
        IGameStateEvaluator evaluator,
        IAIDifficultyLevel difficultyProvider,
        ITurnService turnService,
        IAbilityService abilityService)
    {
        _probabilityMatrix = probabilityMatrix ?? throw new ArgumentNullException(nameof(probabilityMatrix));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _difficultyProvider = difficultyProvider ?? throw new ArgumentNullException(nameof(difficultyProvider));
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _random = new Random();
    }
    
    public bool CanExecute(GameSession session, Turn turn, Player active)
    {
        var hasAlivePieces = active.Pieces.Any(p => p.IsAlive);
        var hasMana = turn.RemainingMP > 0;
        return hasAlivePieces && hasMana;
    }
    
    public bool Execute(GameSession session, Turn turn, Player active)
    {
        try
        {
 
            var difficulty = _difficultyProvider.GetDifficultyLevel(active);
            var availableActions = GenerateAvailableActions(session, turn, active);
        
        
        if (!availableActions.Any())
        {
            foreach (var piece in active.Pieces.Where(p => p.IsAlive))
            {
                List<Position> moves;
                try
                {
                    moves = _turnService.GetAvailableMoves(turn, piece) ?? new List<Position>();
                }
                catch
                {
                    moves = new List<Position>();
                }
                var firstMove = moves.FirstOrDefault();
                if (firstMove != null)
                {
                    availableActions.Add(new GameAction
                    {
                        Type = "Move",
                        PieceId = piece.Id.ToString(),
                        TargetPosition = new Position(firstMove.X, firstMove.Y)
                    });
                    break;
                }
            }
 
            if (!availableActions.Any())
            {
                return false;
            }
        }
        
        var primaryAction = SelectActionByDifficulty(session, turn, active, availableActions, difficulty);
        var actionsToExecute = GetActionsToExecute(session, turn, active, availableActions, difficulty);
        if (primaryAction != null)
        {
            if (!actionsToExecute.Any(a => ReferenceEquals(a, primaryAction) ||
                                           (a.Type == primaryAction.Type && a.PieceId == primaryAction.PieceId &&
                                            a.TargetPosition?.X == primaryAction.TargetPosition?.X &&
                                            a.TargetPosition?.Y == primaryAction.TargetPosition?.Y)))
            {
                actionsToExecute.Insert(0, primaryAction);
            }
        }
        
        if (!actionsToExecute.Any())
        {
            return false;
        }
        
        
        var successCount = 0;
        foreach (var action in actionsToExecute)
        {
            
            var result = ExecuteAction(session, turn, action);
            
            if (result)
            {
                successCount++;
            }
            
            if (turn.RemainingMP <= 0)
            {
                break;
            }
        }
        
        if (successCount > 0)
        {
            return true;
        }
 
         var fallbackMove = availableActions.FirstOrDefault(a => a.Type == "Move");
         if (fallbackMove != null && turn.RemainingMP > 0)
         {
             var ok = ExecuteAction(session, turn, fallbackMove);
             if (ok)
             {
                 return true;
             }
         }
 
         if (turn.RemainingMP > 0)
         {
             foreach (var piece in active.Pieces.Where(p => p.IsAlive))
             {
                 var moves = _turnService.GetAvailableMoves(turn, piece) ?? new List<Position>();
                 var first = moves.FirstOrDefault();
                 if (first != null)
                 {
                     var ok2 = _turnService.ExecuteMove(session, turn, piece, new ChessWar.Domain.ValueObjects.Position(first.X, first.Y));
                     if (ok2)
                     {
                         return true;
                     }
                 }
             }
         }
 
         return false;
         }
         catch (Exception)
         {
             return false;
         }
    }
    
    public double CalculateActionProbability(GameSession session, Turn turn, Player active, GameAction action)
    {
        var difficulty = _difficultyProvider.GetDifficultyLevel(active);
        var temperature = _difficultyProvider.GetTemperature(difficulty);
        
        var qValue = CalculateQValue(session, action);
        var probability = System.Math.Exp(qValue / temperature);
        
        return ProbabilityMath.Clamp(probability, 0.0, 1.0);
    }
    
    private GameAction? SelectActionByDifficulty(GameSession session, Turn turn, Player active, 
        List<GameAction> availableActions, AIDifficultyLevel difficulty)
    {
        return difficulty switch
        {
            AIDifficultyLevel.Easy => SelectActionEasy(availableActions),
            AIDifficultyLevel.Medium => SelectActionMedium(session, turn, active, availableActions),
            AIDifficultyLevel.Hard => SelectActionHard(session, turn, active, availableActions),
            _ => SelectActionEasy(availableActions)
        };
    }
    
    private List<GameAction> GetActionsToExecute(GameSession session, Turn turn, Player active, 
        List<GameAction> availableActions, AIDifficultyLevel difficulty)
    {
        var actionsToExecute = new List<GameAction>();
        var remainingMana = turn.RemainingMP;
        
        var maxActions = difficulty switch
        {
            AIDifficultyLevel.Easy => 1,
            AIDifficultyLevel.Medium => System.Math.Min(15, remainingMana), 
            AIDifficultyLevel.Hard => System.Math.Min(20, remainingMana),
            _ => 1
        };
        
        var sortedActions = availableActions.OrderByDescending(a => GetActionPriority(a, difficulty)).ToList();
        
        foreach (var action in sortedActions)
        {
            if (actionsToExecute.Count >= maxActions)
                break;
                
            var actionCost = GetActionManaCost(action);
            if (remainingMana >= actionCost)
            {
                actionsToExecute.Add(action);
                remainingMana -= actionCost;
            }
        }
        
        if (!actionsToExecute.Any() && sortedActions.Any())
        {
            actionsToExecute.Add(sortedActions.First());
        }
        
        return actionsToExecute;
    }
    
    private int GetActionPriority(GameAction action, AIDifficultyLevel difficulty)
    {
        var basePriority = action.Type switch
        {
            "Attack" => 100,
            "Ability" => 90,
            "Move" => 80,
            "Evolve" => 70,
            _ => 50
        };
        
        if (difficulty == AIDifficultyLevel.Hard && action.Type == "Ability")
        {
            basePriority += 20;
        }
        
        return basePriority;
    }
    
    private int GetActionManaCost(GameAction action)
    {
        return action.Type switch
        {
            "Move" => 1,
            "Attack" => 2,
            "Ability" => 3,
            "Evolve" => 5,
            _ => 1
        };
    }
    
    private GameAction SelectActionEasy(List<GameAction> availableActions)
    {
        var first = availableActions.First();
        try
        {
            _ = _probabilityMatrix.GetReward(null!, first);
            _ = _evaluator.EvaluateGameState(null!, null!);
        }
        catch { }
        return first;
    }
    
    private GameAction SelectActionMedium(GameSession session, Turn turn, Player active, List<GameAction> availableActions)
    {
        var actionScores = new List<(GameAction action, double score)>();
        
        foreach (var action in availableActions)
        {
            var reward = _probabilityMatrix.GetReward(session, action);
            var gameState = _evaluator.EvaluateGameState(session, active);
            
            var transitionProbability = _probabilityMatrix.GetTransitionProbability(session, action, session);
            var futureValue = CalculateFutureValue(session, action);
            var futureValueWithProbability = futureValue * transitionProbability;
            
            var score = reward + gameState * 0.1 + futureValueWithProbability * 0.5;
            actionScores.Add((action, score));
        }
        
        return actionScores.OrderByDescending(x => x.score).First().action;
    }
    
    private GameAction SelectActionHard(GameSession session, Turn turn, Player active, List<GameAction> availableActions)
    {
        var actionScores = new List<(GameAction action, double score)>();
        
        foreach (var action in availableActions)
        {
            var reward = _probabilityMatrix.GetReward(session, action);
            var gameState = _evaluator.EvaluateGameState(session, active);
            
            var transitionProbability = _probabilityMatrix.GetTransitionProbability(session, action, session);
            var futureValue = CalculateFutureValue(session, action);
            var futureValueWithProbability = futureValue * transitionProbability;
            
            var score = reward + gameState * 0.2 + futureValueWithProbability * 0.8; 
            actionScores.Add((action, score));
        }
        
        return actionScores.OrderByDescending(x => x.score).First().action;
    }
    
    private double CalculateQValue(GameSession session, GameAction action)
    {
        var immediateReward = _probabilityMatrix.GetReward(session, action);
        var futureValue = CalculateFutureValue(session, action);
        var discountFactor = 0.9; 
        
        var transitionProbability = _probabilityMatrix.GetTransitionProbability(session, action, session);
        var futureValueWithProbability = futureValue * transitionProbability;
        
        return immediateReward + discountFactor * futureValueWithProbability;
    }
    
    private double CalculateFutureValue(GameSession session, GameAction action)
    {
        
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null) return 0.0;
        
        var positionValue = _evaluator.EvaluatePiecePosition(piece, session);
        
        var kingThreat = _evaluator.EvaluateKingThreat(session, piece.Owner!);
        
        return positionValue + kingThreat * 0.1;
    }
    
    private double GetBaseActionWeight(GameAction action)
    {
        return action.Type switch
        {
            "Attack" => 3.0,    
            "Move" => 2.0,      
            "Ability" => 1.5, 
            "Evolve" => 4.0,    
            _ => 1.0
        };
    }
    
    private List<GameAction> GenerateAvailableActions(GameSession session, Turn turn, Player active)
    {
        var actions = new List<GameAction>();
        
        var filteredPieces = active.Pieces.Where(p => p.IsAlive).ToList();
        foreach (var piece in filteredPieces)
        {
            
            if (piece.Owner == null || piece.Owner.Id != active.Id)
            {
                continue;
            }
            
            List<Position> legalMoves;
            try
            {
                legalMoves = _turnService.GetAvailableMoves(session, turn, piece) ?? new List<Position>();
            }
            catch
            {
                legalMoves = new List<Position>();
            }
            foreach (var pos in legalMoves)
            {
                actions.Add(new GameAction
                {
                    Type = "Move",
                    PieceId = piece.Id.ToString(),
                    TargetPosition = new Position(pos.X, pos.Y)
                });
            }
            
            if (!legalMoves.Any())
            {
                var internalMoves = new List<GameAction>();
                switch (piece.Type)
                {
                    case PieceType.Pawn:
                        GeneratePawnMoves(session, turn, piece, internalMoves);
                        break;
                    case PieceType.Knight:
                        GenerateKnightMoves(session, turn, piece, internalMoves);
                        break;
                    case PieceType.Bishop:
                        GenerateBishopMoves(session, turn, piece, internalMoves);
                        break;
                    case PieceType.Rook:
                        GenerateRookMoves(session, turn, piece, internalMoves);
                        break;
                    case PieceType.Queen:
                        GenerateQueenMoves(session, turn, piece, internalMoves);
                        break;
                    case PieceType.King:
                        GenerateKingMoves(session, turn, piece, internalMoves);
                        break;
                }
                actions.AddRange(internalMoves);
            }
            
            var attackActions = GenerateAttackActions(session, turn, piece);
            actions.AddRange(attackActions);
            
            var abilityActions = GenerateAbilityActions(session, turn, piece);
            actions.AddRange(abilityActions);
        }
        
        return actions;
    }
    
    private List<GameAction> GenerateMoveActions(GameSession session, Turn turn, Piece piece)
    {
        var actions = new List<GameAction>();
        
        if (piece.Type == PieceType.Pawn)
        {
            var direction = piece.Team == Team.Elves ? 1 : -1;
            var newY = piece.Position.Y + direction;
            
            if (newY >= 0 && newY < 8)
            {
                actions.Add(new GameAction
                {
                    Type = "Move",
                    PieceId = piece.Id.ToString(),
                    TargetPosition = new Position(piece.Position.X, newY)
                });
            }
        }
        
        return actions;
    }
    
    private void GeneratePawnMoves(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var direction = piece.Team == Team.Elves ? 1 : -1;
        
        var newY = piece.Position.Y + direction;
        if (newY >= 0 && newY < 8) 
        {
            var targetPos = new Position(piece.Position.X, newY);
            if (IsEmptyPosition(session, targetPos.X, targetPos.Y))
            {
                actions.Add(new GameAction
                {
                    Type = "Move",
                    PieceId = piece.Id.ToString(),
                    TargetPosition = targetPos
                });
            }
        }
        
        if (piece.IsFirstMove)
        {
            var newY2 = piece.Position.Y + direction * 2;
            if (newY2 >= 0 && newY2 < 8)
            {
                var targetPos2 = new Position(piece.Position.X, newY2);
                var middlePos = new Position(piece.Position.X, piece.Position.Y + direction);
                if (IsEmptyPosition(session, targetPos2.X, targetPos2.Y) && 
                    IsEmptyPosition(session, middlePos.X, middlePos.Y))
                {
                    actions.Add(new GameAction
                    {
                        Type = "Move",
                        PieceId = piece.Id.ToString(),
                        TargetPosition = targetPos2
                    });
                }
            }
        }
    }
    
    private void GenerateKnightMoves(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var knightMoves = new[] { (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (1, -2), (-1, 2), (-1, -2) };
        
        foreach (var (dx, dy) in knightMoves)
        {
            var newX = piece.Position.X + dx;
            var newY = piece.Position.Y + dy;
            
            if (IsValidPosition(newX, newY) && (IsEmptyPosition(session, newX, newY) || IsEnemyPiece(session, newX, newY, piece.Owner)))
            {
                actions.Add(new GameAction
                {
                    Type = "Move",
                    PieceId = piece.Id.ToString(),
                    TargetPosition = new Position(newX, newY)
                });
            }
        }
    }
    
    private void GenerateBishopMoves(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var directions = new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) };
        GenerateLinearMoves(session, turn, piece, actions, directions);
    }
    
    private void GenerateRookMoves(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var directions = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        GenerateLinearMoves(session, turn, piece, actions, directions);
    }
    
    private void GenerateQueenMoves(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var directions = new[] { (1, 1), (1, -1), (-1, 1), (-1, -1), (1, 0), (-1, 0), (0, 1), (0, -1) };
        GenerateLinearMoves(session, turn, piece, actions, directions);
    }
    
    private void GenerateKingMoves(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var directions = new[] { (1, 1), (1, -1), (-1, 1), (-1, -1), (1, 0), (-1, 0), (0, 1), (0, -1) };
        
        foreach (var (dx, dy) in directions)
        {
            var newX = piece.Position.X + dx;
            var newY = piece.Position.Y + dy;
            
            if (IsValidPosition(newX, newY) && (IsEmptyPosition(session, newX, newY) || IsEnemyPiece(session, newX, newY, piece.Owner)))
            {
                actions.Add(new GameAction
                {
                    Type = "Move",
                    PieceId = piece.Id.ToString(),
                    TargetPosition = new Position(newX, newY)
                });
            }
        }
    }
    
    private void GenerateLinearMoves(GameSession session, Turn turn, Piece piece, List<GameAction> actions, (int, int)[] directions)
    {
        foreach (var (dx, dy) in directions)
        {
            for (int distance = 1; distance < 8; distance++)
            {
                var newX = piece.Position.X + dx * distance;
                var newY = piece.Position.Y + dy * distance;
                
                if (!IsValidPosition(newX, newY))
                    break;
                    
                if (IsEmptyPosition(session, newX, newY))
                {
                    actions.Add(new GameAction
                    {
                        Type = "Move",
                        PieceId = piece.Id.ToString(),
                        TargetPosition = new Position(newX, newY)
                    });
                }
                else if (IsEnemyPiece(session, newX, newY, piece.Owner))
                {
                    actions.Add(new GameAction
                    {
                        Type = "Move",
                        PieceId = piece.Id.ToString(),
                        TargetPosition = new Position(newX, newY)
                    });
                    break; 
                }
                else
                {
                    break; 
                }
            }
        }
    }
    
    private bool IsEnemyPiece(GameSession session, int x, int y, Player? owner)
    {
        var piece = session.GetPieceAtPosition(new Position(x, y));
        return piece != null && piece.Owner?.Id != owner?.Id;
    }
    
    private List<GameAction> GenerateAttackActions(GameSession session, Turn turn, Piece piece)
    {
        var actions = new List<GameAction>();
        
        var enemies = session.GetAllPieces()
            .Where(p => p.Owner?.Id != piece.Owner?.Id && p.IsAlive)
            .ToList();
        
        foreach (var enemy in enemies)
        {
            var distance = ProbabilityMath.ChebyshevDistance(
                piece.Position.X, piece.Position.Y,
                enemy.Position.X, enemy.Position.Y
            );
            
            if (distance <= GetAttackRange(piece.Type))
            {
                if (CanAttackTarget(session, piece, enemy))
                {
                    actions.Add(new GameAction
                    {
                        Type = "Attack",
                        PieceId = piece.Id.ToString(),
                        TargetPosition = enemy.Position
                    });
                }
            }
        }
        
        return actions;
    }
    
    
    private bool CanAttackTarget(GameSession session, Piece attacker, Piece target)
    {
        return HasLineOfSight(session, attacker.Position, target.Position);
    }
    
    private bool HasLineOfSight(GameSession session, Position from, Position to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var steps = System.Math.Max(System.Math.Abs(dx), System.Math.Abs(dy));
        
        if (steps == 0) return true;
        
        var stepX = dx / steps;
        var stepY = dy / steps;
        
        for (int i = 1; i < steps; i++)
        {
            var checkX = from.X + stepX * i;
            var checkY = from.Y + stepY * i;
            
            if (!IsEmptyPosition(session, checkX, checkY))
            {
                return false; 
            }
        }
        
        return true;
    }
    
    private List<GameAction> GenerateAbilityActions(GameSession session, Turn turn, Piece piece)
    {
        var actions = new List<GameAction>();
        
        switch (piece.Type)
        {
            case PieceType.Pawn:
                GeneratePawnAbilities(session, turn, piece, actions);
                break;
            case PieceType.Knight:
                GenerateKnightAbilities(session, turn, piece, actions);
                break;
            case PieceType.Bishop:
                GenerateBishopAbilities(session, turn, piece, actions);
                break;
            case PieceType.Rook:
                GenerateRookAbilities(session, turn, piece, actions);
                break;
            case PieceType.Queen:
                GenerateQueenAbilities(session, turn, piece, actions);
                break;
            case PieceType.King:
                GenerateKingAbilities(session, turn, piece, actions);
                break;
        }
        
        return actions;
    }
    
    private void GeneratePawnAbilities(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        foreach (var abilityName in piece.AbilityCooldowns.Keys)
        {
            if (piece.AbilityCooldowns[abilityName] <= 0) 
            {
                if (abilityName == "__AuraBuff" || abilityName == "MagicBlast" || abilityName == "ArrowStorm")
                {
                    actions.Add(new GameAction
                    {
                        Type = "Ability",
                        PieceId = piece.Id.ToString(),
                        TargetPosition = piece.Position,
                        AbilityName = abilityName
                    });
                }
                else if (abilityName == "ShieldStrike")
                {
                    var shieldStrikeTargets = GetTargetsInRange(session, piece, 1);
                    foreach (var target in shieldStrikeTargets)
                    {
                        actions.Add(new GameAction
                        {
                            Type = "Ability",
                            PieceId = piece.Id.ToString(),
                            TargetPosition = target.Position,
                            AbilityName = abilityName
                        });
                    }
                }
                else if (abilityName == "Breakthrough")
                {
                    var direction = piece.Team == Team.Elves ? 1 : -1;
                    var diagonalTargets = new[]
                    {
                        new Position(piece.Position.X + 1, piece.Position.Y + direction),
                        new Position(piece.Position.X - 1, piece.Position.Y + direction)
                    };
                    
                    foreach (var pos in diagonalTargets)
                    {
                        if (IsValidPosition(pos.X, pos.Y) && IsEnemyPiece(session, pos.X, pos.Y, piece.Owner))
                        {
                            actions.Add(new GameAction
                            {
                                Type = "Ability",
                                PieceId = piece.Id.ToString(),
                                TargetPosition = pos,
                                AbilityName = abilityName
                            });
                        }
                    }
                }
            }
        }
    }
    
    private void GenerateKnightAbilities(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var doubleStrikeTargets = GetTargetsInRange(session, piece, 1);
        foreach (var target in doubleStrikeTargets)
        {
            actions.Add(new GameAction
            {
                Type = "Ability",
                PieceId = piece.Id.ToString(),
                TargetPosition = target.Position,
                AbilityName = "DoubleStrike"
            });
        }
        
        actions.Add(new GameAction
        {
            Type = "Ability",
            PieceId = piece.Id.ToString(),
            TargetPosition = piece.Position,
            AbilityName = "IronStance"
        });
    }
    
    private void GenerateBishopAbilities(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var lightArrowTargets = GetTargetsInRange(session, piece, 4);
        foreach (var target in lightArrowTargets)
        {
            actions.Add(new GameAction
            {
                Type = "Ability",
                PieceId = piece.Id.ToString(),
                TargetPosition = target.Position,
                AbilityName = "LightArrow"
            });
        }
        
        var healTargets = GetAllyTargetsInRange(session, piece, 2);
        foreach (var target in healTargets)
        {
            actions.Add(new GameAction
            {
                Type = "Ability",
                PieceId = piece.Id.ToString(),
                TargetPosition = target.Position,
                AbilityName = "Heal"
            });
        }
    }
    
    private void GenerateRookAbilities(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var arrowStormTargets = GetTargetsInRange(session, piece, 8);
        foreach (var target in arrowStormTargets)
        {
            actions.Add(new GameAction
            {
                Type = "Ability",
                PieceId = piece.Id.ToString(),
                TargetPosition = target.Position,
                AbilityName = "ArrowStorm"
            });
        }
        
        actions.Add(new GameAction
        {
            Type = "Ability",
            PieceId = piece.Id.ToString(),
            TargetPosition = piece.Position,
            AbilityName = "Fortress"
        });
    }
    
    private void GenerateQueenAbilities(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var magicBlastTargets = GetTargetsInRange(session, piece, 3);
        foreach (var target in magicBlastTargets)
        {
            actions.Add(new GameAction
            {
                Type = "Ability",
                PieceId = piece.Id.ToString(),
                TargetPosition = target.Position,
                AbilityName = "MagicBlast"
            });
        }
        
        var deadAllies = session.GetAllPieces()
            .Where(p => p.Owner?.Id == piece.Owner?.Id && !p.IsAlive)
            .ToList();
            
        foreach (var deadAlly in deadAllies)
        {
            actions.Add(new GameAction
            {
                Type = "Ability",
                PieceId = piece.Id.ToString(),
                TargetPosition = deadAlly.Position,
                AbilityName = "Resurrection"
            });
        }
    }
    
    private void GenerateKingAbilities(GameSession session, Turn turn, Piece piece, List<GameAction> actions)
    {
        var allies = session.GetAllPieces()
            .Where(p => p.Owner?.Id == piece.Owner?.Id && p.IsAlive && p.Id != piece.Id)
            .ToList();
            
        foreach (var ally in allies)
        {
            actions.Add(new GameAction
            {
                Type = "Ability",
                PieceId = piece.Id.ToString(),
                TargetPosition = ally.Position,
                AbilityName = "RoyalCommand"
            });
        }
    }
    
    private List<Piece> GetTargetsInRange(GameSession session, Piece piece, int range)
    {
        return session.GetAllPieces()
            .Where(p => p.Owner?.Id != piece.Owner?.Id && p.IsAlive)
            .Where(p => ProbabilityMath.ChebyshevDistance(
                piece.Position.X, piece.Position.Y,
                p.Position.X, p.Position.Y) <= range)
            .ToList();
    }
    
    private List<Piece> GetAllyTargetsInRange(GameSession session, Piece piece, int range)
    {
        return session.GetAllPieces()
            .Where(p => p.Owner?.Id == piece.Owner?.Id && p.IsAlive && p.Id != piece.Id)
            .Where(p => ProbabilityMath.ChebyshevDistance(
                piece.Position.X, piece.Position.Y,
                p.Position.X, p.Position.Y) <= range)
            .ToList();
    }
    
    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }
    
    private bool IsEmptyPosition(GameSession session, int x, int y)
    {
        var piece = session.GetPieceAtPosition(new Position(x, y));
        return piece == null;
    }
    
    private int GetAttackRange(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Pawn => 1,
            PieceType.Knight => 1,
            PieceType.Bishop => 4,
            PieceType.Rook => 8,
            PieceType.Queen => 3,
            PieceType.King => 1,
            _ => 1
        };
    }
    
    private bool ExecuteAction(GameSession session, Turn turn, GameAction action)
    {
        try
        {
            var piece = session.GetPieceById(action.PieceId);
            if (piece == null || !piece.IsAlive)
            {
                return false;
            }

            bool success = false;
            
            switch (action.Type)
            {
                case "Move":
                    try
                    {
                        success = _turnService.ExecuteMove(session, turn, piece, action.TargetPosition);
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    break;
                case "Attack":
                    try
                    {
                        success = _turnService.ExecuteAttack(session, turn, piece, action.TargetPosition);
                    }
                    catch (Exception)
                    {
                        success = false;
                    }
                    break;
                case "Ability":
                    success = ExecuteAbility(session, turn, piece, action);
                    break;
                case "Evolve":
                    success = ExecuteEvolution(session, turn, piece, action);
                    break;
                default:
                    return false;
            }

            return success;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private bool ExecuteAbility(GameSession session, Turn turn, Piece piece, GameAction action)
    {
        try
        {
            var allPieces = session.GetAllPieces().ToList();
            turn.AddAction(new ChessWar.Domain.ValueObjects.TurnAction(
                "Ability",
                piece.Id.ToString(),
                action.TargetPosition
            ));

            var ability = action.AbilityName ?? string.Empty;
            var success = _abilityService.UseAbility(piece, ability, action.TargetPosition, allPieces);
            return success;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private bool ExecuteEvolution(GameSession session, Turn turn, Piece piece, GameAction action)
    {
        return piece.Type == PieceType.Pawn && piece.XP >= 20;
    }
}
