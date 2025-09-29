namespace ChessWar.Application.DTOs;

public class ConfigVersionDto
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public class ConfigVersionListDto
{
    public List<ConfigVersionDto> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class CreateConfigVersionDto
{
    public string Version { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public class UpdateConfigVersionDto
{
    public string Version { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
