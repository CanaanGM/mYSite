﻿using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Contexts
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> modelBuilder)
        {
            modelBuilder
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId);

            modelBuilder
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");
        }
    }
}