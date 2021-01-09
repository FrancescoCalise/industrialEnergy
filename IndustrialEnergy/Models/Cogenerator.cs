using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace IndustrialEnergy.Models
{
    public class Cogenerator
    {
        
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string InstallationId { get; set; }
        public string ModelName { get; set; }
        //todo add all information 
    }
}