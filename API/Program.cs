using API.Common.Errors;
using API.Common.Middleware;
using API.Extensions;

using Application;

using DataAccess;

using Microsoft.AspNetCore.Mvc.Infrastructure;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    ;
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services
        .AddDataAccess(builder.Configuration)
        .AddApplication(builder.Configuration)
        .AddAuthServices(builder.Configuration);

    builder.Services.AddSingleton<ProblemDetailsFactory, ScarletSiteProblemDetailsFactory>();
}
var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(opt =>
        {
            opt.ConfigObject.PersistAuthorization = true;
        });
    }

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<RequestLoggerMiddleWare>();

    app.UseExceptionHandler("/error");

    app.UseCors("CorsPolicy");
    //app.UseHttpsRedirection();

    app.UseAuthentication(); // hi, I AM b4 AUTHORIZATION !
    app.UseAuthorization(); // hi, I AM AUTHORIZATION !

    app.MapControllers();

    try
    {
        DatabaseInitiliazer.InitDb(app);
    }
    catch (Exception ex)
    {
        // add a retry mechanism
        Console.WriteLine("Add me to a proper logger damn u!!!");
        Console.WriteLine(ex);

    }

    app.Run();
}