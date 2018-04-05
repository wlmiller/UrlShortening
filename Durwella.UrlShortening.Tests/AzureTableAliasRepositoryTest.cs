using FluentAssertions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

// The tests here run against a live Azure Storage account.
// So, to run them you need to populate AzureTestCredentials.txt with two lines:
// Line 1: Your Azure Storage Account Name
// Line 2: Your Azure Storage Access Key
// It is strongly recommended you avoid committing your credentials 
// by invoking the following command from this project's directory:
//     git update-index --assume-unchanged AzureTestCredentials.txt
// 

namespace Durwella.UrlShortening.Tests
{
    public class AzureTableAliasRepositoryFixture : IDisposable
    {
        public string AzureStorageAccountName { get; }
        public string AzureStorageAccessKey { get; }
        public string TablePrefix => "UrlShorteningTest";
        public CloudTable Table { get; }
        public AzureTableAliasRepository Subject { get; }

        public AzureTableAliasRepositoryFixture()
        {
            var credentialsFile = File.ReadAllLines("AzureTestCredentials.txt");
            if (credentialsFile.Length < 2
                || String.IsNullOrWhiteSpace(credentialsFile[0])
                || String.IsNullOrWhiteSpace(credentialsFile[1]))
                return;
            AzureStorageAccountName = credentialsFile[0];
            AzureStorageAccessKey = credentialsFile[1];
            var credentials = new StorageCredentials(AzureStorageAccountName, AzureStorageAccessKey);
            var account = new CloudStorageAccount(credentials, useHttps: true);
            var tableClient = account.CreateCloudTableClient();
            Table = tableClient.GetTableReference(TablePrefix);
            // Delete all the test entries from the test table
            // Deleting the table itself is too slow because it cannot be immediately recreated
            for (int i = 0; i < 10; i++)
            {
                var key = "TheTestKey" + i.ToString();
                var retrieveOp = TableOperation.Retrieve(AzureTableAliasRepository.Partition, key);
                var result = Table.ExecuteAsync(retrieveOp).Result;
                if (result.Result != null)
                {
                    var deleteOp = TableOperation.Delete(result.Result as ITableEntity);
                    Table.ExecuteAsync(deleteOp).Wait();
                }
            }
        }

        public void Dispose()
        {
            // ... clean up test data from the database ...
        }
    }

    public class AzureTableAliasRepositoryTest: IClassFixture<AzureTableAliasRepositoryFixture>
    {
        private const string skip = "Populate AzureTestCredentials.txt for this test";

        private readonly AzureTableAliasRepositoryFixture _fixture;
        private readonly AzureTableAliasRepository _subject;
        public AzureTableAliasRepositoryTest(AzureTableAliasRepositoryFixture fixture)
        {
            _fixture = fixture;
            _subject = new AzureTableAliasRepository(
                fixture.AzureStorageAccountName, fixture.AzureStorageAccessKey, fixture.TablePrefix);
        }

        [Fact(Skip = skip)]
        public async Task ShouldCreateTableWhenConstructed()
        {
            (await _fixture.Table.ExistsAsync()).Should().BeTrue();
        }

        [Fact(Skip = skip)]
        public async Task ShouldAddKeyValueEntity()
        {
            var key = "TheTestKey0";
            var value = "TheTestValue0";

            await _subject.Add(key, value);

            var retrieveOp = TableOperation.Retrieve(AzureTableAliasRepository.Partition, key);
            var result = await _fixture.Table.ExecuteAsync(retrieveOp);
            result.Should().NotBeNull();
            var entity = (ITableEntity)result.Result;
            entity.RowKey.Should().Be(key);
            var properties = entity.WriteEntity(new OperationContext());
            properties["Value"].StringValue.Should().Be(value);
        }

        [Fact(Skip = skip)]
        public async Task ShouldReportContainsKeyAfterAdding()
        {
            var key = "TheTestKey1";
            var value = "TheTestValue1";
            (await _subject.ContainsKey(key)).Should().BeFalse();

            await _subject.Add(key, value);

            (await _subject.ContainsKey(key)).Should().BeTrue();
        }

        [Fact(Skip = skip)]
        public async Task ShouldReportContainsValueAfterAdding()
        {
            var key = "TheTestKey2";
            var value = "TheTestValue2";
            (await _subject.ContainsValue(value)).Should().BeFalse();

            await _subject.Add(key, value);

            (await _subject.ContainsValue(value)).Should().BeTrue();
        }

        [Fact(Skip = skip)]
        public async Task ShouldRetrieveValueAfterAdding()
        {
            var key = "TheTestKey3";
            var value = "TheTestValue3";

            await _subject.Add(key, value);

            (await _subject.GetValue(key)).Should().Be(value);
        }

        [Fact(Skip = skip)]
        public async Task ShouldRetrieveKeyAfterAdding()
        {
            var key = "TheTestKey4";
            var value = "TheTestValue4";

            await _subject.Add(key, value);

            (await _subject.GetKey(value)).Should().Be(key);
        }

        [Fact(Skip = skip)]
        public async Task ShouldRemoveByKey()
        {
            var key = "TheTestKey5";
            var value = "TheTestValue5";
            await _subject.Add(key, value);
            (await _subject.ContainsKey(key)).Should().BeTrue();

            var didRemove = await _subject.Remove(key);

            (await _subject.ContainsKey(key)).Should().BeFalse();
            didRemove.Should().BeTrue();
        }

        [Fact(Skip = skip)]
        public async Task ShouldReturnFalseIfKeyNotPresentForRemoval()
        {
            var key = "TheTestKey6";
            (await _subject.ContainsKey(key)).Should().BeFalse();

            var didRemove = await _subject.Remove(key);

            didRemove.Should().BeFalse();
        }

        [Fact(Skip = skip)]
        public async Task ShouldPreventRemovalAfterLockTimeSpan()
        {
            _subject.LockAge = TimeSpan.FromSeconds(2);
            var key1 = "TheTestKey7";
            await _subject.Add(key1, "value");
            var key2 = "TheTestKey8";
            await _subject.Add(key2, "value8");
            (await _subject.Remove(key2)).Should().BeTrue("Can remove key that is young enough");
            Thread.Sleep(3000);

            try
            {
                await _subject.Remove(key1);
            }
            catch (Exception)
            {
                return;
            }

            Assert.True(false, "Should throw when trying to remove key that is too old");
        }
    }
}
