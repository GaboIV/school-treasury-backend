using Domain.Entities;
using Infrastructure.Persistence;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seeders
{
    public class StudentSeeder : ISeeder
    {
        private readonly IMongoCollection<Student> _studentCollection;

        public StudentSeeder(MongoDbContext context)
        {
            _studentCollection = context.Students;
        }

        public async Task SeedAsync()
        {
            // Verificar si ya existen estudiantes en la colección
            var studentsCount = await _studentCollection.CountDocumentsAsync(Builders<Student>.Filter.Empty);
            
            if (studentsCount > 0)
            {
                // Ya existen estudiantes, no es necesario sembrar
                return;
            }

            // Lista de nombres de estudiantes
            var studentNames = new List<string>
            {
                "Alonso",
                "Santino",
                "Elaine",
                "Ángel",
                "Andrick",
                "Hanna",
                "Mathías",
                "Lara",
                "Liam",
                "Danna",
                "Fabiana",
                "Isabel",
                "Yosué",
                "Matías",
                "Flavia",
                "Emma",
                "Demir",
                "Leonardo",
                "Ryan",
                "Lucas",
                "Emilia",
                "Nicolás",
                "Dareck",
                "Niño X",
                "Niño Y"
            };

            // Crear entidades de estudiantes
            var students = new List<Student>();
            
            foreach (var name in studentNames)
            {
                students.Add(new Student
                {
                    Name = name,
                    Status = true
                });
            }

            // Insertar estudiantes en la base de datos
            await _studentCollection.InsertManyAsync(students);
        }
    }
} 