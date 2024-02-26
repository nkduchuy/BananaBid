using AuctionService.Data;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinished>
    {
        private readonly AuctionDbContext _dbContext;

        public AuctionFinishedConsumer(AuctionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<AuctionFinished> context)
        {
            
            Console.WriteLine("--> Consuming auction finished event");

            // Get auction from db
            var auction = await _dbContext.Auctions.FindAsync(context.Message.AuctionId);

            // If sold, update winner and sold amount
            if (context.Message.ItemSold)
            {
                auction.Winner = context.Message.Winner;
                auction.SoldAmount = context.Message.Amount;
            }

            // Update auction status
            auction.Status = auction.SoldAmount > auction.ReservePrice
                ? Entities.Status.Finished : Entities.Status.ReserveNotMet;

            await _dbContext.SaveChangesAsync();
        }
    }
}