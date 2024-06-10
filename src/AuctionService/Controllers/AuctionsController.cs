using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuctionsController : ControllerBase
    {
        private readonly IAuctionRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(IAuctionRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _repo = repo;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            return await _repo.GetAuctionsAsync(date);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _repo.GetAuctionByIdAsync(id);

            if (auction == null)
                return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto createAuctionDto)
        {
            var auction = _mapper.Map<Auction>(createAuctionDto);
            
            auction.Seller = User.Identity.Name;

            // Add to Auctions table
            _repo.AddAuction(auction);

            // Map and publish to RabbitMQ
            var newAuction = _mapper.Map<AuctionDto>(auction);
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

            // Save all changes to database
            var result = await _repo.SaveChangesAsync();

            if (!result)
                return BadRequest();

            return CreatedAtAction(nameof(GetAuctionById), 
                new {auction.Id}, _mapper.Map<AuctionDto>(auction));
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
        {
            var auction = await _repo.GetAuctionEntityById(id);

            if (auction == null)
                return NotFound();

            // Check seller is current user
            if (auction.Seller != User.Identity.Name)
                return Forbid();
            
            // Update all fields
            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            // Publish message
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            // Save all changes to database
            var result = await _repo.SaveChangesAsync();

            if (!result)
                return BadRequest("Problem saving changes");

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _repo.GetAuctionEntityById(id);

            if (auction == null)
                return NotFound();
            
            // Check seller is current user
            if (auction.Seller != User.Identity.Name)
                return Forbid("You are not allowed to delete this auction");

            _repo.RemoveAuction(auction);

            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _repo.SaveChangesAsync();

            if (!result)
                return BadRequest("Could not delete auction");
            
            return Ok();
        }
    }
}