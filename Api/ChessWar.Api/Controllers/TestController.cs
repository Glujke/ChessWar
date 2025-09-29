using Microsoft.AspNetCore.Mvc;
using ChessWar.Domain.Exceptions;

namespace ChessWar.Api.Controllers;

[ApiController]
[Route("api/v1/test")]
public class TestController : BaseController
{
    public TestController(ILogger<TestController> logger) : base(logger)
    {
    }
    [HttpPost("insufficient-mp")]
    public IActionResult TestInsufficientMp([FromBody] TestInsufficientMpRequest request)
    {
        throw new InsufficientMpException(request.RequiredMp, request.AvailableMp, request.PieceId);
    }

    [HttpPost("stage-not-completed")]
    public IActionResult TestStageNotCompleted([FromBody] TestStageNotCompletedRequest request)
    {
        throw new StageNotCompletedException(request.StageName, request.RequiredCondition);
    }
}

public record TestInsufficientMpRequest(Guid PieceId, int RequiredMp, int AvailableMp);
public record TestStageNotCompletedRequest(string StageName, string RequiredCondition);
