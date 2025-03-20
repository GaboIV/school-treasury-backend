using Application.Interfaces;
using Domain.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IMongoCollection<Student> _studentCollection;

        public StudentRepository(IMongoDatabase database)
        {
            _studentCollection = database.GetCollection<Student>("Students");
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            var collation = new Collation("en", strength: CollationStrength.Primary);
            var options = new AggregateOptions()
            {
                Collation = collation
            };

            var students = await _studentCollection
                .Aggregate(options)
                .Match(Builders<Student>.Filter.Eq(s => s.Status, true))
                .Sort(Builders<Student>.Sort.Ascending(s => s.Name))
                .ToListAsync();

            return students;
        }

        public async Task<Student> GetByIdAsync(string id)
        {
            return await _studentCollection.Find(student => student.Id == id && student.Status == true).FirstOrDefaultAsync();
        }

        public async Task<Student> CreateAsync(Student student)
        {
            student.Status = true;
            student.CreatedAt = DateTime.UtcNow;
            student.UpdatedAt = DateTime.UtcNow;
            
            await _studentCollection.InsertOneAsync(student);
            return student;
        }

        public async Task UpdateAsync(Student student)
        {
            student.UpdatedAt = DateTime.UtcNow;
            
            await _studentCollection.ReplaceOneAsync(
                s => s.Id == student.Id,
                student);
        }

        public async Task DeleteAsync(string id)
        {
            var student = await GetByIdAsync(id);
            if (student != null)
            {
                student.Status = false;
                student.DeletedAt = DateTime.UtcNow;
                await UpdateAsync(student);
            }
        }

        public async Task<int> CountAsync(Expression<Func<Student, bool>> filter)
        {
            return (int)await _studentCollection.CountDocumentsAsync(filter);
        }
    }
} 