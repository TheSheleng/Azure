using Azure.Storage.Blobs;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = Shared.Configuration.Build().GetConnectionString("Storage");
            BlobServiceClient serviceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient("container1");

            foreach (var blob in containerClient.GetBlobs())
            {
                var reference = containerClient.GetBlobClient(blob.Name);
                reference.DownloadTo(blob.Name);
            }

            Console.ReadKey();
        }
    }
}
