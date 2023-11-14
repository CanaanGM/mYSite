using DataAccess.Entities;

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
    public class ConfigureCommentEntity : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> modelBuilder)
        {
            modelBuilder
                .HasOne(x => x.Parent)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.parentId);

            modelBuilder
                .HasIndex(x => x.Id)
                .IsUnique();

            modelBuilder
                .HasIndex(x => x.AuthorId);

            modelBuilder
                .HasIndex(x => x.parentId);

            modelBuilder
                .HasIndex(x => x.PostId);
        }
    }
}
