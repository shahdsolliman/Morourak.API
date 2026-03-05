using Morourak.Domain.Enums.Auth;
using System.ComponentModel.DataAnnotations;

namespace Morourak.Application.DTOs.Admin;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    [Required]
    public string FirstName { get; set; } = null!;
    
    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}

public class UserFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; } // e.g., "CreatedAt", "Email"
    public bool IsDescending { get; set; } = true;
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
