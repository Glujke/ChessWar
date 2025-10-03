using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using ChessWar.Application.Interfaces.Configuration;

namespace ChessWar.Api.Services;

/// <summary>
/// Валидатор JSON-конфигурации баланса по JSON Schema
/// </summary>
public class BalanceConfigValidator : IBalanceConfigValidator
{
    private readonly ILogger<BalanceConfigValidator> _logger;
    private readonly JSchema _schema;

    /// <summary>
    /// Создаёт экземпляр валидатора и загружает схему
    /// </summary>
    public BalanceConfigValidator(ILogger<BalanceConfigValidator> logger)
    {
        _logger = logger;
        _schema = LoadSchema();
    }

    /// <summary>
    /// Валидирует JSON по схеме
    /// </summary>
    /// <param name="json">JSON-строка конфигурации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат валидации с перечнем ошибок</returns>
    public Task<ValidationResult> ValidateAsync(string json, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonObject = JObject.Parse(json);

            var isValid = jsonObject.IsValid(_schema, out IList<string> errorMessages);

            if (isValid)
            {
                return Task.FromResult(new ValidationResult { IsValid = true });
            }

            var errors = errorMessages?.ToList() ?? new List<string>();
            _logger.LogWarning("JSON validation failed: {Errors}", string.Join("; ", errors));

            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                Errors = errors
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format");
            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"Invalid JSON format: {ex.Message}" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during validation");
            return Task.FromResult(new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { $"Validation error: {ex.Message}" }
            });
        }
    }

    /// <summary>
    /// Загружает JSON Schema из файла
    /// </summary>
    /// <returns>Экземпляр JSchema</returns>
    private static JSchema LoadSchema()
    {
        var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schemas", "balance-config-schema.json");

        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException($"Schema file not found: {schemaPath}");
        }

        using var file = File.OpenText(schemaPath);
        using var reader = new JsonTextReader(file);
        return JSchema.Load(reader);
    }
}
