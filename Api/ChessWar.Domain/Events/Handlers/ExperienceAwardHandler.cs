using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;

namespace ChessWar.Domain.Events.Handlers;

/// <summary>
/// Обработчик начисления опыта за убийство
/// </summary>
public class ExperienceAwardHandler : IDomainEventHandler<PieceKilledEvent>
{
    private readonly IPieceDomainService _pieceDomainService;
    private readonly IBalanceConfigProvider _configProvider;

    public ExperienceAwardHandler(IPieceDomainService pieceDomainService, IBalanceConfigProvider configProvider)
    {
        _pieceDomainService = pieceDomainService;
        _configProvider = configProvider;
    }

    public void Handle(PieceKilledEvent domainEvent)
    {
        var config = _configProvider.GetActive();
        var reward = GetKillReward(domainEvent.Victim.Type, config.KillRewards);
        _pieceDomainService.AddExperience(domainEvent.Killer, reward);
    }

    private static int GetKillReward(Enums.PieceType pieceType, Domain.Entities.Config.KillRewardsSection killRewards)
    {
        return pieceType switch
        {
            Enums.PieceType.Pawn => killRewards.Pawn,
            Enums.PieceType.Knight => killRewards.Knight,
            Enums.PieceType.Bishop => killRewards.Bishop,
            Enums.PieceType.Rook => killRewards.Rook,
            Enums.PieceType.Queen => killRewards.Queen,
            Enums.PieceType.King => killRewards.King,
            _ => killRewards.Pawn // fallback to pawn reward
        };
    }
}
