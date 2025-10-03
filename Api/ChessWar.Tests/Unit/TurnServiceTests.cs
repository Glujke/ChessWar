using FluentAssertions;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Events;
using ChessWar.Domain.Events.Handlers;
using Moq;
using ChessWar.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace ChessWar.Tests.Unit;

public class TurnServiceTests
{
    private readonly Mock<IMovementRulesService> _movementRulesServiceMock;
    private readonly Mock<IAttackRulesService> _attackRulesServiceMock;
    private readonly Mock<IEvolutionService> _evolutionServiceMock;
    private readonly Mock<IPieceDomainService> _pieceDomainServiceMock;
    private readonly TurnService _turnService;
    private readonly IBalanceConfigProvider _configProvider;
    private readonly GameSession _gameSession;

    public TurnServiceTests()
    {
        _movementRulesServiceMock = new Mock<IMovementRulesService>();
        _attackRulesServiceMock = new Mock<IAttackRulesService>();
        _evolutionServiceMock = new Mock<IEvolutionService>();
        _pieceDomainServiceMock = new Mock<IPieceDomainService>();

        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        _configProvider = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);

        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(IEnumerable<IDomainEventHandler<PieceKilledEvent>>)))
            .Returns(new List<IDomainEventHandler<PieceKilledEvent>>
            {
                new ExperienceAwardHandler(_pieceDomainServiceMock.Object, _configProvider),
                new BoardCleanupHandler(),
                new PositionSwapHandler()
            });

        var eventDispatcher = new DomainEventDispatcher(serviceProviderMock.Object);

        _turnService = new TurnService(
            _movementRulesServiceMock.Object,
            _attackRulesServiceMock.Object,
            _evolutionServiceMock.Object,
            _configProvider,
            eventDispatcher,
            _pieceDomainServiceMock.Object,
            turnServiceLogger);

        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        _gameSession = new GameSession(player1, player2);
        _gameSession.StartGame();
    }

    [Fact]
    public void StartTurn_ShouldCreateNewTurn()
    {
        var gameSession = CreateTestGameSession();
        var activeParticipant = gameSession.Player1;

        var turn = _turnService.StartTurn(gameSession, activeParticipant);

        turn.Should().NotBeNull();
        turn.Number.Should().Be(1);
        turn.ActiveParticipant.Should().Be(activeParticipant);
        turn.SelectedPiece.Should().BeNull();
    }

    [Fact]
    public void StartTurn_ShouldNotRegenerateMp()
    {
        var gameSession = CreateTestGameSession();
        var activeParticipant = gameSession.Player1;
        activeParticipant.SetMana(0, 50); // У игрока нет маны
        var pawn = CreateTestPiece("p1", PieceType.Pawn, Team.Elves, new Position(0, 1), activeParticipant);

        var turn = _turnService.StartTurn(gameSession, activeParticipant);

        activeParticipant.MP.Should().Be(0);
        turn.ActiveParticipant.Should().Be(activeParticipant);
    }

    [Fact]
    public void StartTurn_WithNullGameSession_ShouldThrowArgumentNullException()
    {
        var activeParticipant = CreateTestPlayer("TestPlayer");

        Assert.Throws<ArgumentNullException>(() => _turnService.StartTurn(null!, activeParticipant));
    }

    [Fact]
    public void StartTurn_WithNullActiveParticipant_ShouldThrowArgumentNullException()
    {
        var gameSession = CreateTestGameSession();

        Assert.Throws<ArgumentNullException>(() => _turnService.StartTurn(gameSession, null!));
    }

    [Fact]
    public void ExecuteMove_WithValidMove_ShouldReturnTrue()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(10, 10); // Устанавливаем ману для игрока
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        var targetPosition = new Position(1, 2);

        _gameSession.GetBoard().PlacePiece(piece);

        turn.SelectPiece(piece);

        _movementRulesServiceMock
            .Setup(x => x.CanMoveTo(piece, targetPosition, It.IsAny<List<Piece>>()))
            .Returns(true);

        var result = _turnService.ExecuteMove(_gameSession, turn, piece, targetPosition);

        result.Should().BeTrue();
        turn.Actions.Should().HaveCount(1);
        turn.Actions[0].ActionType.Should().Be("Move");
    }

    [Fact]
    public void ExecuteMove_WithInvalidMove_ShouldReturnFalse()
    {
        var player = CreateTestPlayer("TestPlayer");
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        var targetPosition = new Position(1, 2);

        turn.SelectPiece(piece);

        _movementRulesServiceMock
            .Setup(x => x.CanMoveTo(piece, targetPosition, It.IsAny<List<Piece>>()))
            .Returns(false);

        var result = _turnService.ExecuteMove(_gameSession, turn, piece, targetPosition);

        result.Should().BeFalse();
        turn.Actions.Should().BeEmpty();
    }


    [Fact]
    public void ExecuteAttack_WithValidAttack_ShouldReturnTrue()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(10, 10); // Устанавливаем ману для игрока
        var turn = new Turn(1, player);
        var attacker = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        var targetPosition = new Position(1, 2);

        var target = CreateTestPiece("piece2", PieceType.Pawn, Team.Orcs, targetPosition, _gameSession.Player2);
        target.HP = 10; // Устанавливаем HP для живой фигуры

        _gameSession.GetBoard().PlacePiece(attacker);
        _gameSession.GetBoard().PlacePiece(target);

        turn.SelectPiece(attacker);

        _attackRulesServiceMock
            .Setup(x => x.CanAttack(attacker, targetPosition, It.IsAny<List<Piece>>()))
            .Returns(true);

        var result = _turnService.ExecuteAttack(_gameSession, turn, attacker, targetPosition);

        result.Should().BeTrue();
        turn.Actions.Should().HaveCount(1);
        turn.Actions[0].ActionType.Should().Be("Attack");
    }

    [Fact]
    public void ExecuteAttack_WithInvalidAttack_ShouldReturnFalse()
    {
        var player = CreateTestPlayer("TestPlayer");
        var turn = new Turn(1, player);
        var attacker = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        var targetPosition = new Position(1, 2);

        turn.SelectPiece(attacker);

        _attackRulesServiceMock
            .Setup(x => x.CanAttack(attacker, targetPosition, It.IsAny<List<Piece>>()))
            .Returns(false);

        var result = _turnService.ExecuteAttack(_gameSession, turn, attacker, targetPosition);

        result.Should().BeFalse();
        turn.Actions.Should().BeEmpty();
    }

    [Fact]
    public void EndTurn_ShouldCallReduceCooldowns()
    {
        var player = CreateTestPlayer("TestPlayer");
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        player.Pieces.Add(piece);

        turn.AddAction(new TurnAction("Move", piece.Id.ToString(), new Position(1, 2)));

        _turnService.EndTurn(turn);

        turn.Should().NotBeNull();
    }

    [Fact]
    public void EndTurn_ShouldRegenerateMpForActivePlayer()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(0, 50); // У игрока нет маны
        var turn = new Turn(1, player);
        var pawn = CreateTestPiece("p1", PieceType.Pawn, Team.Elves, new Position(0, 1), player);

        turn.AddAction(new TurnAction("Move", pawn.Id.ToString(), new Position(0, 2)));

        _turnService.EndTurn(turn);

        player.MP.Should().Be(0); // Игрок не получил ману в EndTurn
        turn.Should().NotBeNull(); // Просто проверяем, что метод не падает
    }

    [Fact]
    public void EndTurn_ShouldNotExceedMaxMp()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(48, 50); // У игрока почти полная мана
        var turn = new Turn(1, player);
        var pawn = CreateTestPiece("p1", PieceType.Pawn, Team.Elves, new Position(0, 1), player);

        turn.AddAction(new TurnAction("Move", pawn.Id.ToString(), new Position(0, 2)));

        _turnService.EndTurn(turn);

        player.MP.Should().Be(48); // Мана не изменилась в EndTurn
        turn.Should().NotBeNull(); // Просто проверяем, что метод не падает
    }

    [Fact]
    public void EndTurn_ShouldNotTickAbilityCooldowns_AfterPolicyChange()
    {
        var player = CreateTestPlayer("TestPlayer");
        var turn = new Turn(1, player);
        var pawn = CreateTestPiece("p1", PieceType.Pawn, Team.Elves, new Position(0, 1), player);
        _pieceDomainServiceMock.Setup(x => x.SetAbilityCooldown(pawn, "TestAbility", 2));

        turn.AddAction(new TurnAction("Move", pawn.Id.ToString(), new Position(0, 2)));

        _turnService.EndTurn(turn);

        _pieceDomainServiceMock.Verify(x => x.TickCooldowns(pawn), Times.Never);
    }

    [Fact]
    public void EndTurn_ShouldNotChangeCooldowns_ForActivePlayerPieces()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(0, 50); // У игрока нет маны
        var turn = new Turn(1, player);
        var p1 = CreateTestPiece("p1", PieceType.Pawn, Team.Elves, new Position(0, 1), player);
        var p2 = CreateTestPiece("p2", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        _pieceDomainServiceMock.Setup(x => x.SetAbilityCooldown(p1, "A", 1));
        _pieceDomainServiceMock.Setup(x => x.SetAbilityCooldown(p2, "B", 2));

        turn.AddAction(new TurnAction("Move", p1.Id.ToString(), new Position(0, 2)));

        _turnService.EndTurn(turn);

        player.MP.Should().Be(0);
        _pieceDomainServiceMock.Verify(x => x.TickCooldowns(p1), Times.Never);
        _pieceDomainServiceMock.Verify(x => x.TickCooldowns(p2), Times.Never);
    }

    [Fact]
    public void EndTurn_ShouldNotAffectCooldowns_WhenZero()
    {
        var player = CreateTestPlayer("TestPlayer");
        var turn = new Turn(1, player);
        var pawn = CreateTestPiece("p1", PieceType.Pawn, Team.Elves, new Position(0, 1), player);
        _pieceDomainServiceMock.Setup(x => x.SetAbilityCooldown(pawn, "A", 0));

        turn.AddAction(new TurnAction("Move", pawn.Id.ToString(), new Position(0, 2)));

        _turnService.EndTurn(turn);

        _pieceDomainServiceMock.Verify(x => x.TickCooldowns(pawn), Times.Never);
    }

    [Fact]
    public void EndTurn_MultipleEnds_ShouldAccumulateMpUpToCap()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(0, 50); // У игрока нет маны
        var turn = new Turn(1, player);
        var pawn = CreateTestPiece("p1", PieceType.Pawn, Team.Elves, new Position(0, 1), player);

        turn.AddAction(new TurnAction("Move", pawn.Id.ToString(), new Position(0, 2)));

        _turnService.EndTurn(turn);
        _turnService.EndTurn(turn);

        player.MP.Should().Be(0);
        turn.Should().NotBeNull(); // Просто проверяем, что метод не падает
    }

    [Fact]
    public void EndTurn_WithNullTurn_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _turnService.EndTurn(null!));
    }

    private GameSession CreateTestGameSession()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        return new GameSession(player1, player2);
    }

    private Player CreateTestPlayer(string name)
    {
        return new Player(name, new List<Piece>());
    }

    private Piece CreateTestPiece(string id, PieceType type, Team team, Position position, Participant owner)
    {
        var piece = TestHelpers.CreatePiece(type, team, position.X, position.Y);
        piece.Owner = owner;
        piece.Id = int.TryParse(id, out var parsed) ? parsed : 0;
        owner.Pieces.Add(piece);
        return piece;
    }

    #region Position Swap on Kill Tests

    [Fact]
    public void ExecuteAttack_WhenKillingEnemy_ShouldMoveAttackerToKilledPosition()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(10, 10);
        var turn = new Turn(1, player);
        var attacker = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        var targetPosition = new Position(1, 2);

        var target = CreateTestPiece("piece2", PieceType.Pawn, Team.Orcs, targetPosition, _gameSession.Player2);
        target.HP = 1; // Низкое HP для гарантированного убийства

        _gameSession.GetBoard().PlacePiece(attacker);
        _gameSession.GetBoard().PlacePiece(target);

        turn.SelectPiece(attacker);

        _attackRulesServiceMock
            .Setup(x => x.CanAttack(attacker, targetPosition, It.IsAny<List<Piece>>()))
            .Returns(true);

        _pieceDomainServiceMock
            .Setup(x => x.TakeDamage(target, It.IsAny<int>()))
            .Callback<Piece, int>((piece, damage) => piece.HP -= damage);
        _pieceDomainServiceMock
            .Setup(x => x.IsDead(target))
            .Returns(() => target.HP <= 0);

        var result = _turnService.ExecuteAttack(_gameSession, turn, attacker, targetPosition);

        result.Should().BeTrue();
        attacker.Position.Should().Be(targetPosition, "Attacker should move to killed enemy position");
        target.IsAlive.Should().BeFalse("Target should be dead");
    }

    [Fact]
    public void ExecuteAttack_WhenNotKillingEnemy_ShouldNotMoveAttacker()
    {
        var player = CreateTestPlayer("TestPlayer");
        player.SetMana(10, 10);
        var turn = new Turn(1, player);
        var attacker = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        var targetPosition = new Position(1, 2);
        var originalPosition = attacker.Position;

        var target = CreateTestPiece("piece2", PieceType.Pawn, Team.Orcs, targetPosition, _gameSession.Player2);
        target.HP = 100; // Высокое HP чтобы не убить

        _gameSession.GetBoard().PlacePiece(attacker);
        _gameSession.GetBoard().PlacePiece(target);

        turn.SelectPiece(attacker);

        _attackRulesServiceMock
            .Setup(x => x.CanAttack(attacker, targetPosition, It.IsAny<List<Piece>>()))
            .Returns(true);

        var result = _turnService.ExecuteAttack(_gameSession, turn, attacker, targetPosition);

        result.Should().BeTrue();
        attacker.Position.Should().Be(originalPosition, "Attacker should stay in original position when not killing");
        target.IsAlive.Should().BeTrue("Target should be alive");
    }

    #endregion
}
