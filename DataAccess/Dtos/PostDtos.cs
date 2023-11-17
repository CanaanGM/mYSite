using System.ComponentModel.DataAnnotations;

namespace DataAccess.Dtos;

public class PostReadDetailsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsSoftDeleted { get; set; }
    public ICollection<TagReadDto> Tags { get; set; }
    public ICollection<CategoryReadDto> Categories { get; set; }
    public ICollection<CommentReadDto> Comments { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public UserReadDto Author { get; set; } = null!;
    public bool IsPublished { get; set; } = false;
    public DateTime PublishDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

public class PostGeneralInfoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsSoftDeleted { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
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
    public string? AuthorId { get; set; }
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

public class PostReadWithEntity
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public DateTime CreatedAt { get; set; }
}