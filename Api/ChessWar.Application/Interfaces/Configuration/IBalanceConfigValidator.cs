namespace ChessWar.Application.Interfaces.Configuration;

public interface IBalanceConfigValidator
{
    Task<ValidationResult> ValidateAsync(string json, CancellationToken cancellationToken = default);
}

public class ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
}
