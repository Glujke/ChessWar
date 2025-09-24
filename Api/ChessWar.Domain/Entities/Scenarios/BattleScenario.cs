using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Entities.Scenarios;

/// <summary>
/// Сценарий боя с ИИ противником
/// </summary>
public class BattleScenario : IScenario
{
    public ScenarioType Type { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public int Progress { get; private set; }
    
    /// <summary>
    /// Сложность ИИ противника
    /// </summary>
    public AiDifficulty Difficulty { get; private set; }
    
    /// <summary>
    /// Фигуры противника (ИИ)
    /// </summary>
    public List<Piece> EnemyPieces { get; private set; }
    
    /// <summary>
    /// Показывать ли подсказки игроку
    /// </summary>
    public bool ShowHints { get; private set; }

    public BattleScenario(AiDifficulty difficulty, bool showHints = true)
    {
        Type = ScenarioType.Battle;
        Difficulty = difficulty;
        ShowHints = showHints;
        IsCompleted = false;
        Progress = 0;
        EnemyPieces = new List<Piece>();
        
        (Name, Description) = GetScenarioInfo(difficulty);
    }

    /// <summary>
    /// Настраивает доску для сценария
    /// </summary>
    public void SetupBoard(GameBoard board, Team playerTeam)
    {
        board.Clear();
        SetupEnemyPieces(board, playerTeam);
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

        return Difficulty switch
        {
            AiDifficulty.Easy => "Это легкий противник. Изучите основы боя!",
            AiDifficulty.Medium => "Противник стал сильнее. Используйте способности!",
            AiDifficulty.Hard => "Сложный бой! Планируйте каждый ход!",
            _ => "Сражайтесь с умом!"
        };
    }

    private void SetupEnemyPieces(GameBoard board, Team playerTeam)
    {
        var enemyTeam = playerTeam == Team.Elves ? Team.Orcs : Team.Elves;
        
        var piecesToPlace = Difficulty switch
        {
            AiDifficulty.Easy => GetEasyEnemyPieces(enemyTeam),
            AiDifficulty.Medium => GetMediumEnemyPieces(enemyTeam),
            AiDifficulty.Hard => GetHardEnemyPieces(enemyTeam),
            _ => GetEasyEnemyPieces(enemyTeam)
        };

        foreach (var piece in piecesToPlace)
        {
            board.PlacePiece(piece);
            EnemyPieces.Add(piece);
        }
    }

    private List<Piece> GetEasyEnemyPieces(Team team)
    {
        return new List<Piece>
        {
            new Piece(PieceType.Pawn, team, new Position(6, 0)),
            new Piece(PieceType.Pawn, team, new Position(6, 1)),
            new Piece(PieceType.Knight, team, new Position(7, 0))
        };
    }

    private List<Piece> GetMediumEnemyPieces(Team team)
    {
        return new List<Piece>
        {
            new Piece(PieceType.Pawn, team, new Position(5, 0)),
            new Piece(PieceType.Pawn, team, new Position(5, 1)),
            new Piece(PieceType.Pawn, team, new Position(5, 2)),
            new Piece(PieceType.Knight, team, new Position(6, 0)),
            new Piece(PieceType.Bishop, team, new Position(6, 1))
        };
    }

    private List<Piece> GetHardEnemyPieces(Team team)
    {
        return new List<Piece>
        {
            new Piece(PieceType.Pawn, team, new Position(4, 0)),
            new Piece(PieceType.Pawn, team, new Position(4, 1)),
            new Piece(PieceType.Pawn, team, new Position(4, 2)),
            new Piece(PieceType.Pawn, team, new Position(4, 3)),
            new Piece(PieceType.Knight, team, new Position(5, 0)),
            new Piece(PieceType.Bishop, team, new Position(5, 1)),
            new Piece(PieceType.Rook, team, new Position(6, 0)),
            new Piece(PieceType.Queen, team, new Position(6, 1)),
            new Piece(PieceType.King, team, new Position(7, 0))
        };
    }

    private static (string name, string description) GetScenarioInfo(AiDifficulty difficulty)
    {
        return difficulty switch
        {
            AiDifficulty.Easy => ("Легкий бой", "Изучите основы боя с легким противником"),
            AiDifficulty.Medium => ("Средний бой", "Примените полученные знания против более сильного врага"),
            AiDifficulty.Hard => ("Сложный бой", "Сражение с опытным противником"),
            _ => ("Бой", $"Сражение со сложностью {difficulty}")
        };
    }
}
