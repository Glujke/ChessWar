using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.Configuration;

public interface IPieceFactory
{
    Piece CreatePiece(PieceType type, Team team, Position position, Participant? owner = null);
}

public class PieceFactory : IPieceFactory
{
    private readonly IBalanceConfigProvider _configProvider;
    private readonly IPieceIdGenerator _pieceIdGenerator;

    public PieceFactory(IBalanceConfigProvider configProvider, IPieceIdGenerator pieceIdGenerator)
    {
        _configProvider = configProvider;
        _pieceIdGenerator = pieceIdGenerator;
    }

    public Piece CreatePiece(PieceType type, Team team, Position position, Participant? owner = null)
    {
        var config = _configProvider.GetActive();
        var pieceTypeName = type.ToString();

        if (!config.Pieces.TryGetValue(pieceTypeName, out var stats))
        {
            throw new InvalidOperationException($"No stats found for piece type: {pieceTypeName}");
        }

        var piece = new Piece
        {
            Id = _pieceIdGenerator.GetNextId(),
            Type = type,
            Team = team,
            Position = position,
            Owner = owner,
            HP = stats.Hp,
            ATK = stats.Atk,
            Range = stats.Range,
            Movement = stats.Movement,
            XP = 0,
            XPToEvolve = stats.XpToEvolve,
            MaxShieldHP = stats.MaxShieldHP,
            ShieldHP = 0,
            IsFirstMove = true,
            AbilityCooldowns = new Dictionary<string, int>()
        };


        return piece;
    }
}
