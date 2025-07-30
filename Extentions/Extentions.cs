using Catalog.Service.Entities;
using static Catalog.Service.Dto.Dtos;

namespace Catalog.Service.Extentions;

public static class Extensions
{
    public static ItemDto AsDto(this Item item)
    {
        return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
    }
}
