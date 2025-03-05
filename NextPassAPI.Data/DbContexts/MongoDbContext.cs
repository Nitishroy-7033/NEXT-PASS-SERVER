using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Security.Authentication;

namespace NextPassAPI.Data.DbContexts
{
    public class MongoDbContext<T>
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbConfigs> mongoDBSettings, string? userConnectionString = null, string? userDatabaseName = null)
        {
            string connectionString = userConnectionString ?? mongoDBSettings.Value.ConnectionString;
            string databaseName = userDatabaseName ?? mongoDBSettings.Value.DatabaseName;

            _client = CreateMongoClient(connectionString);
            _database = _client.GetDatabase(databaseName);
        }

        private IMongoClient CreateMongoClient(string connectionString)
        {
            try
            {
                var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
                clientSettings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
                return new MongoClient(clientSettings);
            }
            catch (TimeoutException ex)
            {
                throw new InvalidOperationException("Connection to MongoDB server timed out.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while connecting to MongoDB.", ex);
            }
        }

        public IMongoCollection<T> GetCollection() => _database.GetCollection<T>(GetCollectionName(typeof(T).Name));

        private static string GetCollectionName(string entityName)
        {
            return entityName.EndsWith("y", StringComparison.OrdinalIgnoreCase)
                ? entityName.Substring(0, entityName.Length - 1) + "ies"
                : entityName + "s";
        }
    }
}
