using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers;

public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> context)
    {
        // Get auction from db
        var auction = await DB.Find<Item>().OneAsync(context.Message.AuctionId);

        // If sold, update winner and sold amount
        if (context.Message.ItemSold)
        {
            auction.Winner = context.Message.Winner;
            auction.SoldAmount = (int)context.Message.Amount;
        }

        // Update status and save
        auction.Status = "Finished";
        await auction.SaveAsync();
    }
}