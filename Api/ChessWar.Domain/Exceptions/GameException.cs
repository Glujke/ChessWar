namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Base exception for all game-related errors
/// </summary>
public abstract class GameException : Exception
{
    protected GameException(string message) : base(message)
    {
    }

    protected GameException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
