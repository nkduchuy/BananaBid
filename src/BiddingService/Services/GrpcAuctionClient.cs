﻿using AuctionService;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient
{
    private readonly ILogger<GrpcAuctionClient> _logger;
    private readonly IConfiguration _config;

    public GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public Auction GetAuction(string id)
    {
        _logger.LogInformation("Calling GRPC Server to get auction with Id {Id}", id);

        var channel = GrpcChannel.ForAddress(_config["GrpcAuction"]);
        var client = new GrpcAuction.GrpcAuctionClient(channel);
        var request = new GetAuctionRequest { Id = id };

        try
        {
            var response = client.GetAuction(request);
            var auction = new Auction {
                ID = response.Auction.Id,
                AuctionEnd = DateTime.Parse(response.Auction.AuctionEnd),
                Seller = response.Auction.Seller,
                ReservePrice = response.Auction.ReservePrice
            };

            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not call GRPC Server.");
            return null;
        }
    }
}
