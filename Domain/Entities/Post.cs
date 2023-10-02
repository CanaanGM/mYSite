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
    public string Body { get; set; } = null!;
    public bool IsSoftDeleted { get; set; } = false;
}