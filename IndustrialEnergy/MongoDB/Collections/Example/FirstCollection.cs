using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IndustrialEnergy.MongoDB.Collections.Models
{
    public class FirstCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

    }

    
    
}


