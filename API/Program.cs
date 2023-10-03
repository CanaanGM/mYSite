using DataAccess;
using Application;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using API.Common.Errors;

var builder = WebApplication.CreateBuilder(args);
{

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services
        .AddDataAccess(builder.Configuration)
        .AddApplication(builder.Configuration);

    builder.Services.AddSingleton<ProblemDetailsFactory, ScarletSiteProblemDetailsFactory>();
}
var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseExceptionHandler("/error");
    app.UseHttpsRedirection();

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}