namespace Presentation.Controllers;

using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly IMapper _mapper;

    public ExpensesController(IExpenseService expenseService, IMapper mapper)
    {
        _expenseService = expenseService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllExpenses()
    {
        var expenses = await _expenseService.GetAllExpensesAsync();
        var expenseDtos = _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
        var response = new ApiResponse<IEnumerable<ExpenseDto>>(expenseDtos);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var expense = await _expenseService.CreateExpenseAsync(dto);
        var expenseDto = _mapper.Map<ExpenseDto>(expense);
        var response = new ApiResponse<ExpenseDto>(expenseDto);
        return CreatedAtAction(nameof(GetAllExpenses), new { id = expense.Id }, response);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateExpense([FromBody] UpdateExpenseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var expense = await _expenseService.UpdateExpenseAsync(dto);
        
        if (expense == null)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        var expenseDto = _mapper.Map<ExpenseDto>(expense);
        var response = new ApiResponse<ExpenseDto>(expenseDto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteExpense(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new ApiResponse<string>("El ID no puede estar vacío", "Bad Request", false));

        var result = await _expenseService.DeleteExpenseAsync(id);
        
        if (!result)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        return Ok(new ApiResponse<string>("Tipo de gasto eliminado correctamente", "Success", true));
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<IEnumerable<ExpenseDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedExpenses([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        // Validar parámetros de paginación
        if (page < 1)
            page = 1;
        
        if (pageSize < 1)
            pageSize = 50;
        
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
        var response = new ApiResponse<PaginatedResponseDto<IEnumerable<ExpenseDto>>>(paginatedResponse);
        
        return Ok(response);
    }
}
