using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class PostReadDto
{
    public PostReadDto(Guid id, string title, string slug, string body, bool isSoftDeleted)
    {
        Id = id;
        Title = title;
        Slug = slug;
        Body = body;
        IsSoftDeleted = isSoftDeleted;
    }

    public Guid Id { get; }
    public string Title { get; }
    public string Slug { get; }
    public string Body { get; }
    public bool IsSoftDeleted { get; }
}

public class PostUpsertDto
{
    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = null!;

    [Required]
    public string Body { get; set; } = null!;
}