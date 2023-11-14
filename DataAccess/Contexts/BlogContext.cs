// Ignore Spelling: Blog

namespace DataAccess.Contexts;

using DataAccess.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class BlogContext : IdentityDbContext<User>
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Comment> Comments { get; set; }

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
        modelBuilder.Entity<User>()
            .HasMany(u => u.Comments);


        modelBuilder.ApplyConfiguration(new ConfigureCategoryEntity());
        modelBuilder.ApplyConfiguration(new ConfigureCommentEntity());
        modelBuilder.ApplyConfiguration(new ConfigurePostCategoryEntity());
        modelBuilder.ApplyConfiguration(new ConfigurePostEntity());
        modelBuilder.ApplyConfiguration(new ConfigurePostTagEntity());
        modelBuilder.ApplyConfiguration(new ConfigureTagEntity());






        //modelBuilder.Entity<Post>()
        //    .HasQueryFilter(p => p.IsPublished);








    }
}