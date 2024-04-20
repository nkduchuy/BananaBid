using AutoMapper;
using BiddingService.Services;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;

namespace BiddingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly GrpcAuctionClient _grpcClient;

        public BidsController(IMapper mapper, IPublishEndpoint publishEndpoint, GrpcAuctionClient grpcClient)
        {
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
            _grpcClient = grpcClient;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BidDto>> PlaceBid(string auctionId, int amount)
        {
            var auction = await DB.Find<Auction>().OneAsync(auctionId);

            if (auction == null)
            {
                auction = _grpcClient.GetAuction(auctionId);

                if (auction == null)
                    return BadRequest("Cannot accept bids on this auction at this time.");
            }

            if (auction.Seller == User.Identity.Name)
                return BadRequest("Seller cannot bid on their own auction.");
            
            var bid = new Bid
            {
                AuctionId = auctionId,
                Bidder = User.Identity.Name,
                Amount = amount
            };

            if (auction.AuctionEnd < DateTime.UtcNow)
                bid.BidStatus = BidStatus.Finished;
            else {
                // Get current highest bid
                var highBid = await DB.Find<Bid>()
                    .Match(a => a.AuctionId == auctionId)
                    .Sort(b => b.Descending(x => x.Amount))
                    .ExecuteFirstAsync();
                
                // Check against high bid and set status
                if (highBid != null && amount > highBid.Amount || highBid == null)
                    bid.BidStatus = amount > auction.ReservePrice
                        ? BidStatus.Accepted
                        : BidStatus.AcceptedBelowReserve;
                
                if (highBid != null && bid.Amount <= highBid.Amount)
                    bid.BidStatus = BidStatus.TooLow;
            }
            
            // Save new bid
            await DB.SaveAsync(bid);

            // Publish BidPlaced event
            await _publishEndpoint.Publish(_mapper.Map<BidPlaced>(bid));

            return Ok(_mapper.Map<BidDto>(bid));
        }

        [HttpGet("{auctionId}")]
        public async Task<ActionResult<List<BidDto>>> GetBidsForAuction(string auctionId)
        {
            var bids = await DB.Find<Bid>()
                .Match(a => a.AuctionId == auctionId)
                .Sort(b => b.Descending(x => x.BidTime))
                .ExecuteAsync();

            return bids.Select(_mapper.Map<BidDto>).ToList();
        }
    }
}