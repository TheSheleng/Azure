using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlobAccess
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = Shared.Configuration.Build().GetConnectionString("Storage");
            BlobServiceClient serviceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient("container1");

            // Скачивание всех blob-объектов с эмуляцией папок
            await DownloadBlobsAsync(containerClient);

            // Создание SAS ссылки на файл
            var blobClient = containerClient.GetBlobClient("folder1/file.txt"); // Пример blob с "папкой"
            var sasUri = GetBlobSasUri(blobClient, TimeSpan.FromDays(7));
            Console.WriteLine($"SAS URI for blob: {sasUri}");

            Console.ReadKey();
        }

        // Метод для асинхронного скачивания blob-ов, эмулируя папки
        private static async Task DownloadBlobsAsync(BlobContainerClient containerClient)
        {
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);

                // Создаем локальные папки, если blob имеет префикс (эмуляция папок)
                string localFilePath = Path.Combine("local_downloads", blobItem.Name.Replace("/", Path.DirectorySeparatorChar.ToString()));
                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath) ?? string.Empty);

                // Скачиваем файл
                Console.WriteLine($"Downloading blob: {blobItem.Name} to {localFilePath}");
                await blobClient.DownloadToAsync(localFilePath);
            }
        }

        // Метод для создания SAS URI с правами чтения на 7 дней
        private static Uri GetBlobSasUri(BlobClient blobClient, TimeSpan validDuration)
        {
            // Убедимся, что Blob существует
            if (!blobClient.CanGenerateSasUri)
            {
                throw new InvalidOperationException("Cannot create SAS token for this blob.");
            }

            // Создаем SAS токен
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b", // "b" означает blob
                ExpiresOn = DateTimeOffset.UtcNow.Add(validDuration)
            };

            // Даем права только на чтение
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Возвращаем полный URI с SAS токеном
            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri;
        }
    }
}
