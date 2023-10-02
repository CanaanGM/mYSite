using DataAccess.Contexts;
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
            opt.UseSqlServer(configuration["ConnectionStrings:SqlServer"]); // get this string from config
        });

        return services;
    }
}