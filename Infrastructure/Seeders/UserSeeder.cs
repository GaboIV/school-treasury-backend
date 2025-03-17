using Domain.Entities;
using Domain.Enums;
using MongoDB.Driver;
using BC = BCrypt.Net.BCrypt;

namespace Infrastructure.Seeders
{
    public class UserSeeder
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Student> _students;
        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(IMongoDatabase database, ILogger<UserSeeder> logger)
        {
            _users = database.GetCollection<User>("Users");
            _students = database.GetCollection<Student>("Students");
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await EnsureAdminExists();
            await EnsureStudentsHaveUsers();
        }

        private async Task EnsureAdminExists()
        {
            var adminExists = await _users.Find(u => u.Role == UserRole.Administrator && u.Username == "admin").AnyAsync();

            if (!adminExists)
            {
                _logger.LogInformation("Creando usuario administrador por defecto...");

                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = BC.HashPassword("admin123"),
                    Email = "admin@schooltreasury.com",
                    FullName = "Administrador",
                    Role = UserRole.Administrator,
                    IsActive = true
                };

                await _users.InsertOneAsync(admin);
                _logger.LogInformation("Usuario administrador creado exitosamente.");
            }
        }

        private async Task EnsureStudentsHaveUsers()
        {
            var students = await _students.Find(s => true).ToListAsync();
            var createdUsers = new List<User>();

            foreach (var student in students)
            {
                var userExists = await _users.Find(u => u.StudentId == student.Id).AnyAsync();

                if (!userExists)
                {
                    var newUser = new User
                    {
                        Username = student.Name,
                        PasswordHash = BC.HashPassword(student.Name),
                        Email = "",
                        FullName = $"Representante de {student.Name}",
                        Role = UserRole.Representative,
                        StudentId = student.Id!,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    createdUsers.Add(newUser);
                }
            }

            if (createdUsers.Count > 0)
            {
                await _users.InsertManyAsync(createdUsers);
                _logger.LogInformation($"Se crearon {createdUsers.Count} usuarios para estudiantes sin cuenta.");
            }
            else
            {
                _logger.LogInformation("Todos los estudiantes ya tienen un usuario.");
            }
        }
    }
}
