﻿using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Contexts
{
    public class CommentReactionConfiguration : IEntityTypeConfiguration<CommentUserReaction>
    {
        public void Configure(EntityTypeBuilder<CommentUserReaction> modelBuilder)
        {
            modelBuilder
                .HasKey(ulc => new { ulc.UserId, ulc.CommentId });
                
            modelBuilder
                .HasOne(ulc => ulc.User)
                .WithMany(u => u.CommentReactions)
                .HasForeignKey(ulc => ulc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .HasOne(ulc => ulc.Comment)
                .WithMany(c => c.UserReactions)
                .HasForeignKey(ulc => ulc.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
