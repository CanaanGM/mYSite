﻿using System.ComponentModel.DataAnnotations;

namespace DataAccess.Dtos
{
    public class TagUpsertDto
    {
        [Required]
        [MaxLength(64)]
        public string Name { get; set; } = null!;
    }

    public class TagReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public ICollection<PostReadWithEntity> Posts { get; set; }
    }
}