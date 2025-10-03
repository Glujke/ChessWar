using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Services.Configuration;

/// <summary>
/// Сервис управления игроками
/// </summary>
public class PlayerManagementService : IPlayerManagementService
{
    private readonly IPieceFactory _pieceFactory;
    private readonly IPieceIdGenerator _pieceIdGenerator;
    private readonly IBalanceConfigProvider _configProvider;

    public PlayerManagementService(IPieceFactory pieceFactory, IPieceIdGenerator pieceIdGenerator, IBalanceConfigProvider configProvider)
    {
        _pieceFactory = pieceFactory ?? throw new ArgumentNullException(nameof(pieceFactory));
        _pieceIdGenerator = pieceIdGenerator ?? throw new ArgumentNullException(nameof(pieceIdGenerator));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
    }

    public Player CreatePlayerWithInitialPieces(string name, Team team)
    {
        var player = new Player(name, team);

        var config = _configProvider.GetActive();
        player.SetMana(config.PlayerMana.InitialMana, config.PlayerMana.MaxMana);

        var pieces = new List<Piece>();

        for (int i = 0; i < 8; i++)
        {
            var y = team == Team.Elves ? 1 : 6;
            var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, team, new Position(i, y), player);
            pieces.Add(pawn);
        }

        var kingY = team == Team.Elves ? 0 : 7;
        var king = _pieceFactory.CreatePiece(PieceType.King, team, new Position(4, kingY), player);
        pieces.Add(king);

        var backRankY = team == Team.Elves ? 0 : 7;
        var bishop = _pieceFactory.CreatePiece(PieceType.Bishop, team, new Position(2, backRankY), player);
        var queen = _pieceFactory.CreatePiece(PieceType.Queen, team, new Position(3, backRankY), player);
        pieces.Add(bishop);
        pieces.Add(queen);

        foreach (var piece in pieces)
        {
            player.AddPiece(piece);
        }

        return player;
    }

    public ChessWar.Domain.Entities.AI CreateAIWithInitialPieces(Team team)
    {
        var ai = new ChessWar.Domain.Entities.AI("AI", team);

        var config = _configProvider.GetActive();
        ai.SetMana(config.PlayerMana.InitialMana, config.PlayerMana.MaxMana);

        var pieces = new List<Piece>();

        for (int i = 0; i < 8; i++)
        {
            var y = team == Team.Elves ? 1 : 6;
            var pawn = _pieceFactory.CreatePiece(PieceType.Pawn, team, new Position(i, y), ai);
            pieces.Add(pawn);
        }

        var kingY = team == Team.Elves ? 0 : 7;
        var king = _pieceFactory.CreatePiece(PieceType.King, team, new Position(4, kingY), ai);
        pieces.Add(king);

        var backRankY = team == Team.Elves ? 0 : 7;
        var bishop = _pieceFactory.CreatePiece(PieceType.Bishop, team, new Position(2, backRankY), ai);
        var queen = _pieceFactory.CreatePiece(PieceType.Queen, team, new Position(3, backRankY), ai);
        pieces.Add(bishop);
        pieces.Add(queen);

        foreach (var piece in pieces)
        {
            ai.Pieces.Add(piece);
        }

        return ai;
    }

    public Piece? FindPieceById(GameSession gameSession, string pieceId)
    {
        var allPlayerPieces = gameSession.Player1.Pieces.Concat(gameSession.Player2.Pieces);
        var piece = allPlayerPieces.FirstOrDefault(p => p.Id.ToString() == pieceId);

        if (piece != null)
        {
            if (piece.Owner == null)
            {
                if (gameSession.Player1.Pieces.Any(p => p.Id == piece.Id))
                {
                    piece.Owner = gameSession.Player1;
                }
                else if (gameSession.Player2.Pieces.Any(p => p.Id == piece.Id))
                {
                    piece.Owner = gameSession.Player2;
                }
            }
        }

        return piece;
    }
}
