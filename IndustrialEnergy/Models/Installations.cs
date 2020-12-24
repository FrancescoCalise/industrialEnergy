using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IndustrialEnergy.Models
{
    public class Installations
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonId]
        public string SocietyId { get; set; }
        public string Strucutres { get; set; }
    }
}


