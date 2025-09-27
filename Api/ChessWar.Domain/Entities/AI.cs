using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Entities;

/// <summary>
/// Искусственный интеллект в системе Chess War
/// </summary>
public class AI : Participant
{
    private readonly Team _team;

    public AI(string name, Team team) : base(name)
    {
        _team = team;
    }

    public override bool IsAI => true;

    public override Team Team => _team;
}


