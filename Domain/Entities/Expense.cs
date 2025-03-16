using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

/// <summary>
/// Representa un gasto en el sistema
/// </summary>
[BsonIgnoreExtraElements] 
public class Expense : BaseEntity
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
    public bool Status { get; set; } = true;
    
    /// <summary>
    /// Relación con el tipo de gasto (reutilizamos CollectionType)
    /// </summary>
    public virtual CollectionType ExpenseType { get; set; }
    
    /// <summary>
    /// Imágenes asociadas al gasto
    /// </summary>
    public virtual ICollection<Image> Images { get; set; } = new List<Image>();
} 