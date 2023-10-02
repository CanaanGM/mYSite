namespace Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection 
{
    public static IServiceCollection AddApplication (this IServiceCollection services,
        ConfigurationManager configuration)
    {
        
        return services;
    }
}