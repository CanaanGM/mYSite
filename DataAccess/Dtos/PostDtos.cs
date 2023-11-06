using System.ComponentModel.DataAnnotations;

namespace DataAccess.Dtos;

public class PostReadDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsSoftDeleted { get; set; }
    public ICollection<string> Tags { get; set; }
    public ICollection<string> Categories { get; set; }

    public bool IsPublished { get; set; } = false;
    public DateTime PublishDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

public class PostUpsertDto
{
    [Required]
    [MaxLength(128)]
    public string Title { get; set; } = null!;

    [Required]
    public string Content { get; set; } = null!;

    public bool IsPublished { get; set; } = false;

    public ICollection<TagUpsertDto> Tags { get; set; }
    public ICollection<CategoryUpsertDto> Categories { get; set; }
}


public class ArchivePostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public DateTime PublishDate { get; set; }
    public DateTime CreatedAt { get; set; }
}


public class PostReadWithTag
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public DateTime CreatedAt { get; set; }
}