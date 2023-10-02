using API.Exceptions;
using DataAccess;
using Application;

var builder = WebApplication.CreateBuilder(args);
{

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services
        .AddDataAccess(builder.Configuration)
        .AddApplication(builder.Configuration);

}
var app = builder.Build();
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseHttpsRedirection();

    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}