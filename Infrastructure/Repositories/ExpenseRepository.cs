using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de gastos
    /// </summary>
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly IMongoCollection<Expense> _expenses;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="database">Base de datos MongoDB</param>
        public ExpenseRepository(IMongoDatabase database)
        {
            _expenses = database.GetCollection<Expense>("Expenses");
        }

        /// <summary>
        /// Obtiene todos los gastos
        /// </summary>
        /// <returns>Lista de gastos</returns>
        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            return await _expenses.Find(_ => true)
                .SortByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un gasto por su ID
        /// </summary>
        /// <param name="id">ID del gasto</param>
        /// <returns>Gasto encontrado o null</returns>
        public async Task<Expense> GetByIdAsync(string id)
        {
            return await _expenses.Find(e => e.Id == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Crea un nuevo gasto
        /// </summary>
        /// <param name="expense">Gasto a crear</param>
        /// <returns>Gasto creado</returns>
        public async Task<Expense> CreateAsync(Expense expense)
        {
            expense.CreatedAt = DateTime.UtcNow;
            expense.UpdatedAt = DateTime.UtcNow;
            
            await _expenses.InsertOneAsync(expense);
            
            return expense;
        }

        /// <summary>
        /// Actualiza un gasto existente
        /// </summary>
        /// <param name="expense">Gasto a actualizar</param>
        /// <returns>Gasto actualizado</returns>
        public async Task<Expense> UpdateAsync(Expense expense)
        {
            expense.UpdatedAt = DateTime.UtcNow;
            
            var result = await _expenses.ReplaceOneAsync(
                e => e.Id == expense.Id,
                expense);
            
            return result.ModifiedCount > 0 ? expense : null;
        }

        /// <summary>
        /// Elimina un gasto
        /// </summary>
        /// <param name="id">ID del gasto a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _expenses.DeleteOneAsync(e => e.Id == id);
            
            return result.DeletedCount > 0;
        }

        /// <summary>
        /// Obtiene gastos de forma paginada
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Tupla con la lista de gastos y el total de registros</returns>
        public async Task<(IEnumerable<Expense> expenses, int totalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var totalCount = await _expenses.CountDocumentsAsync(_ => true);
            
            var expenses = await _expenses.Find(_ => true)
                .SortByDescending(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
            
            return (expenses, (int)totalCount);
        }
    }
} 