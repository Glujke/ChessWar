namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для создания игровой сессии
/// </summary>
public class CreateGameSessionDto
{
    public string Player1Name { get; set; } = string.Empty;
    public string Player2Name { get; set; } = string.Empty;
    public string Mode { get; set; } = "AI"; 
    public Guid? TutorialSessionId { get; set; } 
}
