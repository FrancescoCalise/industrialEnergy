using System;
using IndustrialEnergy.MongoDB.Collections;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IndustrialEnergy.MongoDB
{
    public class MongoDBContext
    {
        public IMongoDatabase _mongoDbContex;
        public class ConnectionDBConfiguration
        {
            public string ConnectionString { get; set; }
            public string Name { get; set; }

        }

        public MongoDBContext(IConfiguration Configuration)
        {
            
            ConnectionDBConfiguration _connectionDB = new ConnectionDBConfiguration()
            {
                ConnectionString = Configuration.GetSection("MongoDB:ConnectionString").Value,
                Name = Configuration.GetSection("MongoDB:Name").Value
            };

            //get the database
            var client = new MongoClient(_connectionDB.ConnectionString);
            _mongoDbContex = client.GetDatabase(_connectionDB.Name);

        }

    }
}
