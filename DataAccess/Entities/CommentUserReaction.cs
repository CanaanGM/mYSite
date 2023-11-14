using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class CommentUserReaction
    {
        public string UserId { get; set; }
        public Guid CommentId { get; set; }
        public ReactionType ReactionType { get; set; } 

        public User User { get; set; }
        public Comment Comment { get; set; }
    }
}
