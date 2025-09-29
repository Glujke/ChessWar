using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Services.Tutorial;
using ChessWar.Domain.Interfaces.Tutorial;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using Moq;

namespace ChessWar.Tests.Unit;

public class TutorialProgressionServiceTests
{
    [Fact]
    public async Task AdvanceToNextStage_ShouldProgress_Battle1_To_Battle2_To_Boss()
    {
        var modeRepo = new Mock<IGameModeRepository>();
        var hintSvc = new Mock<ITutorialHintService>();
        var mockNotificationService = new Mock<IGameNotificationService>();
        var service = new TutorialService(modeRepo.Object, hintSvc.Object, mockNotificationService.Object);

        var session = new TutorialSession(new Player("player123", Team.Elves));

        modeRepo.Setup(r => r.GetModeByIdAsync<ITutorialMode>(session.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(session);
        modeRepo.Setup(r => r.SaveModeAsync(session, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var s1 = await service.AdvanceToNextStageAsync(session.Id);
        var s2 = await service.AdvanceToNextStageAsync(session.Id);
        var s3 = await service.AdvanceToNextStageAsync(session.Id);

        Assert.Equal(TutorialStage.Battle1, s1.CurrentStage);
        Assert.Equal(TutorialStage.Battle2, s2.CurrentStage);
        Assert.Equal(TutorialStage.Boss, s3.CurrentStage);
        modeRepo.Verify(r => r.SaveModeAsync(It.IsAny<ITutorialMode>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}


