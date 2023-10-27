using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace API.Extensions
{
    public static class AuthServicesExtention
    {
        public static IServiceCollection AddAuthServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Personal Site/Blog backend",
                    Contact = new OpenApiContact { Name = "Canaan Mohammad", Email = "Canaand1@gmail.com", Url = new Uri("https://github.com/CanaanGM") },
                    Description = "Backend to our site! 😊🐈",
                    Version = "v2"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Scheme = "Bearer",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            services.AddCors(options => options.AddPolicy(
                "CorsPolicy",
    policy =>
                    {
                        policy
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .WithOrigins("http://localhost:4200")
                          .WithOrigins("http://localhost:5174")
                          .WithOrigins("http://localhost:3000")
                          .WithOrigins("http://localhost:5173")
                          .WithOrigins("http://localhost:5175")
                          .WithOrigins("http://localhost:4321")
                          .WithOrigins("http://localhost:4322")


                          ;
                    })
            );

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(opt =>
                    {
                        opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = configuration["JwtSettings:Issuer"]!,

                            ValidateAudience = true,
                            ValidAudience = configuration["JwtSettings:Audience"]!,

                            
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.FromMinutes(5), //TODO: change this to sec for dep
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]!)),
                            ValidAlgorithms = new[]
                            {
                                SecurityAlgorithms.HmacSha256,
                                SecurityAlgorithms.HmacSha512Signature,
                            }
                        };

                    });

             JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthorization();
            return services;
        }
    }
}