using Azure.Data.Tables;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

var foods = new List<Food>();

var tableClient = new TableServiceClient(Environment.GetEnvironmentVariable("a"));

app.MapGet("/list", () => foods).WithName("GetFoods").WithOpenApi();

app.MapPost("/create", (Food food) =>
{
    foods.Add(food);
    return;
})
.WithName("CreateFood")
.WithOpenApi();

app.MapDelete("/delete", (int id) =>
{
    var food = foods.SingleOrDefault(x => x.Id == id);
    if (food is not null)
        foods.Remove(food);
    
    return;
})
.WithName("DeleteFood")
.WithOpenApi();

app.Run();