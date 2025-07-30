using Common.Library;

namespace Catalog.Service.Entities;

public class Item : IEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

}
