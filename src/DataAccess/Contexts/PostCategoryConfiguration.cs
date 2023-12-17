using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Contexts
{
    public class PostCategoryConfiguration : IEntityTypeConfiguration<PostCategory>
    {
        public void Configure(EntityTypeBuilder<PostCategory> modelBuilder)
        {
            modelBuilder
                .HasKey(pt => new { pt.PostId, pt.CategoryId });

            modelBuilder
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostCategories)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .HasOne(pt => pt.Category)
                .WithMany(t => t.CategoryPosts)
                .HasForeignKey(pt => pt.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}