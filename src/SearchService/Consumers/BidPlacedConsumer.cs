using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService;

public class BidPlacedConsumer : IConsumer<BidPlaced>
{
    public async Task Consume(ConsumeContext<BidPlaced> context)
    {
        Console.WriteLine("--> Consuming bid placed event");

        // Get auction from db
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        // Update high bid if new bid is higher
        if (context.Message.BidStatus.Contains("Accepted") 
            && context.Message.Amount > auction.CurrentHighBid)
        {
            auction.CurrentHighBid = context.Message.Amount;
            await auction.SaveAsync();
        }
    }
}