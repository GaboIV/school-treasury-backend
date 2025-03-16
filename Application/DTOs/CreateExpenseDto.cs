namespace Application.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO para crear un nuevo gasto
/// </summary>
public class CreateExpenseDto
{    
    /// <summary>
    /// Nombre del gasto
    /// </summary>
    [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres")]
    public string Name { get; set; }
    
    /// <summary>
    /// Monto del gasto
    /// </summary>
    [Required(ErrorMessage = "El monto es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Fecha en que se realizó el gasto
    /// </summary>
    [Required(ErrorMessage = "La fecha del gasto es obligatoria")]
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Descripción del gasto
    /// </summary>
    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
    public string Description { get; set; }
    
    /// <summary>
    /// Estado del gasto (activo/inactivo)
    /// </summary>
    public bool Status { get; set; } = true;
    
    /// <summary>
    /// IDs de las imágenes asociadas al gasto
    /// </summary>
    public List<string> ImageIds { get; set; } = new List<string>();
} 