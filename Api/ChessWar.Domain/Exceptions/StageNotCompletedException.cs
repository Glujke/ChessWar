namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Exception thrown when trying to advance tutorial stage that's not completed
/// </summary>
public class StageNotCompletedException : GameException
{
    public string StageName { get; }
    public string RequiredCondition { get; }

    public StageNotCompletedException(string stageName, string requiredCondition)
        : base($"Stage '{stageName}' is not completed. Required: {requiredCondition}")
    {
        if (string.IsNullOrWhiteSpace(stageName))
            throw new ArgumentException("Stage name cannot be null or empty", nameof(stageName));
        if (string.IsNullOrWhiteSpace(requiredCondition))
            throw new ArgumentException("Required condition cannot be null or empty", nameof(requiredCondition));

        StageName = stageName;
        RequiredCondition = requiredCondition;
    }
}
