
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class UserEntity : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Full name (optional: used for display)
        public string FullName { get; set; }

        public string Gender { get; set; }
        public string Address { get; set; }

        public DateTime DateRegistered { get; set; }

        // Path to the uploaded profile image
        public string? PhotoUrl { get; set; }

        // Flag to indicate if agent is approved
        public bool IsApproved { get; set; } = false;
        public string? ProfilePhotoUrl { get; set; }

    }
}
