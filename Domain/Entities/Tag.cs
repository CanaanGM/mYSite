using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Tag 
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}