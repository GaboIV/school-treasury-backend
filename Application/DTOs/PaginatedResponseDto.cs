namespace Application.DTOs
{
    public class PaginatedResponseDto<T>
    {
        public T Items { get; set; }
        public PaginationDto Pagination { get; set; }

        public PaginatedResponseDto(T items, PaginationDto pagination)
        {
            Items = items;
            Pagination = pagination;
        }
    }
} 