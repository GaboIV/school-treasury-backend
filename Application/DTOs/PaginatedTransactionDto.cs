using System;
using System.Collections.Generic;

namespace Application.DTOs
{
    public class PaginatedTransactionDto
    {
        public List<TransactionDto> Items { get; set; } = new List<TransactionDto>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
} 