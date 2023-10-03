using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Repos;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace  DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {

        services.AddDbContext<BlogContext>(opt =>
        {
            opt.UseSqlServer(configuration["ConnectionStrings:SqlServer"]); 
        });

        services.AddScoped<IPostRepo, PostRepo>();
        services.AddScoped<ITagRepo, TagRepo>();

        services.AddAutoMapper(typeof(Profiles).Assembly);
        
        return services;
    }
}