namespace Presentation.Controllers;

using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/expense-types")]
public class ExpenseTypesController : ControllerBase
{
    private readonly IExpenseTypeService _expensiveTypeService;
    private readonly IMapper _mapper;

    public ExpenseTypesController(IExpenseTypeService expensiveTypeService, IMapper mapper)
    {
        _expensiveTypeService = expensiveTypeService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllExpenseTypes()
    {
        var expensiveTypes = await _expensiveTypeService.GetAllExpenseTypesAsync();
        var expensiveTypeDtos = _mapper.Map<IEnumerable<ExpenseTypeDto>>(expensiveTypes);
        var response = new ApiResponse<IEnumerable<ExpenseTypeDto>>(expensiveTypeDtos);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseTypeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateExpenseType([FromBody] CreateExpenseTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var expensiveType = await _expensiveTypeService.CreateExpenseTypeAsync(dto);
        var expensiveTypeDto = _mapper.Map<ExpenseTypeDto>(expensiveType);
        var response = new ApiResponse<ExpenseTypeDto>(expensiveTypeDto);
        return CreatedAtAction(nameof(GetAllExpenseTypes), new { id = expensiveType.Id }, response);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<ExpenseTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateExpenseType([FromBody] UpdateExpenseTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var expensiveType = await _expensiveTypeService.UpdateExpenseTypeAsync(dto);
        
        if (expensiveType == null)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        var expensiveTypeDto = _mapper.Map<ExpenseTypeDto>(expensiveType);
        var response = new ApiResponse<ExpenseTypeDto>(expensiveTypeDto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteExpenseType(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new ApiResponse<string>("El ID no puede estar vacío", "Bad Request", false));

        // Verificar si existen gastos asociados a este tipo de gasto
        var existsExpenses = await _expensiveTypeService.ExistsExpenseWithTypeIdAsync(id);
        if (existsExpenses)
            return BadRequest(new ApiResponse<string>("No se puede eliminar el tipo de gasto porque existen gastos asociados", "Bad Request", false));

        var result = await _expensiveTypeService.DeleteExpenseTypeAsync(id);
        
        if (!result)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        return Ok(new ApiResponse<string>("Tipo de gasto eliminado correctamente", "Success", true));
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<IEnumerable<ExpenseTypeDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedExpenseTypes([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        // Validar parámetros de paginación
        if (page < 1)
            page = 1;
        
        if (pageSize < 1)
            pageSize = 50;
        
        // Obtener datos paginados
        var (expenseTypes, totalCount) = await _expensiveTypeService.GetPaginatedExpenseTypesAsync(page, pageSize);
        
        // Mapear a DTOs
        var expenseTypeDtos = _mapper.Map<IEnumerable<ExpenseTypeDto>>(expenseTypes);
        
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
        var paginatedResponse = new PaginatedResponseDto<IEnumerable<ExpenseTypeDto>>(expenseTypeDtos, paginationInfo);
        
        // Envolver en ApiResponse
        var response = new ApiResponse<PaginatedResponseDto<IEnumerable<ExpenseTypeDto>>>(paginatedResponse);
        
        return Ok(response);
    }
}
