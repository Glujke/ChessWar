using ChessWar.Domain.Entities;
using ChessWar.Domain.Services.AI;

namespace ChessWar.Tests.Unit.AI
{
    public class GameStateEvaluatorNullPlayerTests
    {
        private readonly GameStateEvaluator _evaluator;

        public GameStateEvaluatorNullPlayerTests()
        {
            _evaluator = new GameStateEvaluator();
        }

        [Fact]
        public void EvaluateKingThreat_WithNullPlayer_ShouldNotThrowException()
        {
            var session = CreateGameSession();

            var result = _evaluator.EvaluateKingThreat(session, null);

            Assert.True(result >= -1000 && result <= 1000);
        }

        [Fact]
        public void EvaluateMaterialAdvantage_WithNullPlayer_ShouldNotThrowException()
        {
            var session = CreateGameSession();

            var result = _evaluator.EvaluateMaterialAdvantage(session, null);

            Assert.True(result >= -1000 && result <= 1000);
        }

        [Fact]
        public void EvaluateGameState_WithNullPlayer_ShouldNotThrowException()
        {
            var session = CreateGameSession();

            var result = _evaluator.EvaluateGameState(session, null);

            Assert.True(result >= -1000 && result <= 1000);
        }

        private GameSession CreateGameSession()
        {
            var player1 = new Player("Player 1", new List<Piece>());
            var player2 = new Player("Player 2", new List<Piece>());
            var session = new GameSession(player1, player2, "Test");
            session.StartGame();
            return session;
        }
    }
}
