namespace ChessWar.Domain.ValueObjects;

/// <summary>
/// Centralized catalog of error codes used throughout the application.
/// Follows the Single Responsibility Principle by managing only error code definitions.
/// </summary>
public static class ErrorCodes
{
    #region Game Action Errors
    
    /// <summary>
    /// Error when a piece doesn't have enough MP to perform an action
    /// </summary>
    public static readonly ErrorCode InsufficientMp = new("InsufficientMp", "Insufficient MP to perform this action");
    
    /// <summary>
    /// Error when trying to use an ability that's currently on cooldown
    /// </summary>
    public static readonly ErrorCode AbilityOnCooldown = new("AbilityOnCooldown", "Ability is currently on cooldown");
    
    /// <summary>
    /// Error when target is out of range for the action
    /// </summary>
    public static readonly ErrorCode OutOfRange = new("OutOfRange", "Target is out of range");
    
    /// <summary>
    /// Error when line of sight is blocked for the action
    /// </summary>
    public static readonly ErrorCode LineOfSightBlocked = new("LineOfSightBlocked", "Line of sight is blocked");
    
    /// <summary>
    /// Error when trying to switch pieces during a turn when it's not allowed
    /// </summary>
    public static readonly ErrorCode PieceSwitchForbidden = new("PieceSwitchForbidden", "Cannot switch pieces during this turn");

    #endregion

    #region Tutorial Errors
    
    /// <summary>
    /// Error when trying to advance tutorial stage that's not completed
    /// </summary>
    public static readonly ErrorCode StageNotCompleted = new("StageNotCompleted", "Current stage is not completed");
    
    /// <summary>
    /// Error when tutorial session is not found
    /// </summary>
    public static readonly ErrorCode TutorialNotFound = new("TutorialNotFound", "Tutorial session not found");

    #endregion

    private static readonly IReadOnlyList<ErrorCode> _allErrorCodes = new[]
    {
        InsufficientMp,
        AbilityOnCooldown,
        OutOfRange,
        LineOfSightBlocked,
        PieceSwitchForbidden,
        
        StageNotCompleted,
        TutorialNotFound
    };

    /// <summary>
    /// Gets all available error codes
    /// </summary>
    /// <returns>Read-only list of all error codes</returns>
    public static IReadOnlyList<ErrorCode> GetAll() => _allErrorCodes;

    /// <summary>
    /// Gets an error code by its string code
    /// </summary>
    /// <param name="code">The error code string</param>
    /// <returns>The matching ErrorCode</returns>
    /// <exception cref="ArgumentException">Thrown when the code is not found</exception>
    public static ErrorCode GetByCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        
        var errorCode = _allErrorCodes.FirstOrDefault(ec => ec.Code == code);
        if (errorCode == null)
        {
            throw new ArgumentException($"Error code '{code}' not found", nameof(code));
        }
        return errorCode;
    }

    /// <summary>
    /// Checks if an error code exists
    /// </summary>
    /// <param name="code">The error code string to check</param>
    /// <returns>True if the code exists, false otherwise</returns>
    public static bool Exists(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;
            
        return _allErrorCodes.Any(ec => ec.Code == code);
    }
}
