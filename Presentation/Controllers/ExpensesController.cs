namespace Presentation.Controllers;

using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expensiveService;
    private readonly IMapper _mapper;

    public ExpensesController(IExpenseService expensiveService, IMapper mapper)
    {
        _expensiveService = expensiveService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExpenseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllExpenses()
    {
        var expensives = await _expensiveService.GetAllExpensesAsync();
        var expensiveDtos = _mapper.Map<IEnumerable<ExpenseDto>>(expensives);
        var response = new ApiResponse<IEnumerable<ExpenseDto>>(expensiveDtos);
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

        var expensive = await _expensiveService.CreateExpenseAsync(dto);
        var expensiveDto = _mapper.Map<ExpenseDto>(expensive);
        var response = new ApiResponse<ExpenseDto>(expensiveDto);
        return CreatedAtAction(nameof(GetAllExpenses), new { id = expensive.Id }, response);
    }
}
