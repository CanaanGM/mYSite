using DataAccess.Contexts;
using DataAccess.Dtos;
using DataAccess.Entities;
using DataAccess.Repos;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        ConfigurationManager configuration)
    {
        services.AddDbContext<BlogContext>(opt =>
        {
            //opt.UseSqlServer(configuration["ConnectionStrings:SqlServer"]);
            opt.UseNpgsql(configuration["ConnectionStrings:Postgres"]);
        });

        services.AddScoped<IPostRepo, PostRepo>();
        services.AddScoped<ITagRepo, TagRepo>();
        services.AddScoped<ICommentRepo, CommentRepo>();

        services.AddAutoMapper(typeof(Profiles).Assembly);

        services.AddIdentityCore<User>(opt =>
        {
            opt.User.RequireUniqueEmail = true; //TODO: further configure this
            opt.SignIn.RequireConfirmedEmail = true;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<BlogContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}