using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Data;
using SearchService.Entities;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add HttpClient for calling AuctionService
builder.Services.AddHttpClient<AuctionServiceHttpClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

// Initialize MongoDB
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
