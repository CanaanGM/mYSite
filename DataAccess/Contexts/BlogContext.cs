namespace DataAccess.Contexts;

using Domain.Entities;
using Microsoft.EntityFrameworkCore;
public class BlogContext : DbContext 
{

    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Category> Categories { get; set; }

    public BlogContext()
    {
        // for cli ? 
    }

    public BlogContext(DbContextOptions options) : base(options)
    {

    }
}