using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.GameLogic;

public interface IAbilityService
{
    bool CanUseAbility(Piece piece, string abilityName, Position target, List<Piece> allPieces);
    bool UseAbility(Piece piece, string abilityName, Position target, List<Piece> allPieces);
}


