using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IndustrialEnergy.Models
{
    public class Society
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonId]
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }

    }
}


