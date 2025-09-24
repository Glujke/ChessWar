using System.ComponentModel.DataAnnotations;

namespace ChessWar.Persistence.Core.Entities;

/// <summary>
/// DTO для игрока в базе данных
/// </summary>
public class PlayerDto
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public int Victories { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int MP { get; set; }
    
    public int MaxMP { get; set; }
}
