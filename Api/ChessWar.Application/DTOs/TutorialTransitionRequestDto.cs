namespace ChessWar.Application.DTOs;

/// <summary>
/// Запрос на переход между этапами обучения
/// </summary>
public class TutorialTransitionRequestDto
{
    public string? Action { get; set; }
    public string? Embed { get; set; }
    public Guid? TutorialSessionId { get; set; }
}

