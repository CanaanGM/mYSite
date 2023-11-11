




// Ignore Spelling: Dto Dtos

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Dtos
{

    public class CommentReadDto
    {
        public Guid Id { get; set; }
        public string Body { get; set; } = null!;
        public bool Active { get; set; }
        public Guid PostId { get; set; }
        public Guid? ParentId { get; set; }
        public string AuthorId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
    }

    public class CommentCreateDto
    {
        public Guid Id { get; set; }
        public string Body { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public Guid PostId { get; set; }
    }

    public class CommentUpdateDto
    {
        public Guid Id { get; set; }
        public string Body { get; set; } = null!;

    }
}
