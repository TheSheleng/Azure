using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using TableSearch.Services;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionString = Shared.Configuration.Build().GetConnectionString("Storage");
        var table = new StorageService(connectionString).Table1;
        
        // Запрос отфильтрованных записей
        var query = new TableQuery<BadMessage>()
        {
            TakeCount = 5
        }
        .Where(
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "BadMessages"),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("Text", QueryComparisons.GreaterThanOrEqual, "A"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Text", QueryComparisons.LessThan, "C"))));

        TableContinuationToken token = null;
        TableQuerySegment<BadMessage> seg;

        // Обработка отфильтрованных записей
        do
        {
            seg = await table.ExecuteQuerySegmentedAsync(query, token);
            token = seg.ContinuationToken;

            foreach (var badMessage in seg.Results)
            {
                // Добавляем "1" к полю Text
                badMessage.Text += "1";

                // Обновляем запись в таблице
                var operation = TableOperation.Replace(badMessage);
                await table.ExecuteAsync(operation);

                // Выводим изменённое сообщение на экран
                Console.WriteLine(badMessage.Text);
            }
        } while (token != null);

        // Добавление 5 новых записей с другим полем
        for (int i = 0; i < 5; i++)
        {
            var newRecord = new NewEntity("NewPartition", $"Row{i}")
            {
                CustomField = $"CustomValue{i}"
            };

            var insertOperation = TableOperation.Insert(newRecord);
            await table.ExecuteAsync(insertOperation);

            Console.WriteLine($"Inserted: {newRecord.CustomField}");
        }

        // Чтение всех записей с использованием DynamicTableEntity
        var dynamicQuery = new TableQuery<DynamicTableEntity>();
        token = null;

        do
        {
            var dynamicSegment = await table.ExecuteQuerySegmentedAsync(dynamicQuery, token);
            token = dynamicSegment.ContinuationToken;

            foreach (var entity in dynamicSegment.Results)
            {
                if (entity.Properties.ContainsKey("Text"))
                {
                    // Обрабатываем как BadMessage
                    var badMessage = new BadMessage(entity.PartitionKey, entity.RowKey)
                    {
                        Text = entity.Properties["Text"].StringValue
                    };
                    Console.WriteLine($"BadMessage: {badMessage.Text}");
                }
                else if (entity.Properties.ContainsKey("CustomField"))
                {
                    // Обрабатываем как NewEntity
                    var newEntity = new NewEntity(entity.PartitionKey, entity.RowKey)
                    {
                        CustomField = entity.Properties["CustomField"].StringValue
                    };
                    Console.WriteLine($"NewEntity: {newEntity.CustomField}");
                }
            }
        } while (token != null);

        Console.ReadKey();
    }
}

// Класс для хранения отфильтрованных сообщений (существующие записи)
public class BadMessage : TableEntity
{
    public BadMessage() { }

    public BadMessage(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public string Text { get; set; }
}

// Новый класс для добавления записей с другим полем
public class NewEntity : TableEntity
{
    public NewEntity() { }

    public NewEntity(string partitionKey, string rowKey)
    {
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public string CustomField { get; set; }
}
