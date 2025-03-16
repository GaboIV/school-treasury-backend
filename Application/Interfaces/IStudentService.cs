using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentDto>> GetAllAsync();
        Task<StudentDto> GetByIdAsync(string id);
        Task<StudentDto> CreateAsync(CreateStudentDto createStudentDto);
        Task<StudentDto> UpdateAsync(string id, UpdateStudentDto updateStudentDto);
        Task DeleteAsync(string id);
    }
} 