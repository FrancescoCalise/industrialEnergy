using System;
using System.Collections.Generic;
using System.Text;
using IndustrialEnergy.MongoDB.Collections.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace IndustrialEnergy.MongoDB.Collections.Services
{
   
    public class FirstCollectionService
    {
        private IConfiguration _configuration { get; }
        private string _collectionName = "firstCollection";
        private FirstCollection _firstCollection = new FirstCollection()
        {
            Name = "second",
            Age = 3
        };

        public FirstCollectionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Save(FirstCollection firstCollection)
        {

            MongoDBContext DBContex = new MongoDBContext(_configuration);
            IMongoDatabase contex = DBContex.GetMongo();
            var collection = contex.GetCollection<FirstCollection>(_collectionName);

            if(firstCollection == null)
            {
                firstCollection = _firstCollection;
            }

            collection.InsertOne(firstCollection);


        }
    }
}


