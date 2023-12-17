using Microsoft.AspNetCore.Identity;

namespace DataAccess.Entities;

public class User : IdentityUser
{
    // public string DisplayName { get; set; } = null!;
    public string? ProfilePicture { get; set; }

    public List<Comment>? Comments { get; set; }
    public List<Post> AuthoredPosts { get; set; } = null!;

    // TODO : soft and hard deletion, normal user and admin
    public List<UserFavoritePost> Favorites { get; set; }

    public ICollection<PostUserReaction> PostReactions { get; set; }
    public ICollection<CommentUserReaction> CommentReactions { get; set; }
}