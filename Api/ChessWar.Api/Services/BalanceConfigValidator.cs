using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using ChessWar.Application.Interfaces.Configuration;

namespace ChessWar.Api.Services;

public class BalanceConfigValidator : IBalanceConfigValidator
{
    private readonly ILogger<BalanceConfigValidator> _logger;
    private readonly JSchema _schema;

    public BalanceConfigValidator(ILogger<BalanceConfigValidator> logger)
    {
        _logger = logger;
        _schema = LoadSchema();
    }

    public Task<ValidationResult> ValidateAsync(string json, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonObject = JObject.Parse(json);
            
            var isValid = jsonObject.IsValid(_schema, out IList<string> errorMessages);
            
            if (isValid)
            {
                _logger.LogDebug("JSON validation successful");
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
