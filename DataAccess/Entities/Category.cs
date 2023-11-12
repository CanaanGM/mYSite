using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities;

public class Category
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public ICollection<PostCategory>? CategoryPosts { get; set; }
}