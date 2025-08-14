using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        // User chooses password
        public string Password { get; set; }

        // Confirm password (must match Password)
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }

        // New: Profile image upload
        public IFormFile? ProfilePhoto { get; set; }
    }
}
