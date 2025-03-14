namespace Presentation.Controllers;

using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/collection-types")]
public class CollectionTypesController : ControllerBase
{
    private readonly ICollectionTypeService _expensiveTypeService;
    private readonly IMapper _mapper;

    public CollectionTypesController(ICollectionTypeService expensiveTypeService, IMapper mapper)
    {
        _expensiveTypeService = expensiveTypeService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CollectionTypeDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCollectionTypes()
    {
        var expensiveTypes = await _expensiveTypeService.GetAllCollectionTypesAsync();
        var expensiveTypeDtos = _mapper.Map<IEnumerable<CollectionTypeDto>>(expensiveTypes);
        var response = new ApiResponse<IEnumerable<CollectionTypeDto>>(expensiveTypeDtos);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CollectionTypeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCollectionType([FromBody] CreateCollectionTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var expensiveType = await _expensiveTypeService.CreateCollectionTypeAsync(dto);
        var expensiveTypeDto = _mapper.Map<CollectionTypeDto>(expensiveType);
        var response = new ApiResponse<CollectionTypeDto>(expensiveTypeDto);
        return CreatedAtAction(nameof(GetAllCollectionTypes), new { id = expensiveType.Id }, response);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<CollectionTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCollectionType([FromBody] UpdateCollectionTypeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var expensiveType = await _expensiveTypeService.UpdateCollectionTypeAsync(dto);
        
        if (expensiveType == null)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        var expensiveTypeDto = _mapper.Map<CollectionTypeDto>(expensiveType);
        var response = new ApiResponse<CollectionTypeDto>(expensiveTypeDto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCollectionType(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new ApiResponse<string>("El ID no puede estar vacío", "Bad Request", false));

        // Verificar si existen gastos asociados a este tipo de gasto
        var existsCollections = await _expensiveTypeService.ExistsCollectionWithTypeIdAsync(id);
        if (existsCollections)
            return BadRequest(new ApiResponse<string>("No se puede eliminar el tipo de gasto porque existen gastos asociados", "Bad Request", false));

        var result = await _expensiveTypeService.DeleteCollectionTypeAsync(id);
        
        if (!result)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        return Ok(new ApiResponse<string>("Tipo de gasto eliminado correctamente", "Success", true));
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<IEnumerable<CollectionTypeDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedCollectionTypes([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        // Validar parámetros de paginación
        if (page < 1)
            page = 1;
        
        if (pageSize < 1)
            pageSize = 50;
        
        // Obtener datos paginados
        var (collectionTypes, totalCount) = await _expensiveTypeService.GetPaginatedCollectionTypesAsync(page, pageSize);
        
        // Mapear a DTOs
        var collectionTypeDtos = _mapper.Map<IEnumerable<CollectionTypeDto>>(collectionTypes);
        
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
        var paginatedResponse = new PaginatedResponseDto<IEnumerable<CollectionTypeDto>>(collectionTypeDtos, paginationInfo);
        
        // Envolver en ApiResponse
        var response = new ApiResponse<PaginatedResponseDto<IEnumerable<CollectionTypeDto>>>(paginatedResponse);
        
        return Ok(response);
    }
}
