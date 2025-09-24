using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.Configuration;

namespace ChessWar.Application.Services.Configuration;

/// <summary>
/// Простая реализация пресетов (минимально достаточная раскладка)
/// </summary>
public class BattlePresetService : IBattlePresetService
{
    private readonly IPieceFactory _pieceFactory;
    private readonly IPieceIdGenerator _pieceIdGenerator;

    public BattlePresetService(IPieceFactory pieceFactory, IPieceIdGenerator pieceIdGenerator)
    {
        _pieceFactory = pieceFactory;
        _pieceIdGenerator = pieceIdGenerator;
    }
    public Task ApplyPresetAsync(GameSession session, string stage, CancellationToken cancellationToken = default)
    {
        var board = session.GetBoard();
        board.Clear();

        var playerPieces = session.GetPlayer1Pieces();
        var enemyPieces = session.GetPlayer2Pieces();

        playerPieces.Clear();
        enemyPieces.Clear();
        
        session.Player1.ClearPieces();
        session.Player2.ClearPieces();

        PresetAddPlayerCore(playerPieces, Team.Elves, session.Player1);

        switch (stage)
        {
            case "Battle1":
                PresetAddEnemyBattle1(enemyPieces, Team.Orcs, session.Player2);
                break;
            case "Battle2":
                PresetAddEnemyBattle2(enemyPieces, Team.Orcs, session.Player2);
                break;
            case "Boss":
                PresetAddEnemyBoss(enemyPieces, Team.Orcs, session.Player2);
                break;
            default:
                PresetAddEnemyBattle1(enemyPieces, Team.Orcs, session.Player2);
                break;
        }

        foreach (var p in playerPieces) board.PlacePiece(p);
        foreach (var p in enemyPieces) board.PlacePiece(p);

        return Task.CompletedTask;
    }

    private void PresetAddPlayerCore(List<Piece> pieces, Team team, Player owner)
    {
        for (int x = 0; x < 8; x++)
        {
            var piece = _pieceFactory.CreatePiece(PieceType.Pawn, team, new Position(x, 1), owner);
            piece.Id = _pieceIdGenerator.GetNextId();
            pieces.Add(piece);
        }
        var king = _pieceFactory.CreatePiece(PieceType.King, team, new Position(4, 0), owner);
        king.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(king);
    }

    private void PresetAddEnemyBattle1(List<Piece> pieces, Team team, Player owner)
    {
        for (int x = 0; x < 8; x++)
        {
            var piece = _pieceFactory.CreatePiece(PieceType.Pawn, team, new Position(x, 6), owner);
            piece.Id = _pieceIdGenerator.GetNextId();
            pieces.Add(piece);
        }
        var king = _pieceFactory.CreatePiece(PieceType.King, team, new Position(4, 7), owner);
        king.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(king);
    }

    private void PresetAddEnemyBattle2(List<Piece> pieces, Team team, Player owner)
    {
        for (int x = 0; x < 8; x++)
        {
            if (x % 2 == 0)
            {
                var piece = _pieceFactory.CreatePiece(PieceType.Pawn, team, new Position(x, 6), owner);
                piece.Id = _pieceIdGenerator.GetNextId();
                pieces.Add(piece);
            }
        }
        var knight = _pieceFactory.CreatePiece(PieceType.Knight, team, new Position(2, 5), owner);
        knight.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(knight);
        var bishop = _pieceFactory.CreatePiece(PieceType.Bishop, team, new Position(5, 5), owner);
        bishop.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(bishop);
        var king = _pieceFactory.CreatePiece(PieceType.King, team, new Position(4, 7), owner);
        king.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(king);
    }

    private void PresetAddEnemyBoss(List<Piece> pieces, Team team, Player owner)
    {
        var queen = _pieceFactory.CreatePiece(PieceType.Queen, team, new Position(3, 6), owner);
        queen.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(queen);
        var king = _pieceFactory.CreatePiece(PieceType.King, team, new Position(4, 7), owner);
        king.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(king);
        var pawn1 = _pieceFactory.CreatePiece(PieceType.Pawn, team, new Position(0, 6), owner);
        pawn1.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(pawn1);
        var pawn2 = _pieceFactory.CreatePiece(PieceType.Pawn, team, new Position(7, 6), owner);
        pawn2.Id = _pieceIdGenerator.GetNextId();
        pieces.Add(pawn2);
    }
}


