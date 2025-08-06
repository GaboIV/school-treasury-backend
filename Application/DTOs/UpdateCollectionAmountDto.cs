using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateCollectionAmountDto
    {
        [Required(ErrorMessage = "El monto total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto total debe ser mayor a 0")]
        public decimal TotalAmount { get; set; }
    }
}