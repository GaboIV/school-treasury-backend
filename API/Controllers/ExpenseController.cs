using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

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
                Status = status
            };
            
            // Guardar las imágenes y obtener sus IDs
            if (images != null && images.Length > 0)
            {
                // Generar un ID único para este gasto y guardar las imágenes en su propia carpeta
                var tempExpenseId = Guid.NewGuid().ToString();
                var imagePaths = await _fileService.SaveImagesAsync(images.ToList(), "expenses", tempExpenseId);
                
                // Ahora, imagePaths ya contiene las rutas completas
                _logger.LogInfo($"Controlador: Se guardaron {imagePaths.Count} imágenes para el nuevo gasto");
                
                // Usar las rutas completas como IDs
                dto.ImageIds = imagePaths;
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
                Status = status
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
                // Usar las rutas existentes tal como están
                imageIds.AddRange(existingImageIds);
                _logger.LogInfo($"Controlador: Se mantienen {existingImageIds.Count} imágenes existentes");
            }
            
            // Procesar nuevas imágenes
            if (images != null && images.Length > 0)
            {
                // Guardar nuevas imágenes en la carpeta del gasto
                var newImagePaths = await _fileService.SaveImagesAsync(images.ToList(), "expenses", id);
                imageIds.AddRange(newImagePaths);
                _logger.LogInfo($"Controlador: Se agregaron {newImagePaths.Count} imágenes nuevas");
            }
            
            dto.ImageIds = imageIds;
            
            var expense = await _expenseService.UpdateExpenseAsync(dto);
            
            if (expense == null)
            {
                _logger.LogWarn($"Controlador: No se encontró el gasto con ID: {dto.Id}");
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
            _logger.LogError(ex, $"Controlador: Error al actualizar gasto");
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
    /// <returns>Respuesta de éxito</returns>
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
                _logger.LogWarn($"Controlador: No se encontró el gasto con ID: {id}");
                return NotFound(new ExpenseResponse { 
                    Success = false, 
                    Status = "Not Found", 
                    Message = "Gasto no encontrado" 
                });
            }
            
            _logger.LogInfo($"Controlador: Gasto eliminado correctamente con ID: {id}");
            return Ok(new ExpenseResponse { 
                Success = true, 
                Status = "Success", 
                Message = "Gasto eliminado correctamente" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Controlador: Error al eliminar gasto con ID: {id}");
            return StatusCode(500, new ExpenseResponse { 
                Success = false, 
                Status = "Error", 
                Message = "Error interno del servidor" 
            });
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
            var (expenses, totalCount) = await _expenseService.GetPaginatedExpensesAsync(page, pageSize);
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
            var response = new ExpenseResponse
            {
                Success = true,
                Status = "Success",
                Message = "Gastos obtenidos correctamente",
                Data = new
                {
                    Items = expenseDtos,
                    Pagination = paginationInfo
                }
            };
            
            _logger.LogInfo($"Controlador: Se obtuvieron {expenseDtos.Count()} gastos paginados correctamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Controlador: Error al obtener gastos paginados");
            return StatusCode(500, new ExpenseResponse { 
                Success = false, 
                Status = "Error", 
                Message = "Error interno del servidor" 
            });
        }
    }
}
