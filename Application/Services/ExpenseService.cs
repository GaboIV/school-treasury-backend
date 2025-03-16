using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Implementación del servicio de gastos
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expenseRepository">Repositorio de gastos</param>
        /// <param name="logger">Servicio de logging</param>
        /// <param name="mapper">Servicio de mapeo</param>
        /// <param name="fileService">Servicio de archivos</param>
        public ExpenseService(
            IExpenseRepository expenseRepository,
            ILoggerManager logger,
            IMapper mapper,
            IFileService fileService)
        {
            _expenseRepository = expenseRepository;
            _logger = logger;
            _mapper = mapper;
            _fileService = fileService;
        }

        /// <summary>
        /// Obtiene todos los gastos
        /// </summary>
        /// <returns>Lista de gastos</returns>
        public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
        {
            _logger.LogInfo("Servicio: Obteniendo todos los gastos");
            return await _expenseRepository.GetAllAsync();
        }

        /// <summary>
        /// Obtiene un gasto por su ID
        /// </summary>
        /// <param name="id">ID del gasto</param>
        /// <returns>Gasto encontrado o null</returns>
        public async Task<Expense> GetExpenseByIdAsync(string id)
        {
            _logger.LogInfo($"Servicio: Obteniendo gasto con ID: {id}");
            return await _expenseRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Crea un nuevo gasto
        /// </summary>
        /// <param name="dto">DTO con los datos del gasto a crear</param>
        /// <returns>Gasto creado</returns>
        public async Task<Expense> CreateExpenseAsync(CreateExpenseDto dto)
        {
            _logger.LogInfo("Servicio: Creando nuevo gasto");
            
            var expense = _mapper.Map<Expense>(dto);
            
            // Asociar imágenes si existen
            if (dto.ImageIds != null && dto.ImageIds.Any())
            {
                _logger.LogInfo($"Servicio: Asociando {dto.ImageIds.Count} imágenes al gasto");
                
                // Convertir los IDs de imágenes a objetos Image
                expense.Images = dto.ImageIds.Select(id => new Image
                {
                    Id = id, // Guardar solo el nombre del archivo
                    Url = _fileService.GetImageUrl(id) // Generar la URL completa
                }).ToList();
            }
            
            var result = await _expenseRepository.CreateAsync(expense);
            _logger.LogInfo($"Servicio: Gasto creado con ID: {result.Id}");
            
            return result;
        }

        /// <summary>
        /// Actualiza un gasto existente
        /// </summary>
        /// <param name="dto">DTO con los datos del gasto a actualizar</param>
        /// <returns>Gasto actualizado</returns>
        public async Task<Expense> UpdateExpenseAsync(UpdateExpenseDto dto)
        {
            _logger.LogInfo($"Servicio: Actualizando gasto con ID: {dto.Id}");
            
            var existingExpense = await _expenseRepository.GetByIdAsync(dto.Id);
            
            if (existingExpense == null)
            {
                _logger.LogWarn($"Servicio: No se encontró el gasto con ID: {dto.Id}");
                return null;
            }
            
            // Mapear propiedades actualizadas
            _mapper.Map(dto, existingExpense);
            
            // Actualizar imágenes si se proporcionaron
            if (dto.ImageIds != null)
            {
                _logger.LogInfo($"Servicio: Actualizando imágenes del gasto, {dto.ImageIds.Count} imágenes");
                
                // Convertir los IDs de imágenes a objetos Image
                existingExpense.Images = dto.ImageIds.Select(id => new Image
                {
                    Id = id, // Guardar solo el nombre del archivo
                    Url = _fileService.GetImageUrl(id) // Generar la URL completa
                }).ToList();
            }
            
            var result = await _expenseRepository.UpdateAsync(existingExpense);
            _logger.LogInfo($"Servicio: Gasto actualizado con ID: {result.Id}");
            
            return result;
        }

        /// <summary>
        /// Elimina un gasto
        /// </summary>
        /// <param name="id">ID del gasto a eliminar</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        public async Task<bool> DeleteExpenseAsync(string id)
        {
            _logger.LogInfo($"Servicio: Eliminando gasto con ID: {id}");
            
            var expense = await _expenseRepository.GetByIdAsync(id);
            
            if (expense == null)
            {
                _logger.LogWarn($"Servicio: No se encontró el gasto con ID: {id} para eliminar");
                return false;
            }
            
            // Eliminar imágenes asociadas si existen
            if (expense.Images != null && expense.Images.Any())
            {
                var imagePaths = expense.Images.Select(img => img.Id).ToList();
                _fileService.DeleteImages(imagePaths);
                _logger.LogInfo($"Servicio: Se eliminaron {imagePaths.Count} imágenes asociadas al gasto");
            }
            
            var result = await _expenseRepository.DeleteAsync(id);
            
            if (result)
            {
                _logger.LogInfo($"Servicio: Gasto eliminado con ID: {id}");
            }
            else
            {
                _logger.LogWarn($"Servicio: No se pudo eliminar el gasto con ID: {id}");
            }
            
            return result;
        }

        /// <summary>
        /// Obtiene gastos de forma paginada
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Tupla con la lista de gastos y el total de registros</returns>
        public async Task<(IEnumerable<Expense> expenses, int totalCount)> GetPaginatedExpensesAsync(int page, int pageSize)
        {
            _logger.LogInfo($"Servicio: Obteniendo gastos paginados. Página: {page}, Tamaño: {pageSize}");
            return await _expenseRepository.GetPaginatedAsync(page, pageSize);
        }

        /// <summary>
        /// Ajusta el monto de un gasto
        /// </summary>
        /// <param name="id">ID del gasto</param>
        /// <param name="dto">DTO con el monto ajustado</param>
        /// <returns>Gasto actualizado</returns>
        public async Task<Expense> AdjustExpenseAmountAsync(string id, AdjustExpenseAmountDto dto)
        {
            _logger.LogInfo($"Servicio: Ajustando monto del gasto con ID: {id}");
            
            var expense = await _expenseRepository.GetByIdAsync(id);
            
            if (expense == null)
            {
                _logger.LogWarn($"Servicio: No se encontró el gasto con ID: {id} para ajustar monto");
                throw new KeyNotFoundException($"No se encontró el gasto con ID: {id}");
            }
            
            expense.AdjustedAmount = dto.AdjustedAmount;
            expense.UpdatedAt = DateTime.UtcNow;
            
            var result = await _expenseRepository.UpdateAsync(expense);
            _logger.LogInfo($"Servicio: Monto del gasto ajustado con ID: {id}, Nuevo monto: {dto.AdjustedAmount}");
            
            return result;
        }
    }
} 