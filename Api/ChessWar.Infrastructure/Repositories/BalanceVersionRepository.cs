using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Persistence.Core.Entities;
using ChessWar.Persistence.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ChessWar.Infrastructure.Repositories;

public class BalanceVersionRepository : IBalanceVersionRepository
{
    private readonly ChessWarDbContext _context;

    public BalanceVersionRepository(ChessWarDbContext context)
    {
        _context = context;
    }

    public async Task<BalanceVersion?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var dto = await _context.BalanceVersions
            .AsNoTracking()
            .Include(v => v.Pieces)
            .Include(v => v.EvolutionRules)
            .Include(v => v.Globals!)
            .FirstOrDefaultAsync(v => v.Status == "Active", cancellationToken);

        return dto != null ? MapToDomain(dto) : null;
    }

    public async Task<BalanceVersion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await _context.BalanceVersions
            .AsNoTracking()
            .Include(v => v.Pieces)
            .Include(v => v.EvolutionRules)
            .Include(v => v.Globals!)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        return dto != null ? MapToDomain(dto) : null;
    }

    public async Task<IReadOnlyList<BalanceVersion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var dtos = await _context.BalanceVersions
            .AsNoTracking()
            .Include(v => v.Pieces)
            .Include(v => v.EvolutionRules)
            .Include(v => v.Globals!)
            .ToListAsync(cancellationToken);

        return dtos.Select(MapToDomain).ToList();
    }

    public async Task AddAsync(BalanceVersion version, CancellationToken cancellationToken = default)
    {
        var dto = MapToDto(version);
        _context.BalanceVersions.Add(dto);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(BalanceVersion version, CancellationToken cancellationToken = default)
    {
        var dto = MapToDto(version);

        var existingEntity = _context.ChangeTracker.Entries<BalanceVersionDto>()
            .FirstOrDefault(e => e.Entity.Id == dto.Id);

        if (existingEntity != null)
        {
            existingEntity.CurrentValues.SetValues(dto);
        }
        else
        {
            _context.Entry(dto).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<BalanceVersion> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BalanceVersions.AsNoTracking();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(v => v.Status == status);

        var totalCount = await query.CountAsync(cancellationToken);

        var allDtos = await query.ToListAsync(cancellationToken);

        var dtos = allDtos
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = dtos.Select(MapToDomain).ToList();

        return (items, totalCount);
    }

    public async Task<bool> ExistsByVersionAsync(string version, CancellationToken cancellationToken = default)
    {
        return await _context.BalanceVersions
            .AnyAsync(v => v.Version == version, cancellationToken);
    }

    private static BalanceVersion MapToDomain(BalanceVersionDto dto)
    {
        return new BalanceVersion
        {
            Id = dto.Id,
            Version = dto.Version,
            Status = dto.Status,
            Comment = dto.Comment,
            CreatedAt = dto.CreatedAt,
            PublishedAt = dto.PublishedAt,
            PublishedBy = dto.PublishedBy,
            Pieces = dto.Pieces.Select(p => new PieceDefinition
            {
                Id = p.Id,
                PieceId = p.PieceId,
                HP = p.HP,
                ATK = p.ATK,
                Range = p.Range,
                Movement = p.Movement,
                Energy = p.Energy,
                ExpToEvolve = p.ExpToEvolve
            }).ToList(),
            EvolutionRules = dto.EvolutionRules.Select(r => new EvolutionRule
            {
                Id = r.Id,
                From = r.From,
                To = r.To
            }).ToList(),
            Globals = dto.Globals != null ? new GlobalRules
            {
                Id = dto.Globals.Id,
                MpRegenPerTurn = dto.Globals.MpRegenPerTurn,
                CooldownTickPhase = dto.Globals.CooldownTickPhase
            } : null
        };
    }

    private static BalanceVersionDto MapToDto(BalanceVersion domain)
    {
        return new BalanceVersionDto
        {
            Id = domain.Id,
            Version = domain.Version,
            Status = domain.Status,
            Comment = domain.Comment,
            CreatedAt = domain.CreatedAt,
            PublishedAt = domain.PublishedAt,
            PublishedBy = domain.PublishedBy,
            Pieces = domain.Pieces.Select(p => new PieceDefinitionDto
            {
                Id = p.Id,
                PieceId = p.PieceId,
                HP = p.HP,
                ATK = p.ATK,
                Range = p.Range,
                Movement = p.Movement,
                Energy = p.Energy,
                ExpToEvolve = p.ExpToEvolve
            }).ToList(),
            EvolutionRules = domain.EvolutionRules.Select(r => new EvolutionRuleDto
            {
                Id = r.Id,
                From = r.From,
                To = r.To
            }).ToList(),
            Globals = domain.Globals != null ? new GlobalRulesDto
            {
                Id = domain.Globals.Id,
                MpRegenPerTurn = domain.Globals.MpRegenPerTurn,
                CooldownTickPhase = domain.Globals.CooldownTickPhase
            } : null
        };
    }
}
