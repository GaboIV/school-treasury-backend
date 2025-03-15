namespace API.Controllers;

using AutoMapper;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/collections")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    private readonly IMapper _mapper;

    public CollectionsController(ICollectionService collectionService, IMapper mapper)
    {
        _collectionService = collectionService;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CollectionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCollections()
    {
        var collections = await _collectionService.GetAllCollectionsAsync();
        var collectionDtos = _mapper.Map<IEnumerable<CollectionDto>>(collections);
        var response = new ApiResponse<IEnumerable<CollectionDto>>(collectionDtos);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CollectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var collection = await _collectionService.CreateCollectionAsync(dto);
        var collectionDto = _mapper.Map<CollectionDto>(collection);
        var response = new ApiResponse<CollectionDto>(collectionDto);
        return CreatedAtAction(nameof(GetAllCollections), new { id = collection.Id }, response);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<CollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCollection([FromBody] UpdateCollectionDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var collection = await _collectionService.UpdateCollectionAsync(dto);
        
        if (collection == null)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        var collectionDto = _mapper.Map<CollectionDto>(collection);
        var response = new ApiResponse<CollectionDto>(collectionDto);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCollection(string id)
    {
        if (string.IsNullOrEmpty(id))
            return BadRequest(new ApiResponse<string>("El ID no puede estar vacío", "Bad Request", false));

        var result = await _collectionService.DeleteCollectionAsync(id);
        
        if (!result)
            return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));

        return Ok(new ApiResponse<string>("Tipo de gasto eliminado correctamente", "Success", true));
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<IEnumerable<CollectionDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedCollections([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        // Validar parámetros de paginación
        if (page < 1)
            page = 1;
        
        if (pageSize < 1)
            pageSize = 50;
        
        // Obtener datos paginados
        var (collections, totalCount) = await _collectionService.GetPaginatedCollectionsAsync(page, pageSize);
        
        // Mapear a DTOs
        var collectionDtos = _mapper.Map<IEnumerable<CollectionDto>>(collections);
        
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
        var paginatedResponse = new PaginatedResponseDto<IEnumerable<CollectionDto>>(collectionDtos, paginationInfo);
        
        // Envolver en ApiResponse
        var response = new ApiResponse<PaginatedResponseDto<IEnumerable<CollectionDto>>>(paginatedResponse);
        
        return Ok(response);
    }

    [HttpPatch("{id}/adjust-amount")]
    [ProducesResponseType(typeof(ApiResponse<CollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdjustCollectionAmount(string id, [FromBody] AdjustCollectionAmountDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            dto.Id = id; // Asegurar que el ID en el DTO coincida con el de la URL
            var collection = await _collectionService.AdjustCollectionAmountAsync(id, dto);
            var collectionDto = _mapper.Map<CollectionDto>(collection);
            var response = new ApiResponse<CollectionDto>(collectionDto, "Monto ajustado correctamente");
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<string>(ex.Message, "Not Found", false));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>($"Error al ajustar el monto: {ex.Message}", "Error", false));
        }
    }
}
