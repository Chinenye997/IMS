
namespace Application.DTOs
{
    public class UserRequest
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateRegistered { get; set; }
        public string UserName { get; set; }
    }

    public class UserResponse : UserRequest
    {
        public string? ProfilePhotoUrl { get; set; }     // expose to views
        public bool IsApproved { get; set; }             // expose approval status
        public string Role { get; set; }

    }
}
