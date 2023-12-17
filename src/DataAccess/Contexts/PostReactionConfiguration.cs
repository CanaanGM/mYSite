using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Contexts
{
    public class PostReactionConfiguration : IEntityTypeConfiguration<PostUserReaction>
    {
        public void Configure(EntityTypeBuilder<PostUserReaction> modelBuilder)
        {
            modelBuilder
                .HasKey(ulp => new { ulp.UserId, ulp.PostId });

            modelBuilder
                .HasOne(ulp => ulp.User)
                .WithMany(u => u.PostReactions)
                .HasForeignKey(ulp => ulp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .HasOne(ulp => ulp.Post)
                .WithMany(p => p.UserReactions)
                .HasForeignKey(ulp => ulp.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}