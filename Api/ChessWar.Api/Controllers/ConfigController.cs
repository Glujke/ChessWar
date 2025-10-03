using Microsoft.AspNetCore.Mvc;
using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Configuration;
using AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace ChessWar.Api.Controllers;

/// <summary>
/// Контроллер для управления конфигурациями баланса (версии, публикация, полезные данные)
/// </summary>
public class ConfigController : BaseController
{
    private readonly IConfigService _configService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Создаёт контроллер конфигураций
    /// </summary>
    public ConfigController(IConfigService configService, IMapper mapper, ILogger<ConfigController> logger) : base(logger)
    {
        _configService = configService;
        _mapper = mapper;
    }

    /// <summary>
    /// Возвращает активную версию конфигурации
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveConfig(CancellationToken ct)
    {
        try
        {
            LogInformation("Getting active config");
            var version = await _configService.GetActiveConfigAsync(ct);
            if (version is null)
            {
                LogWarning("No active config found");
                return NotFound();
            }

            var configDto = _mapper.Map<ConfigVersionDto>(version);
            LogInformation("Active config retrieved: {Version}", version.Version);
            return Ok(configDto);
        }
        catch (Exception ex)
        {
            LogError(ex, "Error getting active config");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Возвращает постраничный список версий конфигураций
    /// </summary>
    [HttpGet("versions")]
    public async Task<IActionResult> GetVersions(
        int page = 1,
        int pageSize = 50,
        string? status = null,
        CancellationToken ct = default)
    {
        var (items, total) = await _configService.GetConfigVersionsAsync(page, pageSize, status, ct);

        var itemsDto = _mapper.Map<List<ConfigVersionDto>>(items);
        var response = new ConfigVersionListDto
        {
            Items = itemsDto,
            Total = total,
            Page = page,
            PageSize = pageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// Создаёт новую версию конфигурации
    /// </summary>
    [HttpPost("versions")]
    public async Task<IActionResult> CreateVersion([FromBody] CreateConfigVersionDto dto, CancellationToken ct)
    {
        try
        {
            var version = await _configService.CreateConfigVersionAsync(dto.Version, dto.Comment ?? string.Empty, ct);
            var configDto = _mapper.Map<ConfigVersionDto>(version);
            return Created($"/api/v1/config/versions/{version.Id}", configDto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Обновляет существующую версию конфигурации
    /// </summary>
    [HttpPut("versions/{id:guid}")]
    public async Task<IActionResult> UpdateVersion(Guid id, [FromBody] UpdateConfigVersionDto dto, CancellationToken ct)
    {
        try
        {
            var version = await _configService.UpdateConfigVersionAsync(id, dto.Version, dto.Comment ?? string.Empty, ct);
            var configDto = _mapper.Map<ConfigVersionDto>(version);
            return Ok(configDto);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found") ? NotFound() : Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Публикует указанную версию конфигурации
    /// </summary>
    [HttpPost("versions/{id:guid}/publish")]
    public async Task<IActionResult> PublishVersion(Guid id, CancellationToken ct)
    {
        try
        {
            var version = await _configService.PublishConfigVersionAsync(id, ct);
            var configDto = _mapper.Map<ConfigVersionDto>(version);
            return Ok(configDto);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("not found") ? NotFound() : Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Сохраняет JSON-полезную нагрузку для указанной версии
    /// </summary>
    [HttpPut("versions/{id:guid}/payload")]
    public async Task<IActionResult> SavePayload(Guid id, [FromBody] SavePayloadRequest request, CancellationToken ct)
    {
        await _configService.SavePayloadAsync(id, request.Json, ct);
        return NoContent();
    }

    /// <summary>
    /// Возвращает JSON-полезную нагрузку для указанной версии
    /// </summary>
    [HttpGet("versions/{id:guid}/payload")]
    public async Task<IActionResult> GetPayload(Guid id, CancellationToken ct)
    {
        var json = await _configService.GetPayloadAsync(id, ct);
        if (json == null) return NotFound();
        return Ok(json);
    }
}

public sealed class SavePayloadRequest
{
    [Required]
    public string Json { get; set; } = string.Empty;
}
