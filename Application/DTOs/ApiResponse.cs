namespace Application.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(T data, string message = "Success", bool success = true)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}