using System.Threading;
using System.Threading.Tasks;
using ChessWar.Application.Services.Board;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Очередь обработки ходов для постановки запросов на обработку.
/// </summary>
public interface ITurnProcessingQueue
{
    Task<bool> EnqueueTurnAsync(TurnRequest request);
}



