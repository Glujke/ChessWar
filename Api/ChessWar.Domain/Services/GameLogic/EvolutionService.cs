using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;

namespace ChessWar.Domain.Services.GameLogic;

public class EvolutionService : IEvolutionService
{
    private readonly IBalanceConfigProvider _configProvider;
    private readonly List<EvolutionRecord> _evolutionHistory;

    public EvolutionService(IBalanceConfigProvider configProvider)
    {
        _configProvider = configProvider;
        _evolutionHistory = new List<EvolutionRecord>();
    }

    public bool CanEvolve(Piece piece)
    {
        if (piece == null) return false;
        
        var config = _configProvider.GetActive();
        var possibleEvolutions = GetPossibleEvolutions(piece.Type);
        if (!possibleEvolutions.Any()) return false;
        
        var reachedLastRank = piece.Type == PieceType.Pawn &&
            config.Evolution.ImmediateOnLastRank?.GetValueOrDefault("Pawn", false) == true &&
            ((piece.Team == Team.Elves && piece.Position.Y == 7) ||
             (piece.Team == Team.Orcs && piece.Position.Y == 0));

        return piece.CanEvolve || reachedLastRank;
    }

    public Piece EvolvePiece(Piece piece, PieceType targetType)
    {
        if (!CanEvolve(piece))
            throw new InvalidOperationException("Piece cannot evolve");
        
        if (!MeetsEvolutionRequirements(piece, targetType))
        {
            var allowImmediate = piece.Type == PieceType.Pawn &&
                ((piece.Team == Team.Elves && piece.Position.Y == 7) ||
                 (piece.Team == Team.Orcs && piece.Position.Y == 0));
            if (!allowImmediate)
                throw new InvalidOperationException("Piece does not meet evolution requirements");
        }
        
        var evolvedPiece = CreateEvolvedPiece(targetType, piece.Team, piece.Position);
        LogEvolution(piece.Id, targetType);
        
        return evolvedPiece;
    }

    public List<PieceType> GetPossibleEvolutions(PieceType currentType)
    {
        var config = _configProvider.GetActive();
        var typeName = currentType.ToString();
        
        if (!config.Evolution.Rules.TryGetValue(typeName, out var targetNames))
            return new List<PieceType>();
            
        return targetNames.Select(name => Enum.Parse<PieceType>(name)).ToList();
    }

    public bool MeetsEvolutionRequirements(Piece piece, PieceType targetType)
    {
        if (piece == null) return false;
        
        var config = _configProvider.GetActive();
        var possibleEvolutions = GetPossibleEvolutions(piece.Type);
        if (!possibleEvolutions.Contains(targetType)) return false;
        
        var typeName = piece.Type.ToString();
        var requiredXP = config.Evolution.XpThresholds.GetValueOrDefault(typeName, 0);
        return piece.XP >= requiredXP;
    }

    public List<EvolutionRecord> GetEvolutionHistory()
    {
        return _evolutionHistory.ToList();
    }


    private Piece CreateEvolvedPiece(PieceType newType, Team team, Position position)
    {
        return new Piece(newType, team, position);
    }

    private void LogEvolution(int pieceId, PieceType newType)
    {
        _evolutionHistory.Add(new EvolutionRecord(pieceId, newType, DateTime.UtcNow));
    }
}
