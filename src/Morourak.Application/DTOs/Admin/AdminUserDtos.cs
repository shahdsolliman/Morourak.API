using Morourak.Domain.Enums.Auth;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Morourak.Application.Enums.Admin;

namespace Morourak.Application.DTOs.Admin;

/// <summary>
/// Basic information about a system user.
/// </summary>
    public class UserDto
    {
        public string Id { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? PasswordHint { get; set; }
        public string? NationalId { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

/// <summary>
/// Data required to create a new user.
/// </summary>
public class CreateUserDto
{
    /// <summary>
    /// User's first name.
    /// </summary>
    [Required]
    public string FirstName { get; set; } = null!;
    
    /// <summary>
    /// User's last name.
    /// </summary>
    [Required]
    public string LastName { get; set; } = null!;

    /// <summary>
    /// User's email address (must be unique).
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's security password.
    /// </summary>
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    [Required]
    [JsonRequired]
    public AppRole Role { get; set; }

    /// <summary>
    /// Initial status of the account.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Citizen's National ID (required if role is CITIZEN).
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// Citizen's Phone Number.
    /// </summary>
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Data required to update an existing user.
/// </summary>
public class UpdateUserDto
{
    /// <summary>
    /// Updated first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Updated last name.
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Updated email address.
    /// </summary>
    [EmailAddress]
    public string? Email { get; set; }
    
    /// <summary>
    /// Updated role.
    /// </summary>
    public AppRole? Role { get; set; }

    /// <summary>
    /// Updated account status.
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// Filter criteria for listing users.
/// </summary>
public class UserFilterDto
{
    /// <summary>
    /// Search term for filtering by name or email.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Filter by account activity status.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Property name to sort by (e.g., "CreatedAt", "Email").
    /// </summary>
    public UserSortField? SortBy { get; set; }

    /// <summary>
    /// Whether to sort in descending order.
    /// </summary>
    public bool IsDescending { get; set; } = true;
    
    /// <summary>
    /// The page number for pagination.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
