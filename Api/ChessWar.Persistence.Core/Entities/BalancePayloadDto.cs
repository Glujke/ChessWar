namespace ChessWar.Persistence.Core.Entities;

public class BalancePayloadDto
{
    public Guid Id { get; set; }
    public Guid BalanceVersionId { get; set; }
    public string Json { get; set; } = "{}";

    public BalanceVersionDto? BalanceVersion { get; set; }
}



