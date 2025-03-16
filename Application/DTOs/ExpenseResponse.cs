namespace Application.DTOs;

/// <summary>
/// DTO para la respuesta de gastos
/// </summary>
public class ExpenseResponse
{
    /// <summary>
    /// Datos de la respuesta
    /// </summary>
    public object Data { get; set; }
    
    /// <summary>
    /// Mensaje de la respuesta
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Estado de la respuesta
    /// </summary>
    public string Status { get; set; }
    
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Constructor por defecto
    /// </summary>
    public ExpenseResponse()
    {
        Success = true;
        Status = "Success";
        Message = "Operación completada con éxito";
    }
    
    /// <summary>
    /// Constructor con datos
    /// </summary>
    /// <param name="data">Datos de la respuesta</param>
    public ExpenseResponse(object data)
    {
        Data = data;
        Success = true;
        Status = "Success";
        Message = "Operación completada con éxito";
    }
    
    /// <summary>
    /// Constructor con datos y mensaje
    /// </summary>
    /// <param name="data">Datos de la respuesta</param>
    /// <param name="message">Mensaje de la respuesta</param>
    public ExpenseResponse(object data, string message)
    {
        Data = data;
        Message = message;
        Success = true;
        Status = "Success";
    }
} 