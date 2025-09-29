namespace ChessWar.Domain.Interfaces.DataAccess;

public interface IBalancePayloadRepository
{
    Task<string?> GetJsonByVersionIdAsync(Guid balanceVersionId, CancellationToken cancellationToken = default);
    Task UpsertJsonAsync(Guid balanceVersionId, string json, CancellationToken cancellationToken = default);
}



