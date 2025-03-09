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
}
