using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Entities.Scenarios;

/// <summary>
/// Сценарий боя с боссом
/// </summary>
public class BossScenario : IScenario
{
    public ScenarioType Type { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public int Progress { get; private set; }
    
    /// <summary>
    /// Тип босса
    /// </summary>
    public BossType BossType { get; private set; }
    
    /// <summary>
    /// Фигуры босса
    /// </summary>
    public List<Piece> BossPieces { get; private set; }
    
    /// <summary>
    /// Показывать ли подсказки игроку
    /// </summary>
    public bool ShowHints { get; private set; }

    public BossScenario(BossType bossType = BossType.KingAndQueen, bool showHints = true)
    {
        Type = ScenarioType.Boss;
        BossType = bossType;
        ShowHints = showHints;
        IsCompleted = false;
        Progress = 0;
        BossPieces = new List<Piece>();
        
        (Name, Description) = GetBossInfo(bossType);
    }

    /// <summary>
    /// Настраивает доску для боя с боссом
    /// </summary>
    public void SetupBoard(GameBoard board, Team playerTeam)
    {
        board.Clear();
        SetupBossPieces(board, playerTeam);
    }

    /// <summary>
    /// Обновляет прогресс сценария
    /// </summary>
    public void UpdateProgress(int progress)
    {
        Progress = Math.Clamp(progress, 0, 100);
        
        if (Progress >= 100)
        {
            CompleteScenario();
        }
    }

    /// <summary>
    /// Завершает сценарий
    /// </summary>
    public void CompleteScenario()
    {
        IsCompleted = true;
        Progress = 100;
    }

    /// <summary>
    /// Получает подсказку для текущего состояния
    /// </summary>
    public string? GetHint()
    {
        if (!ShowHints)
            return null;

        return BossType switch
        {
            BossType.KingAndQueen => "Осторожно! Босс имеет короля и ферзя. Сначала уничтожьте ферзя!",
            BossType.FullArmy => "Это финальный бой! Используйте все свои навыки и способности!",
            _ => "Сражайтесь с боссом и покажите свою силу!"
        };
    }

    /// <summary>
    /// Проверяет, побежден ли босс
    /// </summary>
    public bool IsBossDefeated()
    {
        return BossPieces.All(p => p.HP <= 0);
    }

    private void SetupBossPieces(GameBoard board, Team playerTeam)
    {
        var bossTeam = playerTeam == Team.Elves ? Team.Orcs : Team.Elves;
        
        var piecesToPlace = BossType switch
        {
            BossType.KingAndQueen => GetKingAndQueenPieces(bossTeam),
            BossType.FullArmy => GetFullArmyPieces(bossTeam),
            _ => GetKingAndQueenPieces(bossTeam)
        };

        foreach (var piece in piecesToPlace)
        {
            board.PlacePiece(piece);
            BossPieces.Add(piece);
        }
    }

    private List<Piece> GetKingAndQueenPieces(Team team)
    {
        return new List<Piece>
        {
            new Piece(PieceType.King, team, new Position(7, 0)),
            new Piece(PieceType.Queen, team, new Position(6, 0))
        };
    }

    private List<Piece> GetFullArmyPieces(Team team)
    {
        return new List<Piece>
        {
            new Piece(PieceType.Pawn, team, new Position(3, 0)),
            new Piece(PieceType.Pawn, team, new Position(3, 1)),
            new Piece(PieceType.Pawn, team, new Position(3, 2)),
            new Piece(PieceType.Pawn, team, new Position(3, 3)),
            new Piece(PieceType.Knight, team, new Position(4, 0)),
            new Piece(PieceType.Bishop, team, new Position(4, 1)),
            new Piece(PieceType.Rook, team, new Position(5, 0)),
            new Piece(PieceType.Queen, team, new Position(5, 1)),
            new Piece(PieceType.King, team, new Position(6, 0))
        };
    }

    private static (string name, string description) GetBossInfo(BossType bossType)
    {
        return bossType switch
        {
            BossType.KingAndQueen => ("Король и Ферзь", "Финальный бой против могущественного босса"),
            BossType.FullArmy => ("Полная Армия", "Сражение против всей армии противника"),
            _ => ("Босс", "Финальный вызов")
        };
    }
}

/// <summary>
/// Типы боссов
/// </summary>
public enum BossType
{
    /// <summary>
    /// Король и ферзь
    /// </summary>
    KingAndQueen = 1,
    
    /// <summary>
    /// Полная армия
    /// </summary>
    FullArmy = 2
}
