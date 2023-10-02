namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Email { get; set; }  = null!;
    public string Password { get; set; } = null!;
}