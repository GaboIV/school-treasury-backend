using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Presentation.Controllers;

/// <summary>
/// Controlador para gestionar gastos
/// </summary>
[ApiController]
[Route("api/v1/expenses")]
public class ExpenseController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly IMapper _mapper;
    private readonly ILoggerManager _logger;
    private readonly IFileService _fileService;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="expenseService">Servicio de gastos</param>
    /// <param name="mapper">Servicio de mapeo</param>
    /// <param name="logger">Servicio de logging</param>
    /// <param name="fileService">Servicio de archivos</param>
    public ExpenseController(IExpenseService expenseService, IMapper mapper, ILoggerManager logger, IFileService fileService)
    {
        _expenseService = expenseService;
        _mapper = mapper;
        _logger = logger;
        _fileService = fileService;
    }

    /// <summary>
    /// Obtiene todos los gastos
    /// </summary>
    /// <returns>Lista de gastos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllExpenses()
    {
        _logger.LogInfo("Controlador: GET api/v1/expenses - Obteniendo todos los gastos");
        try
        {
            var expenses = await _expenseService.GetAllExpensesAsync();
            var expenseDtos = _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
            var response = new ExpenseResponse(expenseDtos);
            _logger.LogInfo($"Controlador: Se obtuvieron {expenseDtos.Count()} gastos correctamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Controlador: Error al obtener todos los gastos");
            return StatusCode(500, new ExpenseResponse { Success = false, Status = "Error", Message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene un gasto por su ID
    /// </summary>
    /// <param name="id">ID del gasto</param>
    /// <returns>Gasto encontrado</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExpenseById(string id)
    {
        _logger.LogInfo($"Controlador: GET api/v1/expenses/{id} - Obteniendo gasto por ID");
        try
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            
            if (expense == null)
            {
                _logger.LogWarn($"Controlador: No se encontró el gasto con ID: {id}");
                return NotFound(new ExpenseResponse { Success = false, Status = "Not Found", Message = "Gasto no encontrado" });
            }
            
            var expenseDto = _mapper.Map<ExpenseDto>(expense);
            var response = new ExpenseResponse(expenseDto);
            _logger.LogInfo($"Controlador: Se obtuvo el gasto con ID: {id} correctamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Controlador: Error al obtener el gasto con ID: {id}");
            return StatusCode(500, new ExpenseResponse { Success = false, Status = "Error", Message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Crea un nuevo gasto
    /// </summary>
    /// <returns>Gasto creado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateExpense(
        [FromForm] string name, 
        [FromForm] decimal amount,
        [FromForm] DateTime date, 
        [FromForm] string description, 
        [FromForm] bool status, 
        [FromForm] string expenseTypeId,
        [FromForm] IFormFile[] images)
    {
        _logger.LogInfo("Controlador: POST api/v1/expenses - Creando nuevo gasto");
        
        try
        {
            // Crear el DTO manualmente
            var dto = new CreateExpenseDto
            {
                Name = name,
                Amount = amount,
                Date = date,
                Description = description,
                Status = status,
                ExpenseTypeId = expenseTypeId
            };
            
            // Guardar las imágenes y obtener sus IDs
            if (images != null && images.Length > 0)
            {
                var imagePaths = await _fileService.SaveImagesAsync(images.ToList(), "expenses");
                
                // Extraer solo los nombres de archivo de las rutas completas
                var imageIds = imagePaths.Select(path => Path.GetFileName(path)).ToList();
                
                dto.ImageIds = imageIds;
                _logger.LogInfo($"Se guardaron {imageIds.Count} imágenes para el nuevo gasto");
            }
            
            var expense = await _expenseService.CreateExpenseAsync(dto);
            var expenseDto = _mapper.Map<ExpenseDto>(expense);
            var response = new ExpenseResponse(expenseDto);
            _logger.LogInfo($"Controlador: Gasto creado correctamente con ID: {expense.Id}");
            return CreatedAtAction(nameof(GetExpenseById), new { id = expense.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Controlador: Error al crear gasto");
            return StatusCode(500, new ExpenseResponse { 
                Success = false, 
                Status = "Error", 
                Message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>
    /// Actualiza un gasto existente
    /// </summary>
    /// <returns>Gasto actualizado</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateExpense(
        [FromForm] string id,
        [FromForm] string name, 
        [FromForm] decimal amount,
        [FromForm] DateTime date, 
        [FromForm] string description, 
        [FromForm] bool status, 
        [FromForm] string expenseTypeId,
        [FromForm] List<string> existingImageIds,
        [FromForm] IFormFile[] images)
    {
        _logger.LogInfo($"Controlador: PUT api/v1/expenses - Actualizando gasto con ID: {id}");
        
        try
        {
            // Crear el DTO manualmente
            var dto = new UpdateExpenseDto
            {
                Id = id,
                Name = name,
                Amount = amount,
                Date = date,
                Description = description,
                Status = status,
                ExpenseTypeId = expenseTypeId
            };
            
            // Validar el ID
            if (string.IsNullOrEmpty(dto.Id))
            {
                _logger.LogWarn("Controlador: ID inválido al actualizar gasto");
                return BadRequest(new ExpenseResponse { 
                    Success = false, 
                    Status = "Bad Request", 
                    Message = "El ID es obligatorio" 
                });
            }
            
            // Procesar imágenes existentes
            List<string> imageIds = new List<string>();
            
            if (existingImageIds != null && existingImageIds.Any())
            {
                foreach (var imgId in existingImageIds)
                {
                    if (!string.IsNullOrEmpty(imgId))
                    {
                        // Extraer solo el nombre del archivo, independientemente del formato de la ruta
                        string fileName = Path.GetFileName(imgId);
                        _logger.LogInfo($"Procesando imagen existente: {imgId} -> {fileName}");
                        imageIds.Add(fileName);
                    }
                }
            }
            
            // Guardar las nuevas imágenes si existen
            if (images != null && images.Length > 0)
            {
                var imagePaths = await _fileService.SaveImagesAsync(images.ToList(), "expenses");
                
                // Extraer solo los nombres de archivo de las rutas completas
                var newImageIds = imagePaths.Select(path => Path.GetFileName(path)).ToList();
                
                imageIds.AddRange(newImageIds);
                _logger.LogInfo($"Se guardaron {newImageIds.Count} nuevas imágenes para el gasto");
            }
            
            // Asignar los IDs de imágenes al DTO
            dto.ImageIds = imageIds;
            
            var expense = await _expenseService.UpdateExpenseAsync(dto);
            
            if (expense == null)
            {
                _logger.LogWarn($"Controlador: No se encontró el gasto con ID: {dto.Id} para actualizar");
                return NotFound(new ExpenseResponse { 
                    Success = false, 
                    Status = "Not Found", 
                    Message = "Gasto no encontrado" 
                });
            }
            
            var expenseDto = _mapper.Map<ExpenseDto>(expense);
            var response = new ExpenseResponse(expenseDto);
            _logger.LogInfo($"Controlador: Gasto actualizado correctamente con ID: {expense.Id}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Controlador: Error al actualizar gasto con ID: {id}");
            return StatusCode(500, new ExpenseResponse { 
                Success = false, 
                Status = "Error", 
                Message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>
    /// Elimina un gasto
    /// </summary>
    /// <param name="id">ID del gasto a eliminar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteExpense(string id)
    {
        _logger.LogInfo($"Controlador: DELETE api/v1/expenses/{id} - Eliminando gasto");
        
        try
        {
            var result = await _expenseService.DeleteExpenseAsync(id);
            
            if (!result)
            {
                _logger.LogWarn($"Controlador: No se encontró el gasto con ID: {id} para eliminar");
                return NotFound(new ExpenseResponse { Success = false, Status = "Not Found", Message = "Gasto no encontrado" });
            }
            
            _logger.LogInfo($"Controlador: Gasto eliminado correctamente con ID: {id}");
            return Ok(new ExpenseResponse { Message = "Gasto eliminado correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Controlador: Error al eliminar gasto con ID: {id}");
            return StatusCode(500, new ExpenseResponse { Success = false, Status = "Error", Message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Obtiene gastos de forma paginada
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <returns>Lista paginada de gastos</returns>
    [HttpGet("paginated")]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedExpenses([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        _logger.LogInfo($"Controlador: GET api/v1/expenses/paginated - Obteniendo gastos paginados. Página: {page}, Tamaño: {pageSize}");
        
        try
        {
            // Validar parámetros de paginación
            if (page < 1)
            {
                _logger.LogWarn($"Controlador: Página inválida: {page}, se usará página 1");
                page = 1;
            }
            
            if (pageSize < 1)
            {
                _logger.LogWarn($"Controlador: Tamaño de página inválido: {pageSize}, se usará tamaño 50");
                pageSize = 50;
            }
            
            // Obtener datos paginados
            var (expenses, totalCount) = await _expenseService.GetPaginatedExpensesAsync(page, pageSize);
            
            // Mapear a DTOs
            var expenseDtos = _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
            
            // Calcular información de paginación
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var paginationInfo = new PaginationDto
            {
                TotalItems = totalCount,
                ItemsPerPage = pageSize,
                CurrentPage = page,
                TotalPages = totalPages
            };
            
            // Crear respuesta paginada
            var paginatedResponse = new PaginatedResponseDto<IEnumerable<ExpenseDto>>(expenseDtos, paginationInfo);
            
            // Envolver en ApiResponse
            var response = new ExpenseResponse(paginatedResponse);
            
            _logger.LogInfo($"Controlador: Se obtuvieron {expenseDtos.Count()} gastos de un total de {totalCount}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Controlador: Error al obtener gastos paginados");
            return StatusCode(500, new ExpenseResponse { Success = false, Status = "Error", Message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Ajusta el monto de un gasto
    /// </summary>
    /// <param name="id">ID del gasto</param>
    /// <param name="dto">DTO con el monto ajustado</param>
    /// <returns>Gasto actualizado</returns>
    [HttpPatch("{id}/adjust-amount")]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdjustExpenseAmount(string id, [FromBody] AdjustExpenseAmountDto dto)
    {
        _logger.LogInfo($"Controlador: PATCH api/v1/expenses/{id}/adjust-amount - Ajustando monto de gasto");
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarn("Controlador: Modelo inválido al ajustar monto de gasto");
            return BadRequest(new ExpenseResponse { Success = false, Status = "Bad Request", Message = "Datos inválidos", Data = ModelState });
        }
        
        try
        {
            dto.Id = id; // Asegurar que el ID en el DTO coincida con el de la URL
            _logger.LogDebug($"Controlador: Ajustando monto de gasto con ID: {id}, Nuevo monto: {dto.AdjustedAmount}");
            
            var expense = await _expenseService.AdjustExpenseAmountAsync(id, dto);
            var expenseDto = _mapper.Map<ExpenseDto>(expense);
            var response = new ExpenseResponse(expenseDto, "Monto ajustado correctamente");
            
            _logger.LogInfo($"Controlador: Monto de gasto ajustado correctamente con ID: {id}");
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarn($"Controlador: No se encontró el gasto con ID: {id} para ajustar monto: {ex.Message}");
            return NotFound(new ExpenseResponse { Success = false, Status = "Not Found", Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Controlador: Error al ajustar monto de gasto con ID: {id}");
            return StatusCode(500, new ExpenseResponse { Success = false, Status = "Error", Message = $"Error al ajustar el monto: {ex.Message}" });
        }
    }
}
