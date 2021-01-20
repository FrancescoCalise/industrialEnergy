using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IndustrialEnergy.Models
{
    public class InstallationModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string SocietyId { get; set; }
        [Required]
        public StrcutureSystem Strucutres { get; set; }
    }

    public class CollectionInstallation
    {
        public List<InstallationModel> Installations { get; set; }
    }

    public class StrcutureSystem
    {
        public List<Cogenerator> CogeneratorList { get; set; }
        public List<Boiler> BoilerList { get; set; }
        public List<Absorber> AbsorberList { get; set; }
        public List<HeatPump> HeatPumpList { get; set; }
        public List<Sensor> SensorList { get; set; }
    }
}


