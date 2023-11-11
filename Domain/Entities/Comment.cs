using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Body { get; set; } = null!;
        public bool Active { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; } = null!;
        public Guid? parentId { get; set; }
        public Comment? Parent { get; set; }
        public List<Comment>? Replies { get; set; }

        public int Likes { get; set; }
        public int DisLikes { get; set; }

        public string? AuthorId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
    }
}
