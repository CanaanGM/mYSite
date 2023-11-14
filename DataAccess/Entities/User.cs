using Microsoft.AspNetCore.Identity;

namespace DataAccess.Entities;

public class User : IdentityUser
{
    // public string DisplayName { get; set; } = null!;
    public string? ProfilePicture { get; set; }
    public List<Comment>? Comments { get; set; }
    public List<Post> AuthoredPosts { get; set; } = null!;

    public List<UserFavoritePost> Favorites { get; set; }

}