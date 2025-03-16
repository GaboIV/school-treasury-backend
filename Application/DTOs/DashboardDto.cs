using System.Collections.Generic;
using Domain.Entities;

namespace Application.DTOs
{
    public class DashboardDto
    {
        public PendingPaymentsDto PendingPayments { get; set; } = new PendingPaymentsDto();
        public StudentsInfoDto StudentsInfo { get; set; } = new StudentsInfoDto();
        public PettyCashSummaryDto PettyCashSummary { get; set; } = new PettyCashSummaryDto();
        public List<InterestLinkDto> InterestLinks { get; set; } = new List<InterestLinkDto>();
        public TopPendingCollectionsDto TopPendingCollections { get; set; } = new TopPendingCollectionsDto();
    }

    public class PendingPaymentsDto
    {
        public int TotalPendingPayments { get; set; }
        public int TotalPayments { get; set; }
        public decimal CompletionPercentage { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public List<PendingPaymentDetailDto> TopPendingPayments { get; set; } = new List<PendingPaymentDetailDto>();
        public int RemainingPendingPayments { get; set; }
    }

    public class PendingPaymentDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
        public decimal PendingAmount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }

    public class StudentsInfoDto
    {
        public int TotalStudents { get; set; }
        public List<StudentInitialDto> StudentInitials { get; set; } = new List<StudentInitialDto>();
    }

    public class StudentInitialDto
    {
        public string Id { get; set; } = string.Empty;
        public string Initial { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class PettyCashSummaryDto
    {
        public decimal CurrentBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Available { get; set; }
        public decimal PercentageChange { get; set; }
    }

    public class TopPendingCollectionsDto
    {
        public List<PendingCollectionDetailDto> TopCollections { get; set; } = new List<PendingCollectionDetailDto>();
        public int RemainingPendingCollections { get; set; }
    }

    public class PendingCollectionDetailDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CollectionTypeName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int TotalStudents { get; set; }
        public int PendingStudents { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
} 