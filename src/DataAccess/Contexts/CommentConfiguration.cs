using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Contexts
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
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