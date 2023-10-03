using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = null!;
    [Required]
    [MaxLength(128)]
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsSoftDeleted { get; set; } = false;
    public bool IsPublished { get; set; } = false;
    public DateTime PublishDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }

    public ICollection<PostTag> PostTags { get; set; }
    public ICollection<PostCategory> PostCategories { get; set; } 


}