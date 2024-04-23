using Azure;
using Azure.Data.Tables;

using System.Text.Json;

public class FoodTableEntity : BaseFood, ITableEntity
{
    public FoodTableEntity()
    { }

    public string Components { get; set; }
    public string PartitionKey { get; set; } = "food";
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public ETag ETag { get; set; }

    public FoodTableEntity(Food food)
    {
        Description = food.Description;
        Price = food.Price;
        Kcal = food.Kcal;
        Name = food.Name;
        Id = food.Id;
        Components = JsonSerializer.Serialize(food.Components);
        RowKey = food.Id.ToString();
    }
}
