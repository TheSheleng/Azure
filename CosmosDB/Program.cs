using Microsoft.EntityFrameworkCore;
using CosmosEfDemo.Data;
using Microsoft.Azure.Cosmos;
using CosmosEfDemo.Models;

var endpoint = "<Your_Cosmos_Endpoint>";
var key = "<Your_Cosmos_Key>";

// Создаём клиент
CosmosClient cosmosClient = CosmosClientFactory.CreateClient(endpoint, key);

// Создаём базу, если не существует
Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("Your_DB");
Console.WriteLine($"Database '{database.Id}' ready.");

// Создаём контейнер (Products)
Container productContainer = await database.CreateContainerIfNotExistsAsync(
    id: "Products",
    partitionKeyPath: "/partitionKey", // Путь к полю для партиционирования
    throughput: 400
);
Console.WriteLine($"Container '{productContainer.Id}' ready.");

// CRUD via SDK:
// C
var product = new Product
{
    Id = Guid.NewGuid().ToString(),
    Name = "Laptop",
    Price = 999.99m
};

ItemResponse<Product> createResponse = await productContainer.CreateItemAsync(
    product,
    new PartitionKey(product.partitionKey)
);
Console.WriteLine($"Created item with id: {createResponse.Resource.Id}");

// R
var itemId = "<Random_ID>";
ItemResponse<Product> readResponse = await productContainer.ReadItemAsync<Product>(
    itemId,
    new PartitionKey(product.partitionKey)
);
var existingProduct = readResponse.Resource;
Console.WriteLine($"Read item: {existingProduct.Name}");

// U
existingProduct.Price = 1099.99m; 
ItemResponse<Product> updateResponse = await productContainer.ReplaceItemAsync(
    existingProduct, 
    existingProduct.Id, 
    new PartitionKey(existingProduct.partitionKey)
);
Console.WriteLine($"Updated item price: {updateResponse.Resource.Price}");

// D
await productContainer.DeleteItemAsync<Product>(
    existingProduct.Id,
    new PartitionKey(existingProduct.partitionKey)
);
Console.WriteLine("Deleted item");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseCosmos(
        accountEndpoint: "<Your_Cosmos_Endpoint>",
        accountKey: "<Your_Cosmos_Key>",
        databaseName: "Your_DB");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
