using System.Threading.Tasks;

namespace Infrastructure.Persistence.Seeders
{
    public interface ISeeder
    {
        Task SeedAsync();
    }
} 