using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.DataAccess; using ChessWar.Domain.Interfaces.Configuration;

namespace ChessWar.Application.Services.Pieces;

public class PieceService : IPieceService
{
    private readonly IPieceRepository _pieceRepository;
    private readonly IPieceFactory _pieceFactory;

    public PieceService(IPieceRepository pieceRepository, IPieceFactory pieceFactory)
    {
        _pieceRepository = pieceRepository;
        _pieceFactory = pieceFactory;
    }

    public async Task<Piece> CreatePieceAsync(PieceType type, Team team, Position position, CancellationToken cancellationToken = default)
    {
        if (!IsPositionOnBoard(position))
            throw new ArgumentException("Position is outside the board boundaries");

        if (!await IsPositionFreeAsync(position, cancellationToken))
            throw new InvalidOperationException("Position is already occupied");

        var piece = _pieceFactory.CreatePiece(type, team, position);
        await _pieceRepository.AddAsync(piece, cancellationToken);
        return piece;
    }

    public async Task<Piece?> GetPieceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _pieceRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<Piece>> GetAllPiecesAsync(CancellationToken cancellationToken = default)
    {
        return await _pieceRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Piece>> GetPiecesByTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        return await _pieceRepository.GetByTeamAsync(team, cancellationToken);
    }

    public async Task<IReadOnlyList<Piece>> GetAlivePiecesAsync(CancellationToken cancellationToken = default)
    {
        return await _pieceRepository.GetAlivePiecesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Piece>> GetAlivePiecesByTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        return await _pieceRepository.GetAlivePiecesByTeamAsync(team, cancellationToken);
    }

    public async Task<Piece> UpdatePiecePositionAsync(int pieceId, Position newPosition, CancellationToken cancellationToken = default)
    {
        if (!IsPositionOnBoard(newPosition))
            throw new ArgumentException("Position is outside the board boundaries");

        var piece = await _pieceRepository.GetByIdAsync(pieceId, cancellationToken);
        if (piece == null)
            throw new InvalidOperationException("Piece not found");

        if (!await IsPositionFreeAsync(newPosition, cancellationToken))
            throw new InvalidOperationException("Position is already occupied");

        piece.Position = newPosition;
        await _pieceRepository.UpdateAsync(piece, cancellationToken);
        return piece;
    }

    public async Task<Piece> UpdatePieceStatsAsync(int pieceId, int? hp = null, int? atk = null, int? mp = null, int? xp = null, CancellationToken cancellationToken = default)
    {
        var piece = await _pieceRepository.GetByIdAsync(pieceId, cancellationToken);
        if (piece == null)
            throw new InvalidOperationException("Piece not found");

        if (hp.HasValue) piece.HP = hp.Value;
        if (atk.HasValue) piece.ATK = atk.Value;;
        if (xp.HasValue) piece.XP = xp.Value;

        await _pieceRepository.UpdateAsync(piece, cancellationToken);
        return piece;
    }

    public async Task DeletePieceAsync(int pieceId, CancellationToken cancellationToken = default)
    {
        var piece = await _pieceRepository.GetByIdAsync(pieceId, cancellationToken);
        if (piece == null)
            throw new InvalidOperationException("Piece not found");

        await _pieceRepository.DeleteAsync(piece, cancellationToken);
    }

    public async Task<bool> IsPositionFreeAsync(Position position, CancellationToken cancellationToken = default)
    {
        var piece = await _pieceRepository.GetByPositionAsync(position, cancellationToken);
        return piece == null;
    }

    public bool IsPositionOnBoard(Position position)
    {
        return position.X >= 0 && position.X < 8 && position.Y >= 0 && position.Y < 8;
    }
}
