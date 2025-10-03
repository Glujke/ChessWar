using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Tests.Unit;

internal static class TestHelpers
{
    private static readonly IPieceDomainService _pieceDomainService = new PieceDomainService();

    public static IPieceFactory CreatePieceFactory()
    {
        var provider = _TestConfig.CreateProvider();
        var idGenerator = new ChessWar.Application.Services.Pieces.PieceIdGenerator();
        return new PieceFactory(provider, idGenerator);
    }

    public static Piece CreatePiece(PieceType type, Team team, int x = 0, int y = 0)
    {
        var factory = CreatePieceFactory();
        var tempOwner = new Player("TempOwner", new List<Piece>());
        return factory.CreatePiece(type, team, new Position(x, y), tempOwner);
    }

    public static Piece CreatePiece(PieceType type, Team team, Position position, Participant owner, int? id = null)
    {
        var factory = CreatePieceFactory();
        var piece = factory.CreatePiece(type, team, position, owner);
        if (id.HasValue)
        {
            piece.Id = id.Value;
        }
        owner.AddPiece(piece);
        return piece;
    }

    public static Piece CreatePiece(PieceType type, Team team, int x, int y, Participant owner, int? id = null)
    {
        return CreatePiece(type, team, new Position(x, y), owner, id);
    }

    public static void TakeDamage(Piece piece, int damage)
    {
        _pieceDomainService.TakeDamage(piece, damage);
    }

    public static void AddXP(Piece piece, int xp)
    {
        _pieceDomainService.AddXP(piece, xp);
    }

    public static void SetAbilityCooldown(Piece piece, string abilityName, int cooldown)
    {
        _pieceDomainService.SetAbilityCooldown(piece, abilityName, cooldown);
    }

    public static int GetMaxHP(Piece piece)
    {
        return _pieceDomainService.GetMaxHP(piece.Type);
    }
}


