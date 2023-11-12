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

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public ICollection<PostReadWithTag> Posts { get; set; }
    }
}
