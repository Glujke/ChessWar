using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Services.GameManagement;

/// <summary>
/// Фабрика для создания игровых сессий
/// </summary>
public class GameSessionFactory : IGameSessionFactory
{
    private readonly IPieceFactory _pieceFactory;
    private readonly IPieceIdGenerator _pieceIdGenerator;
    private readonly IBalanceConfigProvider _configProvider;

    public GameSessionFactory(
        IPieceFactory pieceFactory,
        IPieceIdGenerator pieceIdGenerator,
        IBalanceConfigProvider configProvider)
    {
        _pieceFactory = pieceFactory ?? throw new ArgumentNullException(nameof(pieceFactory));
        _pieceIdGenerator = pieceIdGenerator ?? throw new ArgumentNullException(nameof(pieceIdGenerator));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
    }

    public GameSession CreateGameSession(CreateGameSessionDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Player1Name))
            throw new ArgumentException("Player1 name cannot be empty", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Player2Name))
            throw new ArgumentException("Player2 name cannot be empty", nameof(dto));

        var mode = (dto.Mode ?? string.Empty).Trim();
        if (!string.Equals(mode, "AI", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(mode, "LocalCoop", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid session mode. Allowed values: 'AI' or 'LocalCoop'", nameof(dto.Mode));
        }

        var player1 = CreatePlayerWithInitialPieces(dto.Player1Name, Team.Elves);
        Participant player2 = dto.Player2Name == "AI" ? CreateAIWithInitialPieces("AI", Team.Orcs) : CreatePlayerWithInitialPieces(dto.Player2Name, Team.Orcs);

        var gameSession = new GameSession(player1, player2, string.Equals(mode, "LocalCoop", StringComparison.OrdinalIgnoreCase) ? "LocalCoop" : "AI");
        
        
        if (dto.TutorialSessionId.HasValue)
        {
            gameSession.SetTutorialSessionId(dto.TutorialSessionId.Value);
        }
        else
        {
            
        }
        
        return gameSession;
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

        foreach (var piece in pieces)
        {
            player.AddPiece(piece);
        }
        
        return player;
    }

    public ChessWar.Domain.Entities.AI CreateAIWithInitialPieces(string name, Team team)
    {
        var ai = new ChessWar.Domain.Entities.AI(name, team);
        
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

        foreach (var piece in pieces)
        {
            ai.AddPiece(piece);
        }
        
        return ai;
    }
}
