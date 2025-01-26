using Microsoft.Azure.Cosmos;

namespace CosmosEfDemo.Data
{
    public static class CosmosClientFactory
    {
        private static CosmosClient _client;

        public static CosmosClient CreateClient(string endpoint, string key)
        {
            if (_client == null)
            {
                _client = new CosmosClient(endpoint, key);
            }
            return _client;
        }
    }
}
