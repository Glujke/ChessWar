using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Entities;

public class GameState
{
    public int Id { get; set; }
    public GameBoard Board { get; set; } = new();
    public Team CurrentPlayer { get; set; } = Team.Elves;
    public int TurnNumber { get; set; } = 1;
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public Team? Winner { get; set; }
    public List<string> GameLog { get; set; } = new();

    public void SwitchPlayer()
    {
        CurrentPlayer = CurrentPlayer == Team.Elves ? Team.Orcs : Team.Elves;
        if (CurrentPlayer == Team.Elves)
            TurnNumber++;
    }

    public void EndGame(Team winner)
    {
        Status = GameStatus.Finished;
        Winner = winner;
    }

    public void LogAction(string action)
    {
        GameLog.Add($"[Turn {TurnNumber}] {action}");
    }

    public bool IsGameOver()
    {
        return Status == GameStatus.Finished;
    }

    public bool IsKingAlive(Team team)
    {
        var king = Board.GetAlivePiecesByTeam(team)
            .FirstOrDefault(p => p.Type == PieceType.King);
        return king != null && king.IsAlive;
    }
}
public enum GameStatus
{
    InProgress,
    Finished
}

