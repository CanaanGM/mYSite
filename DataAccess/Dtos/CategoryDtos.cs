using System.ComponentModel.DataAnnotations;

namespace DataAccess.Dtos
{
    public class CategoryUpsertDto
    {
        [Required]
        [MaxLength(64)]
        public string Name { get; set; } = null!;
    }

    public class CategoryReadDto
    {
        public CategoryReadDto(Guid id, string name, DateTime createdAt, DateTime lastModifiedAt)
        {
            Id = id;
            Name = name;
            CreatedAt = createdAt;
            LastModifiedAt = lastModifiedAt;
        }

        public Guid Id { get; }
        public string Name { get; } = null!;
        public DateTime CreatedAt { get; }
        public DateTime LastModifiedAt { get; }
    }
}