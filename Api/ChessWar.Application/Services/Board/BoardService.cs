using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;

namespace ChessWar.Application.Services.Board;

public class BoardService : IBoardService
{
    private readonly IPieceRepository _pieceRepository;
    private readonly IPieceFactory _pieceFactory;
    private const int BoardSize = 8;

    public BoardService(IPieceRepository pieceRepository, IPieceFactory pieceFactory)
    {
        _pieceRepository = pieceRepository;
        _pieceFactory = pieceFactory;
    }

    public async Task<GameBoard> GetBoardAsync(CancellationToken cancellationToken = default)
    {
        var pieces = await _pieceRepository.GetAllAsync(cancellationToken);
        var gameBoard = new GameBoard();
        
        foreach (var piece in pieces)
        {
            if (piece.IsAlive && piece.Position != null)
            {
                gameBoard.SetPieceAt(piece.Position, piece);
            }
        }
        
        return gameBoard;
    }

    public async Task ResetBoardAsync(CancellationToken cancellationToken = default)
    {
        var pieces = await _pieceRepository.GetAllAsync(cancellationToken);
        foreach (var piece in pieces)
        {
            await _pieceRepository.DeleteAsync(piece, cancellationToken);
        }
    }

    public async Task SetupInitialPositionAsync(CancellationToken cancellationToken = default)
    {
        var pieces = new List<Piece>();
        
        for (int i = 0; i < 8; i++)
        {
            pieces.Add(_pieceFactory.CreatePiece(PieceType.Pawn, Team.Elves, new Position(i, 1)));
        }
        pieces.Add(_pieceFactory.CreatePiece(PieceType.King, Team.Elves, new Position(4, 0)));
        
        for (int i = 0; i < 8; i++)
        {
            pieces.Add(_pieceFactory.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(i, 6)));
        }
        pieces.Add(_pieceFactory.CreatePiece(PieceType.King, Team.Orcs, new Position(4, 7)));

        foreach (var piece in pieces)
        {
            await _pieceRepository.AddAsync(piece, cancellationToken);
        }
    }

    public async Task<Piece> PlacePieceAsync(PieceType type, Team team, Position position, CancellationToken cancellationToken = default)
    {
        if (!IsPositionOnBoard(position))
            throw new ArgumentException("Position is outside the board boundaries");

        if (!await IsPositionFreeAsync(position, cancellationToken))
            throw new InvalidOperationException("Position is occupied");

        var piece = _pieceFactory.CreatePiece(type, team, position);
        await _pieceRepository.AddAsync(piece, cancellationToken);
        return piece;
    }

    public async Task<Piece> MovePieceAsync(int pieceId, Position newPosition, CancellationToken cancellationToken = default)
    {
        if (!IsPositionOnBoard(newPosition))
            throw new ArgumentException("Position is outside the board boundaries");

        var piece = await _pieceRepository.GetByIdAsync(pieceId, cancellationToken);
        if (piece == null)
            throw new InvalidOperationException("Piece not found");

        if (!await IsPositionFreeAsync(newPosition, cancellationToken))
            throw new InvalidOperationException("Position is occupied");

        piece.Position = newPosition;
        await _pieceRepository.UpdateAsync(piece, cancellationToken);
        return piece;
    }

    public async Task<Piece?> GetPieceAtPositionAsync(Position position, CancellationToken cancellationToken = default)
    {
        return await _pieceRepository.GetByPositionAsync(position, cancellationToken);
    }

    public async Task<bool> IsPositionFreeAsync(Position position, CancellationToken cancellationToken = default)
    {
        var piece = await _pieceRepository.GetByPositionAsync(position, cancellationToken);
        return piece == null;
    }

    public bool IsPositionOnBoard(Position position)
    {
        return position.X >= 0 && position.X < BoardSize && position.Y >= 0 && position.Y < BoardSize;
    }

    public int GetBoardSize()
    {
        return BoardSize;
    }
}
