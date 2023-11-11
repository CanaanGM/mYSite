using Domain.Entities;

using Microsoft.AspNetCore.Identity;

namespace DataAccess.Entities;

public class User : IdentityUser
{
    // public string DisplayName { get; set; } = null!;
    public string? ProfilePicture { get; set; }
    public List<Comment>? Comments { get; set; }

    // likes, fav and dislikes
}