using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AuctionService.IntegrationTests.Fixtures;

public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _postgresSqlContainer = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _postgresSqlContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services => 
        {
            // Remove the existing AuctionDbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AuctionDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add AuctionDbContext using an in-memory database for testing
            services.AddDbContext<AuctionDbContext>(options =>
            {
                options.UseNpgsql(_postgresSqlContainer.GetConnectionString());
            });

            // Add MassTransitTestHarness
            services.AddMassTransitTestHarness();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AuctionDbContext>();

            db.Database.Migrate();
        });
    }

    Task IAsyncLifetime.DisposeAsync()
        => _postgresSqlContainer.DisposeAsync().AsTask();
}
