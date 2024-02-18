using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

// Initialize MongoDB and define indexes for searching
await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));
await DB.Index<Item>()
    .Key(a => a.Make, KeyType.Text)
    .Key(a => a.Model, KeyType.Text)
    .Key(a => a.Color, KeyType.Text)
    .CreateAsync();

app.Run();
