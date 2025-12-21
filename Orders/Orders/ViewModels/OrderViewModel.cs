using System.ComponentModel.DataAnnotations;

namespace Orders.ViewModels
{
    public class OrderViewModel
    {
        [Required]
        [StringLength(100)]
        public string UserId { get; set; } = null!;


        [Required]
        [StringLength(100)]
        public string ProductId { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than 0")]
        public decimal ProductPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
