using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

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
        private readonly IPettyCashService _pettyCashService;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expenseRepository">Repositorio de gastos</param>
        /// <param name="logger">Servicio de logging</param>
        /// <param name="mapper">Servicio de mapeo</param>
        /// <param name="fileService">Servicio de archivos</param>
        /// <param name="pettyCashService">Servicio de caja chica</param>
        /// <param name="notificationService">Servicio de notificaciones</param>
        public ExpenseService(
            IExpenseRepository expenseRepository,
            ILoggerManager logger,
            IMapper mapper,
            IFileService fileService,
            IPettyCashService pettyCashService,
            INotificationService notificationService)
        {
            _expenseRepository = expenseRepository;
            _logger = logger;
            _mapper = mapper;
            _fileService = fileService;
            _pettyCashService = pettyCashService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Obtiene todos los gastos
        /// </summary>
        /// <returns>Lista de gastos</returns>
        public async Task<IEnumerable<Expense>> GetAllExpensesAsync()
        {
            _logger.LogInfo("Servicio: Obteniendo todos los gastos");
            var expenses = await _expenseRepository.GetAllAsync();
            
            // Asegurarnos de que las URLs de las imágenes sean correctas
            foreach (var expense in expenses)
            {
                UpdateImageUrls(expense);
            }
            
            return expenses;
        }

        /// <summary>
        /// Obtiene un gasto por su ID
        /// </summary>
        /// <param name="id">ID del gasto</param>
        /// <returns>Gasto encontrado o null</returns>
        public async Task<Expense> GetExpenseByIdAsync(string id)
        {
            _logger.LogInfo($"Servicio: Obteniendo gasto con ID: {id}");
            var expense = await _expenseRepository.GetByIdAsync(id);
            
            if (expense != null)
            {
                UpdateImageUrls(expense);
            }
            
            return expense;
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
                expense.Images = dto.ImageIds.Select(imageId => 
                {
                    // Asegurarnos de que tenemos la ruta completa
                    string fullPath = imageId;
                    if (!imageId.StartsWith("/uploads/"))
                    {
                        fullPath = $"/uploads/expenses/{imageId}";
                    }
                    
                    _logger.LogInfo($"Servicio: Creando imagen con ID {imageId} y ruta {fullPath}");
                    
                    return new Image
                    {
                        Id = imageId, // Guardar el ID original
                        Url = _fileService.GetImageUrl(fullPath) // Generar la URL completa con la ruta correcta
                    };
                }).ToList();
            }
            
            var result = await _expenseRepository.CreateAsync(expense);
            _logger.LogInfo($"Servicio: Gasto creado con ID: {result.Id}");
            
            // Registrar el gasto en la caja chica
            await RegisterExpenseInPettyCash(result);
            
            // Enviar notificación a los representantes del nuevo gasto
            await SendNewExpenseNotificationAsync(result);
            
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
            
            // Guardar el monto anterior para calcular la diferencia
            decimal previousAmount = existingExpense.Amount;
            
            // Mapear propiedades actualizadas
            _mapper.Map(dto, existingExpense);
            
            // Actualizar imágenes si se proporcionaron
            if (dto.ImageIds != null)
            {
                _logger.LogInfo($"Servicio: Actualizando imágenes del gasto, {dto.ImageIds.Count} imágenes");
                
                // Convertir los IDs de imágenes a objetos Image
                existingExpense.Images = dto.ImageIds.Select(imageId => 
                {
                    // Asegurarnos de que tenemos la ruta completa
                    string fullPath = imageId;
                    if (!imageId.StartsWith("/uploads/"))
                    {
                        fullPath = $"/uploads/expenses/{imageId}";
                    }
                    
                    _logger.LogInfo($"Servicio: Actualizando imagen con ID {imageId} y ruta {fullPath}");
                    
                    return new Image
                    {
                        Id = imageId, // Guardar el ID original
                        Url = _fileService.GetImageUrl(fullPath) // Generar la URL completa con la ruta correcta
                    };
                }).ToList();
            }
            
            var result = await _expenseRepository.UpdateAsync(existingExpense);
            _logger.LogInfo($"Servicio: Gasto actualizado con ID: {result.Id}");
            
            // Registrar la actualización en la caja chica si el monto cambió
            if (previousAmount != result.Amount)
            {
                await RegisterExpenseUpdateInPettyCash(result, previousAmount);
            }
            
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
                // Construir rutas completas para las imágenes si es necesario
                var imagePaths = expense.Images.Select(img => 
                {
                    if (!img.Id.StartsWith("/uploads/"))
                    {
                        return $"/uploads/expenses/{img.Id}";
                    }
                    return img.Id;
                }).ToList();
                
                _fileService.DeleteImages(imagePaths);
                _logger.LogInfo($"Servicio: Se eliminaron {imagePaths.Count} imágenes asociadas al gasto");
            }
            
            // Registrar la eliminación en la caja chica antes de eliminar el gasto
            await RegisterExpenseDeletionInPettyCash(expense);
            
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
            var result = await _expenseRepository.GetPaginatedAsync(page, pageSize);
            
            // Asegurarnos de que las URLs de las imágenes sean correctas
            foreach (var expense in result.expenses)
            {
                UpdateImageUrls(expense);
            }
            
            return result;
        }
        
        /// <summary>
        /// Actualiza las URLs de las imágenes en un gasto
        /// </summary>
        /// <param name="expense">Gasto a actualizar</param>
        private void UpdateImageUrls(Expense expense)
        {
            if (expense.Images != null && expense.Images.Any())
            {
                foreach (var image in expense.Images)
                {
                    // Asegurarnos de que tenemos la ruta completa
                    string fullPath = image.Id;
                    if (!fullPath.StartsWith("/uploads/"))
                    {
                        fullPath = $"/uploads/expenses/{image.Id}";
                    }
                    
                    // Actualizar la URL con la ruta correcta
                    image.Url = _fileService.GetImageUrl(fullPath);
                }
            }
        }

        /// <summary>
        /// Registra un gasto en la caja chica
        /// </summary>
        /// <param name="expense">Gasto a registrar</param>
        private async Task RegisterExpenseInPettyCash(Expense expense)
        {
            try
            {
                _logger.LogInfo($"Servicio: Registrando gasto con ID: {expense.Id} en caja chica");
                
                var transactionDto = new CreateTransactionDto
                {
                    Type = TransactionType.Expense, // Tipo egreso
                    Amount = expense.Amount,
                    Description = $"Gasto: {expense.Name} - {expense.Description}",
                    RelatedEntityId = expense.Id,
                    RelatedEntityType = "Expense",
                    ExpenseId = expense.Id,
                    ExpenseName = expense.Name
                };
                
                await _pettyCashService.AddTransactionAsync(transactionDto);
                
                _logger.LogInfo($"Servicio: Gasto con ID: {expense.Id} registrado en caja chica");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Servicio: Error al registrar gasto en caja chica: {ex.Message}");
                // No lanzamos la excepción para no interrumpir el flujo principal
            }
        }
        
        /// <summary>
        /// Registra la actualización de un gasto en la caja chica
        /// </summary>
        /// <param name="expense">Gasto actualizado</param>
        /// <param name="previousAmount">Monto anterior del gasto</param>
        private async Task RegisterExpenseUpdateInPettyCash(Expense expense, decimal previousAmount)
        {
            try
            {
                _logger.LogInfo($"Servicio: Registrando actualización de gasto con ID: {expense.Id} en caja chica");
                
                decimal difference = expense.Amount - previousAmount;
                
                if (difference == 0)
                {
                    return; // No hay cambio en el monto, no es necesario registrar
                }
                
                string description;
                TransactionType type;
                decimal amount;
                
                if (difference > 0)
                {
                    // El nuevo monto es mayor, registrar un egreso adicional
                    type = TransactionType.Expense;
                    amount = difference;
                    description = $"Incremento en gasto: {expense.Name} - Diferencia: S/ {difference:N2}";
                }
                else
                {
                    // El nuevo monto es menor, registrar un ingreso por la diferencia
                    type = TransactionType.Income;
                    amount = Math.Abs(difference);
                    description = $"Reducción en gasto: {expense.Name} - Diferencia: S/ {amount:N2}";
                }
                
                var transactionDto = new CreateTransactionDto
                {
                    Type = type,
                    Amount = amount,
                    Description = description,
                    RelatedEntityId = expense.Id,
                    RelatedEntityType = "Expense",
                    ExpenseId = expense.Id,
                    ExpenseName = expense.Name
                };
                
                await _pettyCashService.AddTransactionAsync(transactionDto);
                
                _logger.LogInfo($"Servicio: Actualización de gasto con ID: {expense.Id} registrada en caja chica");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Servicio: Error al registrar actualización de gasto en caja chica: {ex.Message}");
                // No lanzamos la excepción para no interrumpir el flujo principal
            }
        }
        
        /// <summary>
        /// Registra la eliminación de un gasto en la caja chica
        /// </summary>
        /// <param name="expense">Gasto eliminado</param>
        private async Task RegisterExpenseDeletionInPettyCash(Expense expense)
        {
            try
            {
                _logger.LogInfo($"Servicio: Registrando eliminación de gasto con ID: {expense.Id} en caja chica");
                
                var transactionDto = new CreateTransactionDto
                {
                    Type = TransactionType.Income, // Tipo ingreso (devuelve el dinero)
                    Amount = expense.Amount,
                    Description = $"Eliminación de gasto: {expense.Name} - Devolución: S/ {expense.Amount:N2}",
                    RelatedEntityId = expense.Id,
                    RelatedEntityType = "Expense",
                    ExpenseId = expense.Id,
                    ExpenseName = expense.Name
                };
                
                await _pettyCashService.AddTransactionAsync(transactionDto);
                
                _logger.LogInfo($"Servicio: Eliminación de gasto con ID: {expense.Id} registrada en caja chica");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Servicio: Error al registrar eliminación de gasto en caja chica: {ex.Message}");
                // No lanzamos la excepción para no interrumpir el flujo principal
            }
        }

        /// <summary>
        /// Envía notificación a los representantes sobre un nuevo gasto
        /// </summary>
        /// <param name="expense">Gasto a notificar</param>
        private async Task SendNewExpenseNotificationAsync(Expense expense)
        {
            try
            {
                _logger.LogInfo($"Servicio: Enviando notificación de nuevo gasto: {expense.Name}");
                
                // Preparar los datos para la notificación
                object notificationData;
                
                // Si hay imágenes, incluir la primera en la notificación (boleta)
                if (expense.Images != null && expense.Images.Any())
                {
                    // Obtener la URL de la primera imagen
                    string imageUrl = expense.Images.First().Url;
                    
                    _logger.LogInfo($"Servicio: Incluyendo imagen en la notificación: {imageUrl}");
                    
                    notificationData = new
                    {
                        ExpenseId = expense.Id,
                        ExpenseName = expense.Name,
                        Description = expense.Description,
                        Amount = expense.Amount,
                        Date = expense.Date,
                        ImageUrl = imageUrl
                    };
                }
                else
                {
                    notificationData = new
                    {
                        ExpenseId = expense.Id,
                        ExpenseName = expense.Name,
                        Description = expense.Description,
                        Amount = expense.Amount,
                        Date = expense.Date
                    };
                }
                
                // Enviar notificación a todos los representantes
                var title = "Nuevo gasto registrado";
                var body = $"Se ha registrado un nuevo gasto: {expense.Name} por S/ {expense.Amount:N2}";
                
                // Enviar la notificación al tema de representantes
                await _notificationService.SendNotificationAsync("Representative", title, body, notificationData);
                
                _logger.LogInfo($"Servicio: Notificación de nuevo gasto enviada exitosamente para: {expense.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Servicio: Error al enviar notificación de nuevo gasto: {ex.Message}");
                // No propagar la excepción para no interrumpir el flujo principal
            }
        }
    }
} 