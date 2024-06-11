using System.Net;
using System.Net.Http.Json;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.IntegrationTests;

public class AuctionControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private const string Bugatti_ID = "c8c3ec17-01bf-49db-82aa-1ef80b833a9f";

    public AuctionControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturn3Auctions()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetFromJsonAsync<List<AuctionDto>>("api/auctions");

        // Assert
        Assert.Equal(3, response.Count);
    }

    [Fact]
    public async Task GetAuctionById_WithValidId_ShouldReturnAuction()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetFromJsonAsync<AuctionDto>($"api/auctions/{Bugatti_ID}");

        // Assert
        Assert.Equal("Bugatti", response.Make);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidId_Returns404NotFound()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync($"api/auctions/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_WithInvalidGuid_Returns400BadRequest()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync("api/auctions/notaguid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithNoAuth_Returns401Unauthorized()
    {
        // Arrange
        var auction = new CreateAuctionDto {Make = "Test"};

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_WithAuth_Returns201Created()
    {
        // Arrange
        var auction = GetAuctionForCreate();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("bob"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auction);
        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDto>();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("bob", createdAuction.Seller);
        
    }

    public Task InitializeAsync()
        => Task.CompletedTask;
    
    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(db);
        return Task.CompletedTask;
    }

    private static CreateAuctionDto GetAuctionForCreate()
    {
        return new CreateAuctionDto
        {
            Make = "test",
            Model = "testModel",
            ImageUrl = "test",
            Color = "test",
            Mileage = 10,
            Year = 10,
            ReservePrice = 10
        };
    }
}
