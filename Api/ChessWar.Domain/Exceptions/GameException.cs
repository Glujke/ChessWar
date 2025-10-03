namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Базовое исключение для всех игровых ошибок.
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
