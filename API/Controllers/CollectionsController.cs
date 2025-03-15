namespace Presentation.Controllers;

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
    private readonly ILoggerManager _logger;

    public CollectionsController(ICollectionService collectionService, IMapper mapper, ILoggerManager logger)
    {
        _collectionService = collectionService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CollectionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCollections()
    {
        _logger.LogInfo("Endpoint: GET api/v1/collections - Obteniendo todas las colecciones");
        try
        {
            var collections = await _collectionService.GetAllCollectionsAsync();
            var collectionDtos = _mapper.Map<IEnumerable<CollectionDto>>(collections);
            var response = new ApiResponse<IEnumerable<CollectionDto>>(collectionDtos);
            _logger.LogInfo($"Se obtuvieron {collectionDtos.Count()} colecciones correctamente");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las colecciones");
            return StatusCode(500, new ApiResponse<string>("Error interno del servidor", "Error", false));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CollectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
    {
        _logger.LogInfo($"Endpoint: POST api/v1/collections - Creando nueva colección: {dto.Name}");
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarn("Modelo inválido al crear colección");
            return BadRequest(ModelState);
        }

        try
        {
            var collection = await _collectionService.CreateCollectionAsync(dto);
            var collectionDto = _mapper.Map<CollectionDto>(collection);
            var response = new ApiResponse<CollectionDto>(collectionDto);
            _logger.LogInfo($"Colección creada correctamente con ID: {collection.Id}");
            return CreatedAtAction(nameof(GetAllCollections), new { id = collection.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear colección");
            return StatusCode(500, new ApiResponse<string>("Error interno del servidor", "Error", false));
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<CollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCollection([FromBody] UpdateCollectionDto dto)
    {
        _logger.LogInfo($"Endpoint: PUT api/v1/collections - Actualizando colección con ID: {dto.Id}");
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarn("Modelo inválido al actualizar colección");
            return BadRequest(ModelState);
        }

        try
        {
            var collection = await _collectionService.UpdateCollectionAsync(dto);
            
            if (collection == null)
            {
                _logger.LogWarn($"No se encontró la colección con ID: {dto.Id} para actualizar");
                return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));
            }

            var collectionDto = _mapper.Map<CollectionDto>(collection);
            var response = new ApiResponse<CollectionDto>(collectionDto);
            _logger.LogInfo($"Colección actualizada correctamente con ID: {collection.Id}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al actualizar colección con ID: {dto.Id}");
            return StatusCode(500, new ApiResponse<string>("Error interno del servidor", "Error", false));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCollection(string id)
    {
        _logger.LogInfo($"Endpoint: DELETE api/v1/collections/{id} - Eliminando colección");
        
        if (string.IsNullOrEmpty(id))
        {
            _logger.LogWarn("ID vacío al intentar eliminar colección");
            return BadRequest(new ApiResponse<string>("El ID no puede estar vacío", "Bad Request", false));
        }

        try
        {
            var result = await _collectionService.DeleteCollectionAsync(id);
            
            if (!result)
            {
                _logger.LogWarn($"No se encontró la colección con ID: {id} para eliminar");
                return NotFound(new ApiResponse<string>("Tipo de gasto no encontrado", "Not Found", false));
            }

            _logger.LogInfo($"Colección eliminada correctamente con ID: {id}");
            return Ok(new ApiResponse<string>("Tipo de gasto eliminado correctamente", "Success", true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar colección con ID: {id}");
            return StatusCode(500, new ApiResponse<string>("Error interno del servidor", "Error", false));
        }
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponseDto<IEnumerable<CollectionDto>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaginatedCollections([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        _logger.LogInfo($"Endpoint: GET api/v1/collections/paginated - Obteniendo colecciones paginadas. Página: {page}, Tamaño: {pageSize}");
        
        try
        {
            // Validar parámetros de paginación
            if (page < 1)
            {
                _logger.LogWarn($"Página inválida: {page}, se usará página 1");
                page = 1;
            }
            
            if (pageSize < 1)
            {
                _logger.LogWarn($"Tamaño de página inválido: {pageSize}, se usará tamaño 50");
                pageSize = 50;
            }
            
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
            
            _logger.LogInfo($"Se obtuvieron {paginatedResponse.Items.Count()} colecciones de un total de {totalCount}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener colecciones paginadas");
            return StatusCode(500, new ApiResponse<string>("Error interno del servidor", "Error", false));
        }
    }

    [HttpPatch("{id}/adjust-amount")]
    [ProducesResponseType(typeof(ApiResponse<CollectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ValidationProblemDetails>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AdjustCollectionAmount(string id, [FromBody] AdjustCollectionAmountDto dto)
    {
        _logger.LogInfo($"Endpoint: PATCH api/v1/collections/{id}/adjust-amount - Ajustando monto de colección");
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarn("Modelo inválido al ajustar monto de colección");
            return BadRequest(ModelState);
        }

        try
        {
            dto.Id = id; // Asegurar que el ID en el DTO coincida con el de la URL
            _logger.LogDebug($"Ajustando monto de colección con ID: {id}, Nuevo monto: {dto.AdjustedAmount}");
            
            var collection = await _collectionService.AdjustCollectionAmountAsync(id, dto);
            var collectionDto = _mapper.Map<CollectionDto>(collection);
            var response = new ApiResponse<CollectionDto>(collectionDto, "Monto ajustado correctamente");
            
            _logger.LogInfo($"Monto de colección ajustado correctamente con ID: {id}");
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarn($"No se encontró la colección con ID: {id} para ajustar monto: {ex.Message}");
            return NotFound(new ApiResponse<string>(ex.Message, "Not Found", false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al ajustar monto de colección con ID: {id}");
            return StatusCode(500, new ApiResponse<string>($"Error al ajustar el monto: {ex.Message}", "Error", false));
        }
    }
} 