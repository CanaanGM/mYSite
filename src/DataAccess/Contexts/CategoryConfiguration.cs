using DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccess.Contexts
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> modelBuilder)
        {
            modelBuilder
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("NOW()");

            modelBuilder
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}