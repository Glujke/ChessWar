using ChessWar.Domain.Enums;

namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для игровой сессии
/// </summary>
public class GameSessionDto
{
    public Guid Id { get; set; }
    public PlayerDto Player1 { get; set; } = null!;
    public PlayerDto Player2 { get; set; } = null!;
    public GameStatus Status { get; set; }
    public GameResult? Result { get; set; }
    public TurnDto? CurrentTurn { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Mode { get; set; } = "AI";
}

