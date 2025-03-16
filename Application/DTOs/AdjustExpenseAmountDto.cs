namespace Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO para ajustar el monto de un gasto
/// </summary>
public class AdjustExpenseAmountDto
{
    /// <summary>
    /// Identificador único del gasto
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Monto ajustado del gasto
    /// </summary>
    [Required(ErrorMessage = "El monto ajustado es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto ajustado debe ser mayor que cero")]
    public decimal AdjustedAmount { get; set; }
} 