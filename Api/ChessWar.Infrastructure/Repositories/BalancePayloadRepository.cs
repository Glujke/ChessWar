using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Persistence.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChessWar.Infrastructure.Repositories;

public class BalancePayloadRepository : IBalancePayloadRepository
{
    private readonly ChessWarDbContext _context;

    public BalancePayloadRepository(ChessWarDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetJsonByVersionIdAsync(Guid balanceVersionId, CancellationToken cancellationToken = default)
    {
        var payload = await _context.BalancePayloads
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.BalanceVersionId == balanceVersionId, cancellationToken);
        return payload?.Json;
    }

    public async Task UpsertJsonAsync(Guid balanceVersionId, string json, CancellationToken cancellationToken = default)
    {
        var payload = await _context.BalancePayloads
            .FirstOrDefaultAsync(p => p.BalanceVersionId == balanceVersionId, cancellationToken);
        if (payload == null)
        {
            _context.BalancePayloads.Add(new Persistence.Core.Entities.BalancePayloadDto
            {
                Id = Guid.NewGuid(),
                BalanceVersionId = balanceVersionId,
                Json = json
            });
        }
        else
        {
            payload.Json = json;
            _context.BalancePayloads.Update(payload);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}



