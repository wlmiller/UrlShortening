using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Durwella.UrlShortening
{
    public class AzureTableAliasRepository : IAliasRepository
    {
        public const string Partition = "Global";
        public const string DefaultTablePrefix = "UrlShortening";

        /// <summary>
        /// The age after which the key cannot be removed.
        /// </summary>
        public TimeSpan LockAge { get; set; }

        public class Entity : TableEntity
        {
            public string Value { get; set; }

            public Entity()
            {
            }

            public Entity(string key, string value)
            {
                PartitionKey = Partition;
                RowKey = key;
                Value = value;
            }
        }

        /// <summary>
        /// The default repository uses the Development Storage Emulator which must be running. 
        /// See: http://azure.microsoft.com/en-us/documentation/articles/storage-use-emulator/
        /// </summary>
        public AzureTableAliasRepository()
            : this(CloudStorageAccount.DevelopmentStorageAccount, DefaultTablePrefix)
        {
        }

        public AzureTableAliasRepository(string azureStorageAccountName, string azureStorageAccessKey, string tablePrefix = DefaultTablePrefix)
            : this(new StorageCredentials(azureStorageAccountName, azureStorageAccessKey), tablePrefix)
        {
        }

        public AzureTableAliasRepository(StorageCredentials credentials, string tablePrefix = DefaultTablePrefix)
            : this(new CloudStorageAccount(credentials, useHttps: true), tablePrefix)
        {
        }

        public AzureTableAliasRepository(string connectionString, string tablePrefix = DefaultTablePrefix)
            : this(CloudStorageAccount.Parse(connectionString), tablePrefix)
        {
        }

        public AzureTableAliasRepository(CloudStorageAccount account, string tablePrefix)
        {
            LockAge = TimeSpan.FromHours(12);
            var tableClient = account.CreateCloudTableClient();
            _table = tableClient.GetTableReference(tablePrefix);
            _table.CreateIfNotExistsAsync().Wait();
        }

        public async Task Add(string key, string value)
        {
            var entity = new Entity(key, value);
            var insertOperation = TableOperation.InsertOrReplace(entity);
            await _table.ExecuteAsync(insertOperation);
        }

        public async Task<bool> Remove(string key)
        {
            var entity = await RetrieveEntity(key);

            if (entity == null)
                return false;

            var entityAge = DateTimeOffset.UtcNow.Subtract(entity.Timestamp);

            if (entityAge > LockAge)
                throw new InvalidOperationException("Cannot change a short URL that is too old.");

            var removeOperation = TableOperation.Delete(entity);
            await _table.ExecuteAsync(removeOperation);

            return true;
        }

        public async Task<bool> ContainsKey(string key)
        {
            var entity = await RetrieveEntity(key);
            return entity != null;
        }

        public async Task<bool> ContainsValue(string value)
        {
            return (await _table.ExecuteQuerySegmentedAsync(WhereValueIs(value), null)).Any();
        }

        public async Task<string> GetKey(string value)
        {
            var entity = (await _table.ExecuteQuerySegmentedAsync(WhereValueIs(value), null)).First();
            return entity.RowKey;
        }

        public async Task<string> GetValue(string key)
        {
            var entity = await RetrieveEntity(key);
            return entity.Value;
        }

        private async Task<Entity> RetrieveEntity(string key)
        {
            var op = TableOperation.Retrieve<Entity>(Partition, key);
            var result = await _table.ExecuteAsync(op);
            var entity = (Entity) result.Result;
            return entity;
        }
        private static TableQuery<Entity> WhereValueIs(string value)
        {
            return new TableQuery<Entity>().Where(TableQuery.GenerateFilterCondition("Value", QueryComparisons.Equal, value));
        }

        private readonly CloudTable _table;
    }
}
