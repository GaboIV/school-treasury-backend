using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterestLinkController : ControllerBase
    {
        private readonly IInterestLinkService _interestLinkService;
        private readonly ILoggerManager _logger;

        public InterestLinkController(IInterestLinkService interestLinkService, ILoggerManager logger)
        {
            _interestLinkService = interestLinkService ?? throw new ArgumentNullException(nameof(interestLinkService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InterestLinkDto>>> GetAll()
        {
            try
            {
                var interestLinks = await _interestLinkService.GetAllAsync();
                return Ok(interestLinks);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener links de interés: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al obtener links de interés", false));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InterestLinkDto>> GetById(string id)
        {
            try
            {
                var interestLink = await _interestLinkService.GetByIdAsync(id);
                if (interestLink == null)
                {
                    return NotFound(new ApiResponse<string>("", $"Link de interés con id {id} no encontrado", false));
                }
                return Ok(interestLink);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener link de interés: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al obtener link de interés", false));
            }
        }

        [HttpPost]
        public async Task<ActionResult<InterestLinkDto>> Create(CreateInterestLinkDto createInterestLinkDto)
        {
            try
            {
                var interestLink = await _interestLinkService.CreateAsync(createInterestLinkDto);
                return CreatedAtAction(nameof(GetById), new { id = interestLink.Id }, interestLink);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear link de interés: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al crear link de interés", false));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<InterestLinkDto>> Update(string id, UpdateInterestLinkDto updateInterestLinkDto)
        {
            try
            {
                var interestLink = await _interestLinkService.UpdateAsync(id, updateInterestLinkDto);
                return Ok(interestLink);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<string>("", $"Link de interés con id {id} no encontrado", false));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar link de interés: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al actualizar link de interés", false));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var result = await _interestLinkService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<string>("", $"Link de interés con id {id} no encontrado", false));
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar link de interés: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>("", "Error al eliminar link de interés", false));
            }
        }
    }
} 