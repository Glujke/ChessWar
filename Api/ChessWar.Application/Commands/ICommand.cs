namespace ChessWar.Application.Commands;

/// <summary>
/// Базовый интерфейс для команд
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Выполняет команду
    /// </summary>
    Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Базовый интерфейс для команд с результатом
/// </summary>
/// <typeparam name="TResult">Тип результата</typeparam>
public interface ICommand<TResult>
{
    /// <summary>
    /// Выполняет команду и возвращает результат
    /// </summary>
    Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
