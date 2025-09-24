using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Events.Handlers;

/// <summary>
/// Обработчик начисления опыта за убийство
/// </summary>
public class ExperienceAwardHandler : IDomainEventHandler<PieceKilledEvent>
{
    private const int KillExperienceReward = 10;
    private readonly IPieceDomainService _pieceDomainService = new PieceDomainService();

    public void Handle(PieceKilledEvent domainEvent)
    {
        _pieceDomainService.AddExperience(domainEvent.Killer, KillExperienceReward);
    }
}
