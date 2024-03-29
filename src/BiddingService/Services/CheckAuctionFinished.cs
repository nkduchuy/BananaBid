

using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace BiddingService.Services;

public class CheckAuctionFinished : BackgroundService
{
    private readonly ILogger<CheckAuctionFinished> _logger;
    private readonly IServiceProvider _services;

    public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting check for finished auctions.");

        stoppingToken.Register(() => _logger.LogInformation("==> Stopping check for finished auctions."));

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckAuctions(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task CheckAuctions(CancellationToken stoppingToken)
    {
        // Get all auctions that have ended and not yet marked as Finished
        var finishedAuctions = await DB.Find<Auction>()
            .Match(x => x.AuctionEnd <= DateTime.UtcNow)
            .Match(x => !x.Finished)
            .ExecuteAsync(stoppingToken);

        // Return if none found
        if (finishedAuctions.Count == 0)
            return;
        
        _logger.LogInformation($"==> Found {finishedAuctions.Count} finished auctions.");

        using var scope = _services.CreateScope();
        var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var auction in finishedAuctions)
        {
            // Mark the auction as finished
            auction.Finished = true;
            await auction.SaveAsync(null, stoppingToken);

            // Get the winning bid if any
            var winningBid = await DB.Find<Bid>()
                .Match(x => x.AuctionId == auction.ID)
                .Match(x => x.BidStatus == BidStatus.Accepted)
                .Sort(x => x.Amount, Order.Descending)
                .ExecuteFirstAsync(stoppingToken);

            // Publish the AuctionFinished event with the auction and winning bid details
            await endpoint.Publish(new AuctionFinished
            {
                ItemSold = winningBid != null,
                AuctionId = auction.ID,
                Winner = winningBid?.Bidder,
                Amount = winningBid?.Amount,
                Seller = auction.Seller
            }, stoppingToken);
        }
    }
}
