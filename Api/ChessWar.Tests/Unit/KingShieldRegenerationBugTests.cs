using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Events;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для исправления бага с регенерацией щита короля
/// </summary>
public class KingShieldRegenerationBugTests
{
    private readonly Mock<ICollectiveShieldService> _collectiveShieldServiceMock;
    private readonly Mock<IBalanceConfigProvider> _configProviderMock;
    private readonly Mock<IAttackRulesService> _attackRulesServiceMock;
    private readonly Mock<IEvolutionService> _evolutionServiceMock;
    private readonly Mock<IDomainEventDispatcher> _eventDispatcherMock;
    private readonly Mock<IPieceDomainService> _pieceDomainServiceMock;
    private readonly Mock<IMovementRulesService> _movementRulesServiceMock;
    private readonly Mock<ILogger<TurnService>> _loggerMock;

    public KingShieldRegenerationBugTests()
    {
        _collectiveShieldServiceMock = new Mock<ICollectiveShieldService>();
        _configProviderMock = new Mock<IBalanceConfigProvider>();
        _attackRulesServiceMock = new Mock<IAttackRulesService>();
        _evolutionServiceMock = new Mock<IEvolutionService>();
        _eventDispatcherMock = new Mock<IDomainEventDispatcher>();
        _pieceDomainServiceMock = new Mock<IPieceDomainService>();
        _movementRulesServiceMock = new Mock<IMovementRulesService>();
        _loggerMock = new Mock<ILogger<TurnService>>();
    }

    /// <summary>
    /// Тест демонстрирует баг: король регенерирует щит при движении пешки
    /// Ожидается: щит короля НЕ должен изменяться при движении пешки
    /// </summary>
    [Fact]
    public void ExecuteMove_WhenPawnMoves_KingShieldShouldNotRegenerate()
    {
       
        var turnService = new TurnService(
            _movementRulesServiceMock.Object,
            _attackRulesServiceMock.Object,
            _evolutionServiceMock.Object,
            _configProviderMock.Object,
            _eventDispatcherMock.Object,
            _pieceDomainServiceMock.Object,
            _collectiveShieldServiceMock.Object,
            _loggerMock.Object);

               var gameSession = CreateTestGameSession();
               var turn = new Turn(1, gameSession.Player1);
               var player1 = gameSession.Player1;
        
        var king = gameSession.Board.Pieces.First(p => p.Type == PieceType.King && p.Team == Team.Elves);
        var pawn = gameSession.Board.Pieces.First(p => p.Type == PieceType.Pawn && p.Team == Team.Elves);
        
       
        king.ShieldHP = 100;
        var initialKingShield = king.ShieldHP;

              
               _movementRulesServiceMock.Setup(x => x.CanMoveTo(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
                   .Returns(true);
               
               _pieceDomainServiceMock.Setup(x => x.MoveTo(It.IsAny<Piece>(), It.IsAny<Position>()));
               
              
               var config = new BalanceConfig
               {
                   Globals = new GlobalsSection(),
                   PlayerMana = new PlayerManaSection
                   {
                       MovementCosts = new Dictionary<string, int> { { "Pawn", 1 } }
                   },
                   Pieces = new Dictionary<string, PieceStats>(),
                   Abilities = new Dictionary<string, AbilitySpecModel>(),
                   Evolution = new EvolutionSection
                   {
                       XpThresholds = new Dictionary<string, int>(),
                       Rules = new Dictionary<string, List<string>>()
                   },
                   Ai = new AiSection(),
                   KillRewards = new KillRewardsSection(),
                   ShieldSystem = new ShieldSystemConfig()
               };
               _configProviderMock.Setup(x => x.GetActive()).Returns(config);

       
        var targetPosition = new Position(pawn.Position.X, pawn.Position.Y + 1);
        var result = turnService.ExecuteMove(gameSession, turn, pawn, targetPosition);

              
               
              
               var canMove = _movementRulesServiceMock.Object.CanMoveTo(pawn, targetPosition, gameSession.Board.Pieces.ToList());
               
              
               try
               {
                   var balanceConfig = _configProviderMock.Object.GetActive();
                   if (balanceConfig != null)
                   {
                   }
               }
               catch (Exception ex)
               {
               }
               
              

       
        result.Should().BeTrue("ход должен быть успешным");
        
       
        king.ShieldHP.Should().Be(initialKingShield, "щит короля не должен регенерироваться при движении пешки");
        
       
        _collectiveShieldServiceMock.Verify(
            x => x.RegenerateKingShield(It.IsAny<Piece>(), It.IsAny<List<Piece>>()),
            Times.Never,
            "RegenerateKingShield не должен вызываться при движении фигуры");
    }

    private GameSession CreateTestGameSession()
    {
        var player1 = new Player("Player1", Team.Elves);
        var player2 = new Player("Player2", Team.Orcs);
        
        var gameSession = new GameSession(player1, player2, "Local");
        
       
        var king = new Piece(PieceType.King, Team.Elves, new Position(4, 0));
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 1));
        
       
        king.Owner = player1;
        pawn.Owner = player1;
        
       
        player1.SetMana(10, 10);
        
        gameSession.Board.PlacePiece(king);
        gameSession.Board.PlacePiece(pawn);
        
        return gameSession;
    }
}
