using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;
using System.Collections.Generic;

namespace ChessWar.Domain.Interfaces.GameLogic
{
    /// <summary>
    /// Сервис вычисления доступных целей для способностей.
    /// </summary>
    public interface IAbilityTargetService
    {
        List<Position> GetAvailableTargets(Piece piece, string abilityName, IEnumerable<Piece> allPieces);
    }
}


