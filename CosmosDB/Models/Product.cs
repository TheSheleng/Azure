namespace CosmosEfDemo.Models;

public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Поле для партиционирования
    public string partitionKey => "ProductPartition";
}

