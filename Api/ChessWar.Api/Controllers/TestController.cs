using Microsoft.AspNetCore.Mvc;
using ChessWar.Domain.Exceptions;

namespace ChessWar.Api.Controllers;

[ApiController]
[Route("api/v1/test")]
/// <summary>
/// Тестовый контроллер для генерации предсказуемых исключений (для проверки middleware)
/// </summary>
public class TestController : BaseController
{
    /// <summary>
    /// Создаёт тестовый контроллер
    /// </summary>
    public TestController(ILogger<TestController> logger) : base(logger)
    {
    }
    /// <summary>
    /// Сгенерировать исключение недостатка маны (InsufficientMpException)
    /// </summary>
    [HttpPost("insufficient-mp")]
    public IActionResult TestInsufficientMp([FromBody] TestInsufficientMpRequest request)
    {
        throw new InsufficientMpException(request.RequiredMp, request.AvailableMp, request.PieceId);
    }

    /// <summary>
    /// Сгенерировать исключение незавершённой стадии (StageNotCompletedException)
    /// </summary>
    [HttpPost("stage-not-completed")]
    public IActionResult TestStageNotCompleted([FromBody] TestStageNotCompletedRequest request)
    {
        throw new StageNotCompletedException(request.StageName, request.RequiredCondition);
    }
}

public record TestInsufficientMpRequest(Guid PieceId, int RequiredMp, int AvailableMp);
public record TestStageNotCompletedRequest(string StageName, string RequiredCondition);
