using System.ComponentModel.DataAnnotations;

namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для запроса проверки атаки
/// </summary>
public class AttackRequestDto
{
    /// <summary>
    /// ID атакующей фигуры
    /// </summary>
    [Required]
    public int AttackerId { get; set; }

    /// <summary>
    /// X координата цели
    /// </summary>
    [Required]
    [Range(0, 7, ErrorMessage = "X coordinate must be between 0 and 7")]
    public int TargetX { get; set; }

    /// <summary>
    /// Y координата цели
    /// </summary>
    [Required]
    [Range(0, 7, ErrorMessage = "Y coordinate must be between 0 and 7")]
    public int TargetY { get; set; }
}
