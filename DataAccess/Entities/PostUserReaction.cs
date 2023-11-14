﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class PostUserReaction
    {
        public string UserId { get; set; }
        public Guid PostId { get; set; }
        public ReactionType ReactionType { get; set; } 

        public User User { get; set; }
        public Post Post { get; set; }

        // can add later more props, like liked on (DateTime)
    }
}
