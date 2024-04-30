using Azure.Data.Tables;

using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

var partitionKey = "food";
var tableClient = new TableClient(
    "",
    "fastfood");
await tableClient.CreateIfNotExistsAsync();

app.MapGet("/list", () =>
{
    var result = tableClient.Query<FoodTableEntity>(filter: $"PartitionKey eq '{partitionKey}'");
    var foods = new List<Food>();
    foreach (var tableEntity in result)
    {
        var food = new Food();
        food.Id = tableEntity.Id;
        food.Kcal = tableEntity.Kcal;
        food.Name = tableEntity.Name;
        food.Price = tableEntity.Price;
        food.Description = tableEntity.Description;

        if (tableEntity.Components != null)
            food.Components = JsonSerializer.Deserialize<List<string>>(tableEntity.Components);
        
        foods.Add(food);
    }

    return foods;
})
.WithName("GetFoods").WithOpenApi();

app.MapPost("/create", async (Food food) =>
{
    var foodEntity = await GetFoodAsync(food.Id);
    if (foodEntity is not null)
        Results.UnprocessableEntity("Ilyen étel már létezik!");

    var foodTableEntity = new FoodTableEntity(food);
    if (food.Components.Any())
        foodTableEntity.Components = JsonSerializer.Serialize(food.Components);

    await tableClient.AddEntityAsync(foodTableEntity);

    return Results.Ok();
})
.WithName("CreateFood")
.WithOpenApi();

app.MapPut("/update", async (Food food) =>
{
    var foodEntity = await tableClient.GetEntityAsync<FoodTableEntity>(partitionKey, food.Id.ToString());
    if (foodEntity is null)
        return Results.NotFound("Étel nem található");

    foodEntity.Value.Description = food.Description;
    foodEntity.Value.Price = food.Price;
    foodEntity.Value.Name = food.Name;
    foodEntity.Value.Kcal = food.Kcal;
    if (food.Components.Any())
        foodEntity.Value.Components = JsonSerializer.Serialize(food.Components);

    await tableClient.UpsertEntityAsync<FoodTableEntity>(foodEntity);

    return Results.Ok();
})
.WithName("UpdateFood")
.WithOpenApi();

app.MapDelete("/delete", async (int id) =>
{
    var food = await GetFoodAsync(id);
    if (food is null)
        return Results.NotFound("Étel nem található");

    await tableClient.DeleteEntityAsync("food", id.ToString());

    return Results.Ok();
})
.WithName("DeleteFood")
.WithOpenApi();

app.Run();

async Task<FoodTableEntity?> GetFoodAsync(int id)
{
    try
    {
        var food = await tableClient.GetEntityAsync<FoodTableEntity>(partitionKey, id.ToString());
        return food;
    }
    catch (Exception)
    {
        return null;
    }
}