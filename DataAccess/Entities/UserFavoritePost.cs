﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class UserFavoritePost
    {
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public Guid PostId { get; set; } 
        public Post Post { get; set; } = null!; 
        public DateTime FavoriteOn { get; set; }
    }
}
