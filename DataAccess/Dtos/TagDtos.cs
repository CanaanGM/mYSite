using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public TagReadDto(Guid id, string name, DateTime createdAt, DateTime lastModifiedAt)
        {
            Id = id;
            Name = name;
            CreatedAt = createdAt;
            LastModifiedAt = lastModifiedAt;
        }

        public Guid Id { get;  }
        public string Name { get; } = null!;
        public DateTime CreatedAt { get;  }
        public DateTime LastModifiedAt { get; }

    }
}
