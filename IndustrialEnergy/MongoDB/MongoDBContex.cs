using System;
using IndustrialEnergy.MongoDB.Collections;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IndustrialEnergy.MongoDB
{
    public class MongoDBContext
    {
        private IMongoDatabase _mongoDB;

        public class ConnectionDBConfiguration
        {
            public string ConnectionString { get; set; }
            public string Name { get; set; }

        }

        public MongoDBContext(IConfiguration configuration)
        {
            
            ConnectionDBConfiguration _connectionDB = new ConnectionDBConfiguration()
            {
                ConnectionString = configuration.GetSection("MongoDB:ConnectionString").Value,
                Name = configuration.GetSection("MongoDB:Name").Value
            };

            //get the database
            var client = new MongoClient(_connectionDB.ConnectionString);
            _mongoDB = client.GetDatabase(_connectionDB.Name);

        }

        public IMongoDatabase GetMongo()
        {
            return _mongoDB;
        }

    }
}
