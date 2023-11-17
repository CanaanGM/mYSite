using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Contexts
{
    public class UserFavoritePostConfiguration : IEntityTypeConfiguration<UserFavoritePost>
    {
        public void Configure(EntityTypeBuilder<UserFavoritePost> modelBuilder)
        {
            modelBuilder
                .HasKey(pu => new { pu.PostId, pu.UserId });

            modelBuilder
                .HasOne(pu => pu.User)
                .WithMany(pu => pu.Favorites)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .HasOne(pu => pu.Post)
                .WithMany(pu => pu.Favorites)
                .HasForeignKey(pu => pu.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder
                .Property(p => p.FavoriteOn)
                .HasDefaultValueSql("NOW()");

            modelBuilder
                .HasIndex(p => p.FavoriteOn);
            modelBuilder
                .HasIndex(p => p.PostId);
            modelBuilder
                .HasIndex(p => p.UserId);
        }
    }
}