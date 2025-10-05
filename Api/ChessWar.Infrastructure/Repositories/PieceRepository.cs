using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.ValueObjects;
using ChessWar.Persistence.Sqlite;
using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChessWar.Infrastructure.Repositories;

public class PieceRepository : IPieceRepository
{
    private readonly ChessWarDbContext _context;

    public PieceRepository(ChessWarDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Piece>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pieces = await _context.Pieces
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return pieces.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Piece>> GetByTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        var pieces = await _context.Pieces
            .AsNoTracking()
            .Where(p => p.Team == team)
            .ToListAsync(cancellationToken);

        return pieces.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Piece>> GetAlivePiecesAsync(CancellationToken cancellationToken = default)
    {
        var pieces = await _context.Pieces
            .AsNoTracking()
            .Where(p => p.HP > 0)
            .ToListAsync(cancellationToken);

        return pieces.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Piece>> GetAlivePiecesByTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        var pieces = await _context.Pieces
            .AsNoTracking()
            .Where(p => p.Team == team && p.HP > 0)
            .ToListAsync(cancellationToken);

        return pieces.Select(MapToDomain).ToList();
    }

    public async Task<Piece?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var piece = await _context.Pieces
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return piece != null ? MapToDomain(piece) : null;
    }

    public async Task<Piece?> GetByPositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        var piece = await _context.Pieces
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PositionX == position.X && p.PositionY == position.Y, cancellationToken);

        return piece != null ? MapToDomain(piece) : null;
    }

    public async Task AddAsync(Piece piece, CancellationToken cancellationToken = default)
    {
        var dto = MapToDto(piece);
        _context.Pieces.Add(dto);
        await _context.SaveChangesAsync(cancellationToken);

        piece.Id = dto.Id;
    }

    public async Task UpdateAsync(Piece piece, CancellationToken cancellationToken = default)
    {
        var existingDto = await _context.Pieces.FindAsync(piece.Id);
        if (existingDto != null)
        {
            existingDto.HP = piece.HP;
            existingDto.ATK = piece.ATK;
            existingDto.XP = piece.XP;
            existingDto.XPToEvolve = piece.XPToEvolve;
            existingDto.PositionX = piece.Position.X;
            existingDto.PositionY = piece.Position.Y;
            existingDto.IsFirstMove = piece.IsFirstMove;
            existingDto.Movement = piece.Movement;
            existingDto.Range = piece.Range;
            existingDto.AbilityCooldownsJson = System.Text.Json.JsonSerializer.Serialize(piece.AbilityCooldowns);
            existingDto.ShieldHp = piece.ShieldHP;
            existingDto.NeighborCount = piece.NeighborCount;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Piece piece, CancellationToken cancellationToken = default)
    {
        var existingDto = await _context.Pieces.FindAsync(piece.Id);
        if (existingDto != null)
        {
            _context.Pieces.Remove(existingDto);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static Piece MapToDomain(PieceDto dto)
    {
        var piece = new Piece
        {
            Id = dto.Id,
            Type = dto.Type,
            Team = dto.Team,
            Position = new Position(dto.PositionX, dto.PositionY),
            HP = dto.HP,
            ATK = dto.ATK,
            Range = dto.Range,
            Movement = dto.Movement,
            XP = dto.XP,
            XPToEvolve = dto.XPToEvolve,
            IsFirstMove = dto.IsFirstMove,
            ShieldHP = dto.ShieldHp,
            NeighborCount = dto.NeighborCount
        };

        try
        {
            piece.AbilityCooldowns = JsonSerializer.Deserialize<Dictionary<string, int>>(dto.AbilityCooldownsJson) ?? new();
        }
        catch
        {
            piece.AbilityCooldowns = new();
        }

        return piece;
    }

    private static PieceDto MapToDto(Piece piece)
    {
        return new PieceDto
        {
            Id = piece.Id,
            Type = piece.Type,
            Team = piece.Team,
            PositionX = piece.Position.X,
            PositionY = piece.Position.Y,
            HP = piece.HP,
            ATK = piece.ATK,
            Range = piece.Range,
            Movement = piece.Movement,
            XP = piece.XP,
            XPToEvolve = piece.XPToEvolve,
            IsFirstMove = piece.IsFirstMove,
            AbilityCooldownsJson = JsonSerializer.Serialize(piece.AbilityCooldowns),
            ShieldHp = piece.ShieldHP,
            NeighborCount = piece.NeighborCount
        };
    }
}
