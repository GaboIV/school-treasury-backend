using Domain.Entities;
using Gabonet.Hubble.Extensions;
using Gabonet.Hubble.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System;

namespace Infrastructure.Persistence
{
    public class MongoDbContext
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _database;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MongoDbContext(string connectionString, string databaseName, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is missing");
            
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException(nameof(databaseName), "MongoDB database name is missing");

            var settings = MongoClientSettings.FromConnectionString(connectionString);
            
            settings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                {
                    // Capture the query for Hubble
                    if (_httpContextAccessor.HttpContext != null)
                    {
                        var query = new DatabaseQueryLog(
                            databaseType: "MongoDB",
                            databaseName: databaseName,
                            query: e.Command.ToJson(),
                            parameters: null,
                            callerMethod: MongoDbExtensions.GetCallerMethod(),
                            tableName: e.Command.GetCollectionName(),
                            operationType: e.CommandName
                        );
                        
                        _httpContextAccessor.HttpContext.AddDatabaseQuery(query);
                    }
                });
            };

            _client = new MongoClient(settings);
            _database = _client.GetDatabase(databaseName);
        }

        public IMongoClient GetMongoClient() => _client;
        public IMongoDatabase GetMongoDatabase() => _database;

        // Métodos para obtener colecciones específicas
        public IMongoCollection<Student> Students => _database.GetCollection<Student>("Students");
        public IMongoCollection<StudentPayment> StudentPayments => _database.GetCollection<StudentPayment>("StudentPayments");
        public IMongoCollection<Collection> Collections => _database.GetCollection<Collection>("Collections");
        public IMongoCollection<CollectionType> CollectionTypes => _database.GetCollection<CollectionType>("CollectionTypes");
        public IMongoCollection<Expense> Expenses => _database.GetCollection<Expense>("Expenses");
        public IMongoCollection<PettyCash> PettyCash => _database.GetCollection<PettyCash>("PettyCash");
        public IMongoCollection<TransactionLog> TransactionLogs => _database.GetCollection<TransactionLog>("TransactionLogs");
        public IMongoCollection<Transaction> Transactions => _database.GetCollection<Transaction>("Transactions");
        public IMongoCollection<InterestLink> InterestLinks => _database.GetCollection<InterestLink>("InterestLinks");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    }
} 