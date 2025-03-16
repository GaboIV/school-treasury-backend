using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILoggerManager _logger;

        public StudentService(IStudentRepository studentRepository, ILoggerManager logger)
        {
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<StudentDto>> GetAllAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            _logger.LogInfo("Obteniendo todos los estudiantes");
            
            try
            {
                var students = await _studentRepository.GetAllAsync();
                var studentDtos = new List<StudentDto>();
                
                foreach (var student in students)
                {
                    studentDtos.Add(new StudentDto
                    {
                        Id = student.Id,
                        Name = student.Name,
                        Avatar = string.IsNullOrEmpty(student.Avatar) ? "001-boy.svg" : student.Avatar,
                        Status = student.Status,
                        CreatedAt = student.CreatedAt,
                        UpdatedAt = student.UpdatedAt,
                        DeletedAt = student.DeletedAt
                    });
                }
                
                stopwatch.Stop();
                _logger.LogInfo($"Estudiantes obtenidos exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return studentDtos;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Error al obtener estudiantes. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }

        public async Task<StudentDto> GetByIdAsync(string id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            _logger.LogInfo($"Obteniendo estudiante con ID: {id}");
            
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);
                
                if (student == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Estudiante con ID: {id} no encontrado. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return null;
                }
                
                var studentDto = new StudentDto
                {
                    Id = student.Id,
                    Name = student.Name,
                    Avatar = string.IsNullOrEmpty(student.Avatar) ? "001-boy.svg" : student.Avatar,
                    Status = student.Status,
                    CreatedAt = student.CreatedAt,
                    UpdatedAt = student.UpdatedAt,
                    DeletedAt = student.DeletedAt
                };
                
                stopwatch.Stop();
                _logger.LogInfo($"Estudiante con ID: {id} obtenido exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return studentDto;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Error al obtener estudiante con ID: {id}. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }

        public async Task<StudentDto> CreateAsync(CreateStudentDto createStudentDto)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            _logger.LogInfo("Creando nuevo estudiante");
            
            try
            {
                var student = new Student
                {
                    Name = createStudentDto.Name,
                    Avatar = string.IsNullOrEmpty(createStudentDto.Avatar) ? "001-boy.svg" : createStudentDto.Avatar
                };
                
                var createdStudent = await _studentRepository.CreateAsync(student);
                
                var studentDto = new StudentDto
                {
                    Id = createdStudent.Id,
                    Name = createdStudent.Name,
                    Avatar = string.IsNullOrEmpty(createdStudent.Avatar) ? "001-boy.svg" : createdStudent.Avatar,
                    Status = createdStudent.Status,
                    CreatedAt = createdStudent.CreatedAt,
                    UpdatedAt = createdStudent.UpdatedAt,
                    DeletedAt = createdStudent.DeletedAt
                };
                
                stopwatch.Stop();
                _logger.LogInfo($"Estudiante creado exitosamente. ID: {studentDto.Id}. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return studentDto;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Error al crear estudiante. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }

        public async Task<StudentDto> UpdateAsync(string id, UpdateStudentDto updateStudentDto)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            _logger.LogInfo($"Actualizando estudiante con ID: {id}");
            
            try
            {
                var existingStudent = await _studentRepository.GetByIdAsync(id);
                
                if (existingStudent == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Estudiante con ID: {id} no encontrado para actualizar. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return null;
                }
                
                existingStudent.Name = updateStudentDto.Name;
                
                if (!string.IsNullOrEmpty(updateStudentDto.Avatar))
                {
                    existingStudent.Avatar = updateStudentDto.Avatar;
                }
                
                await _studentRepository.UpdateAsync(existingStudent);
                
                var studentDto = new StudentDto
                {
                    Id = existingStudent.Id,
                    Name = existingStudent.Name,
                    Avatar = string.IsNullOrEmpty(existingStudent.Avatar) ? "001-boy.svg" : existingStudent.Avatar,
                    Status = existingStudent.Status,
                    CreatedAt = existingStudent.CreatedAt,
                    UpdatedAt = existingStudent.UpdatedAt,
                    DeletedAt = existingStudent.DeletedAt
                };
                
                stopwatch.Stop();
                _logger.LogInfo($"Estudiante con ID: {id} actualizado exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                
                return studentDto;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Error al actualizar estudiante con ID: {id}. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }

        public async Task DeleteAsync(string id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            _logger.LogInfo($"Eliminando estudiante con ID: {id}");
            
            try
            {
                var existingStudent = await _studentRepository.GetByIdAsync(id);
                
                if (existingStudent == null)
                {
                    stopwatch.Stop();
                    _logger.LogWarn($"Estudiante con ID: {id} no encontrado para eliminar. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                    return;
                }
                
                await _studentRepository.DeleteAsync(id);
                
                stopwatch.Stop();
                _logger.LogInfo($"Estudiante con ID: {id} eliminado exitosamente. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Error al eliminar estudiante con ID: {id}. Tiempo: {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }
    }
} 