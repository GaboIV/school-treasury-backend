namespace Application.Interfaces;

using Domain.Entities;

/// <summary>
/// Interfaz para el repositorio de gastos
/// </summary>
public interface IExpenseRepository
{
    /// <summary>
    /// Obtiene todos los gastos
    /// </summary>
    /// <returns>Lista de gastos</returns>
    Task<IEnumerable<Expense>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un gasto por su ID
    /// </summary>
    /// <param name="id">ID del gasto</param>
    /// <returns>Gasto encontrado o null</returns>
    Task<Expense> GetByIdAsync(string id);
    
    /// <summary>
    /// Crea un nuevo gasto
    /// </summary>
    /// <param name="expense">Gasto a crear</param>
    /// <returns>Gasto creado</returns>
    Task<Expense> CreateAsync(Expense expense);
    
    /// <summary>
    /// Actualiza un gasto existente
    /// </summary>
    /// <param name="expense">Gasto a actualizar</param>
    /// <returns>Gasto actualizado</returns>
    Task<Expense> UpdateAsync(Expense expense);
    
    /// <summary>
    /// Elimina un gasto
    /// </summary>
    /// <param name="id">ID del gasto a eliminar</param>
    /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
    Task<bool> DeleteAsync(string id);
    
    /// <summary>
    /// Obtiene gastos de forma paginada
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Tupla con la lista de gastos y el total de registros</returns>
    Task<(IEnumerable<Expense> expenses, int totalCount)> GetPaginatedAsync(int page, int pageSize);
} 