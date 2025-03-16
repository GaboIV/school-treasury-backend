using Application.DTOs;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
        Task<PendingPaymentsDto> GetPendingPaymentsAsync();
        Task<StudentsInfoDto> GetStudentsInfoAsync();
        Task<PettyCashSummaryDto> GetPettyCashSummaryAsync();
        Task<TopPendingCollectionsDto> GetTopPendingCollectionsAsync();
    }
} 