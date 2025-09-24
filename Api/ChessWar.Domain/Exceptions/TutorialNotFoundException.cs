namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Exception thrown when tutorial session is not found
/// </summary>
public class TutorialNotFoundException : GameException
{
    public Guid TutorialId { get; }

    public TutorialNotFoundException(Guid tutorialId)
        : base($"Tutorial session {tutorialId} not found")
    {
        if (tutorialId == Guid.Empty)
            throw new ArgumentException("Tutorial ID cannot be empty", nameof(tutorialId));

        TutorialId = tutorialId;
    }
}
