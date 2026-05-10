namespace Morourak.API.DTOs.User
{
    public class CitizenProfileDto
    {
        public string FullName { get; set; } = null!;
        public string NationalId { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
