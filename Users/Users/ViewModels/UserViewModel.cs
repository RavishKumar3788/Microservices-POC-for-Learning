using System.ComponentModel.DataAnnotations;

namespace Users.ViewModels
{
    public class UserViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;
        
        [Required]
        [StringLength(200)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = null!;
    }
}
