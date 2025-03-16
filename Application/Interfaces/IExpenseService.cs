namespace Application.Interfaces;

using Domain.Entities;
using Application.DTOs;

/// <summary>
/// Interfaz para el servicio de gastos
/// </summary>
public interface IExpenseService
{
    /// <summary>
    /// Obtiene todos los gastos
    /// </summary>
    /// <returns>Lista de gastos</returns>
    Task<IEnumerable<Expense>> GetAllExpensesAsync();
    
    /// <summary>
    /// Obtiene un gasto por su ID
    /// </summary>
    /// <param name="id">ID del gasto</param>
    /// <returns>Gasto encontrado o null</returns>
    Task<Expense> GetExpenseByIdAsync(string id);
    
    /// <summary>
    /// Crea un nuevo gasto
    /// </summary>
    /// <param name="dto">DTO con los datos del gasto a crear</param>
    /// <returns>Gasto creado</returns>
    Task<Expense> CreateExpenseAsync(CreateExpenseDto dto);
    
    /// <summary>
    /// Actualiza un gasto existente
    /// </summary>
    /// <param name="dto">DTO con los datos del gasto a actualizar</param>
    /// <returns>Gasto actualizado</returns>
    Task<Expense> UpdateExpenseAsync(UpdateExpenseDto dto);
    
    /// <summary>
    /// Elimina un gasto
    /// </summary>
    /// <param name="id">ID del gasto a eliminar</param>
    /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
    Task<bool> DeleteExpenseAsync(string id);
    
    /// <summary>
    /// Obtiene gastos de forma paginada
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Tupla con la lista de gastos y el total de registros</returns>
    Task<(IEnumerable<Expense> expenses, int totalCount)> GetPaginatedExpensesAsync(int page, int pageSize);
} 