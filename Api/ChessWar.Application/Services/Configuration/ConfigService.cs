using ChessWar.Application.Interfaces.Configuration; using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.DataAccess;

namespace ChessWar.Application.Services.Configuration;

public class ConfigService : IConfigService
{
    private readonly IBalanceVersionRepository _balanceVersionRepository;
    private readonly IPieceConfigService _pieceConfigService;
    private readonly IBalancePayloadRepository _payloadRepository;
    private readonly IBalanceConfigValidator _validator;

    public ConfigService(IBalanceVersionRepository balanceVersionRepository, IPieceConfigService pieceConfigService, IBalancePayloadRepository payloadRepository, IBalanceConfigValidator validator)
    {
        _balanceVersionRepository = balanceVersionRepository;
        _pieceConfigService = pieceConfigService;
        _payloadRepository = payloadRepository;
        _validator = validator;
    }

    public async Task<BalanceVersion?> GetActiveConfigAsync(CancellationToken cancellationToken = default)
    {
        return await _pieceConfigService.GetActiveAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<BalanceVersion> Items, int TotalCount)> GetConfigVersionsAsync(
        int page, 
        int pageSize, 
        string? status = null, 
        CancellationToken cancellationToken = default)
    {
        return await _balanceVersionRepository.GetPagedAsync(page, pageSize, status, cancellationToken);
    }

    public async Task<BalanceVersion> CreateConfigVersionAsync(string version, string comment, CancellationToken cancellationToken = default)
    {
        if (await _balanceVersionRepository.ExistsByVersionAsync(version, cancellationToken))
        {
            throw new InvalidOperationException($"Version '{version}' already exists.");
        }

        var balanceVersion = new BalanceVersion
        {
            Id = Guid.NewGuid(),
            Version = version,
            Status = "Draft",
            Comment = comment,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _balanceVersionRepository.AddAsync(balanceVersion, cancellationToken);
        return balanceVersion;
    }

    public async Task<BalanceVersion> UpdateConfigVersionAsync(Guid id, string version, string comment, CancellationToken cancellationToken = default)
    {
        var balanceVersion = await _balanceVersionRepository.GetByIdAsync(id, cancellationToken);
        if (balanceVersion == null)
            throw new InvalidOperationException("Config version not found");
        
        if (balanceVersion.Status != "Draft")
            throw new InvalidOperationException("Only Draft versions can be updated");

        if (balanceVersion.Version != version && 
            await _balanceVersionRepository.ExistsByVersionAsync(version, cancellationToken))
        {
            throw new InvalidOperationException($"Version '{version}' already exists.");
        }

        balanceVersion.Version = version;
        balanceVersion.Comment = comment;

        await _balanceVersionRepository.UpdateAsync(balanceVersion, cancellationToken);
        return balanceVersion;
    }

    public async Task<BalanceVersion> PublishConfigVersionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var balanceVersion = await _balanceVersionRepository.GetByIdAsync(id, cancellationToken);
        if (balanceVersion == null)
            throw new InvalidOperationException("Config version not found");
        
        if (balanceVersion.Status != "Draft")
            throw new InvalidOperationException("Only Draft versions can be published");

        var currentActive = await _balanceVersionRepository.GetActiveAsync(cancellationToken);
        if (currentActive != null)
        {
            currentActive.Status = "Published";
            await _balanceVersionRepository.UpdateAsync(currentActive, cancellationToken);
        }

        balanceVersion.Status = "Active";
        balanceVersion.PublishedAt = DateTimeOffset.UtcNow;
        balanceVersion.PublishedBy = "api";

        await _balanceVersionRepository.UpdateAsync(balanceVersion, cancellationToken);
        return balanceVersion;
    }

    public async Task SavePayloadAsync(Guid versionId, string json, CancellationToken cancellationToken = default)
    {
        var version = await _balanceVersionRepository.GetByIdAsync(versionId, cancellationToken);
        if (version == null)
            throw new InvalidOperationException("Config version not found");
        if (version.Status != "Draft")
            throw new InvalidOperationException("Only Draft versions can be modified");

        var validationResult = await _validator.ValidateAsync(json, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid configuration JSON: {string.Join("; ", validationResult.Errors)}");
        }

        await _payloadRepository.UpsertJsonAsync(versionId, json, cancellationToken);
    }

    public Task<string?> GetPayloadAsync(Guid versionId, CancellationToken cancellationToken = default)
    {
        return _payloadRepository.GetJsonByVersionIdAsync(versionId, cancellationToken);
    }
}
