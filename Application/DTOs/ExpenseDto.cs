namespace Application.DTOs;

/// <summary>
/// DTO para representar un gasto
/// </summary>
public class ExpenseDto : BaseDto
{    
    /// <summary>
    /// Nombre del gasto
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Monto original del gasto
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Monto ajustado del gasto (si aplica)
    /// </summary>
    public decimal? AdjustedAmount { get; set; }
    
    /// <summary>
    /// Fecha en que se realizó el gasto
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Descripción del gasto
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Estado del gasto (activo/inactivo)
    /// </summary>
    public bool Status { get; set; }
    
    /// <summary>
    /// Imágenes asociadas al gasto
    /// </summary>
    public ICollection<ImageDto> Images { get; set; } = new List<ImageDto>();
} 