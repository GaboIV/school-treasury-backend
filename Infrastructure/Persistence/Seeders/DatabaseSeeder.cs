using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seeders
{
    public class DatabaseSeeder
    {
        private readonly IEnumerable<ISeeder> _seeders;

        public DatabaseSeeder(IEnumerable<ISeeder> seeders)
        {
            _seeders = seeders;
        }

        public async Task SeedAllAsync()
        {
            foreach (var seeder in _seeders)
            {
                await seeder.SeedAsync();
            }
        }
    }
} 