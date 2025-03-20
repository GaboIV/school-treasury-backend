using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student> GetByIdAsync(string id);
        Task<Student> CreateAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(string id);
        Task<int> CountAsync(Expression<Func<Student, bool>> filter);
    }
} 