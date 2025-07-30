using Catalog.Contracts;
using Catalog.Service.Entities;
using Catalog.Service.Extentions;
using Common.Library;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Catalog.Service.Dto.Dtos;

namespace Catalog.Service.Controllers;
[Route("[controller]")]
[ApiController]
[Authorize(Roles = AdminRole)]
public class ItemsController : ControllerBase
{
    private const string AdminRole = "Admin";

    private readonly IRepository<Item> itemsRepository;
    //private static int requestCounter = 0;
    private readonly IPublishEndpoint publishEndpoint;
    public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
    {
        this.itemsRepository = itemsRepository;
        this.publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [Authorize(Policies.Read)]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        //requestCounter++;
        //Console.WriteLine($"Request {requestCounter}: Starting...");
        //if (requestCounter <= 2)
        //{
        //    Console.WriteLine($"Request {requestCounter}: Delaying...");
        //    await Task.Delay(TimeSpan.FromSeconds(10));
        //}
        //if (requestCounter <= 4)
        //{
        //    Console.WriteLine($"Request {requestCounter}: 500 (Internal Server Error)");
        //    return StatusCode(500);
        //}

        var items = (await itemsRepository.GetAllAsync())
            .Select(item => item.AsDto());
        return Ok(items);
    }

    [HttpGet("{id}")]
    [Authorize(Policies.Read)]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await itemsRepository.GetAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return item.AsDto();
    }

    [HttpPost]
    [Authorize(Policies.Write)]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
    {
        var item = new Item
        {
            Name = createItemDto.Name,
            Description = createItemDto.Description,
            Price = createItemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow,
        };
        await itemsRepository.CreateAsync(item);
        await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description, item.Price));
        return CreatedAtAction(nameof(GetByIdAsync), new { Id = item.Id }, item);
    }


    [HttpPut("{id}")]
    [Authorize(Policies.Write)]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
    {
        var existingItem = await itemsRepository.GetAsync(id);
        if (existingItem == null)
        {
            return NotFound();
        }
        existingItem.Name = updateItemDto.Name;
        existingItem.Description = updateItemDto.Description;
        existingItem.Price = updateItemDto.Price;

        await itemsRepository.UpdateAsync(existingItem);
        await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name,
            existingItem.Description, existingItem.Price));
        return NoContent();
    }


    [HttpDelete("{id}")]
    [Authorize(Policies.Write)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await itemsRepository.GetAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        await itemsRepository.RemoveAsync(item.Id);
        await publishEndpoint.Publish(new CatalogItemDeleted(item.Id));
        return NoContent();
    }

}


