namespace Morourak.Domain.Entities;
public class EmailOtp
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Code { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }
}
