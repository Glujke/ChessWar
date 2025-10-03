using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Entities;

/// <summary>
/// Игровая сессия - бой между двумя игроками
/// </summary>
public class GameSession
{
    public Guid Id { get; private set; }
    public Participant Player1 { get; private set; }
    public Participant Player2 { get; private set; }
    public GameBoard Board { get; private set; }
    public Enums.GameStatus Status { get; private set; }
    public GameResult? Result { get; private set; }
    public Turn? CurrentTurn { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Mode { get; private set; } = "AI";
    public Guid? TutorialSessionId { get; private set; }

    public GameSession(Participant player1, Participant player2, string mode = "AI")
    {
        Id = Guid.NewGuid();
        Player1 = player1 ?? throw new ArgumentNullException(nameof(player1));
        Player2 = player2 ?? throw new ArgumentNullException(nameof(player2));
        Board = new GameBoard();
        Status = Enums.GameStatus.Waiting;
        Mode = string.IsNullOrWhiteSpace(mode) ? "AI" : mode;
        Result = null;
        CurrentTurn = null;
        CreatedAt = DateTime.UtcNow;

        SetupInitialPieces();
    }

    /// <summary>
    /// Начинает игру
    /// </summary>
    public void StartGame()
    {
        if (Status == Enums.GameStatus.Active)
            throw new InvalidOperationException("Game is already active");

        Status = Enums.GameStatus.Active;

        CurrentTurn = new Turn(1, Player1);
    }

    /// <summary>
    /// Завершает игру
    /// </summary>
    public void CompleteGame(GameResult result)
    {
        if (Status != Enums.GameStatus.Active)
            throw new InvalidOperationException("Game is not active");

        Status = Enums.GameStatus.Finished;
        Result = result;
        CurrentTurn = null;
    }

    /// <summary>
    /// Получает текущий ход
    /// </summary>
    public Turn GetCurrentTurn()
    {
        if (Status != Enums.GameStatus.Active)
            throw new InvalidOperationException("Game has not started");

        if (CurrentTurn == null)
            throw new InvalidOperationException("No current turn");

        return CurrentTurn;
    }

    /// <summary>
    /// Устанавливает текущий ход
    /// </summary>
    public void SetCurrentTurn(Turn turn)
    {
        CurrentTurn = turn ?? throw new ArgumentNullException(nameof(turn));
    }

    /// <summary>
    /// Завершает текущий ход и переходит к следующему игроку
    /// </summary>
    public void EndCurrentTurn()
    {
        if (CurrentTurn == null)
            throw new InvalidOperationException("No current turn to end");

        var nextPlayer = CurrentTurn.ActiveParticipant.Id == Player1.Id ? Player2 : Player1;

        var nextTurnNumber = CurrentTurn.Number + 1;
        CurrentTurn = new Turn(nextTurnNumber, nextPlayer);
    }

    /// <summary>
    /// Завершает текущий ход с восстановлением маны
    /// </summary>
    public void EndCurrentTurnWithManaRestore(int manaRegen)
    {
        if (CurrentTurn == null)
            throw new InvalidOperationException("No current turn to end");

        var currentPlayer = CurrentTurn.ActiveParticipant;
        var nextPlayer = currentPlayer.Id == Player1.Id ? Player2 : Player1;

        currentPlayer.Restore(manaRegen);

        var nextTurnNumber = CurrentTurn.Number + 1;
        CurrentTurn = new Turn(nextTurnNumber, nextPlayer);
    }

    /// <summary>
    /// Проверяет, может ли участник выполнить действие в текущем ходе
    /// </summary>
    public bool CanPlayerAct(Participant participant)
    {
        if (CurrentTurn == null)
            return false;

        return CurrentTurn.ActiveParticipant == participant;
    }

    /// <summary>
    /// Получает следующего участника
    /// </summary>
    public Participant GetNextPlayer()
    {
        if (CurrentTurn == null)
            return Player1;

        return CurrentTurn.ActiveParticipant == Player1 ? Player2 : Player1;
    }

    /// <summary>
    /// Получает доску
    /// </summary>
    public GameBoard GetBoard()
    {
        return Board;
    }

    /// <summary>
    /// Получает фигуры первого игрока
    /// </summary>
    public List<Piece> GetPlayer1Pieces()
    {
        return Player1.Pieces;
    }

    /// <summary>
    /// Получает фигуры второго игрока
    /// </summary>
    public List<Piece> GetPlayer2Pieces()
    {
        return Player2.Pieces;
    }

    /// <summary>
    /// Получает все фигуры на доске
    /// </summary>
    public List<Piece> GetAllPieces()
    {
        return Board.Pieces.Where(p => p.IsAlive).ToList();
    }

    /// <summary>
    /// Получает фигуру по ID
    /// </summary>
    public Piece? GetPieceById(string pieceId)
    {
        return GetAllPieces().FirstOrDefault(p => p.Id.ToString() == pieceId);
    }

    /// <summary>
    /// Получает фигуру по позиции
    /// </summary>
    public Piece? GetPieceAtPosition(Position position)
    {
        return Board.GetPieceAt(position);
    }

    /// <summary>
    /// Устанавливает связь с туториалом
    /// </summary>
    public void SetTutorialSessionId(Guid tutorialSessionId)
    {
        TutorialSessionId = tutorialSessionId;
    }

    /// <summary>
    /// Настраивает начальную расстановку фигур
    /// </summary>
    private void SetupInitialPieces()
    {
        foreach (var piece in Player1.Pieces)
        {
            Board.PlacePiece(piece);
        }

        foreach (var piece in Player2.Pieces)
        {
            Board.PlacePiece(piece);
        }
    }
}
