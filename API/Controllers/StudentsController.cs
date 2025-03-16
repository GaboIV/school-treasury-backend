using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILoggerManager _logger;

        public StudentsController(IStudentService studentService, ILoggerManager logger)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAll()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            try
            {
                _logger.LogInfo("Endpoint: GET api/Students - Obteniendo todos los estudiantes");
                var students = await _studentService.GetAllAsync();
                
                stopwatch.Stop();
                _logger.LogInfo($"Endpoint: GET api/Students - Estudiantes obtenidos exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");

                return Ok(new ApiResponse<object>(students, "Estudiantes obtenidos exitosamente"));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Endpoint: GET api/Students - Error al obtener estudiantes. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                return StatusCode(500, "Error interno del servidor al obtener estudiantes");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetById(string id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            try
            {
                _logger.LogInfo($"Endpoint: GET api/Students/{id} - Obteniendo estudiante por ID");
                var student = await _studentService.GetByIdAsync(id);
                
                if (student == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Endpoint: GET api/Students/{id} - Estudiante no encontrado. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return NotFound($"Estudiante con ID {id} no encontrado");
                }
                
                stopwatch.Stop();
                _logger.LogInfo($"Endpoint: GET api/Students/{id} - Estudiante obtenido exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return Ok(student);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Endpoint: GET api/Students/{id} - Error al obtener estudiante. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                return StatusCode(500, $"Error interno del servidor al obtener estudiante con ID {id}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StudentDto>> Create(CreateStudentDto createStudentDto)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            try
            {
                _logger.LogInfo("Endpoint: POST api/Students - Creando nuevo estudiante");
                
                if (!ModelState.IsValid)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Endpoint: POST api/Students - Datos de estudiante inválidos. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return BadRequest(ModelState);
                }
                
                var createdStudent = await _studentService.CreateAsync(createStudentDto);
                
                stopwatch.Stop();
                _logger.LogInfo($"Endpoint: POST api/Students - Estudiante creado exitosamente. ID: {createdStudent.Id}. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return CreatedAtAction(nameof(GetById), new { id = createdStudent.Id }, createdStudent);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Endpoint: POST api/Students - Error al crear estudiante. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                return StatusCode(500, "Error interno del servidor al crear estudiante");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StudentDto>> Update(string id, UpdateStudentDto updateStudentDto)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            try
            {
                _logger.LogInfo($"Endpoint: PUT api/Students/{id} - Actualizando estudiante");
                
                if (!ModelState.IsValid)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Endpoint: PUT api/Students/{id} - Datos de estudiante inválidos. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return BadRequest(ModelState);
                }
                
                var updatedStudent = await _studentService.UpdateAsync(id, updateStudentDto);
                
                if (updatedStudent == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Endpoint: PUT api/Students/{id} - Estudiante no encontrado. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return NotFound($"Estudiante con ID {id} no encontrado");
                }
                
                stopwatch.Stop();
                _logger.LogInfo($"Endpoint: PUT api/Students/{id} - Estudiante actualizado exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return Ok(updatedStudent);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Endpoint: PUT api/Students/{id} - Error al actualizar estudiante. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                return StatusCode(500, $"Error interno del servidor al actualizar estudiante con ID {id}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            try
            {
                _logger.LogInfo($"Endpoint: DELETE api/Students/{id} - Eliminando estudiante");
                
                var student = await _studentService.GetByIdAsync(id);
                
                if (student == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Endpoint: DELETE api/Students/{id} - Estudiante no encontrado. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return NotFound($"Estudiante con ID {id} no encontrado");
                }
                
                await _studentService.DeleteAsync(id);
                
                stopwatch.Stop();
                _logger.LogInfo($"Endpoint: DELETE api/Students/{id} - Estudiante eliminado exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Endpoint: DELETE api/Students/{id} - Error al eliminar estudiante. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                return StatusCode(500, $"Error interno del servidor al eliminar estudiante con ID {id}");
            }
        }
    }
}
