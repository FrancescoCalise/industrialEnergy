using System;
using System.Collections.Generic;
using System.Text;
using IndustrialEnergy.MongoDB.Collections.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace IndustrialEnergy.MongoDB.Collections.Services
{
   
    public class FirstCollectionService
    {
        public IMongoDatabase _mongoDBContex { get; set; }

        private string _collectionName = "firstCollection";
        private FirstCollection _firstCollection = new FirstCollection()
        {
            Name = "second",
            Age = 3
        };

        public FirstCollectionService(MongoDBContext MongoDBContex)
        {
            _mongoDBContex = MongoDBContex._mongoDbContex;
        }

        public void Save(FirstCollection firstCollection)
        {
            var collection = _mongoDBContex.GetCollection<FirstCollection>(_collectionName);

            if(firstCollection == null)
            {
                firstCollection = _firstCollection;
            }

            collection.InsertOne(firstCollection);


        }
    }
}


