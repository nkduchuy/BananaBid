using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Data;
using SearchService.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

// Initialize MongoDB and define indexes for searching
try
{
    await DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine("Error initializing database", ex);
    throw;
}

app.Run();
