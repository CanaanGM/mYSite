using DataAccess.Contexts;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess
{
    public class DatabaseInitiliazer
    {
        public static void InitDb(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            SeedData(scope.ServiceProvider.GetService<BlogContext>());
        }

        private static void SeedData(BlogContext? blogContext)
        {
            blogContext?.Database.Migrate();
        }
    }
}