namespace DataAccess.Contexts;

using DataAccess.Entities;

using Domain.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BlogContext : IdentityDbContext<User>
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<PostTag> PostTags { get; set; }

    public BlogContext()
    {
        // for cli ?
    }

    public BlogContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityRole>()
            .HasData(
                new IdentityRole { Name = "User", NormalizedName = "USER" },
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" }
                );

        modelBuilder.Entity<PostTag>()
            .HasKey(pt => new { pt.PostId, pt.TagId });

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.PostTags)
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags)
            .HasForeignKey(pt => pt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostCategory>()
            .HasKey(pt => new { pt.PostId, pt.CategoryId });

        modelBuilder.Entity<PostCategory>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.PostCategories)
            .HasForeignKey(pt => pt.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostCategory>()
            .HasOne(pt => pt.Category)
            .WithMany(t => t.CategoryPosts)
            .HasForeignKey(pt => pt.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasIndex(p => p.Slug)
            .IsUnique();

        modelBuilder.Entity<Post>()
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        //modelBuilder.Entity<Post>()
        //    .HasQueryFilter(p => p.IsPublished);

        modelBuilder.Entity<Tag>()
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        modelBuilder.Entity<Category>()
            .HasIndex(t => t.Name)
            .IsUnique();
    }
}