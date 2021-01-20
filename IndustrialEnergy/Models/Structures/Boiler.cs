using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IndustrialEnergy.Models
{
    public class Boiler
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string InstallationId { get; set; }
        public string ModelName { get; set; }
        public string InstallationArea { get; set; }
        public List<BoilerValue> BoilerValue { get; set; }
    }

    public class BoilerValue
    {
        public List<string> LabelValues { get; set; }
        public DateTime DetectionDate { get; set; }
        public double SetPointTemperatureSend { get; set; }
        public double ModulationFlame { get; set; }
        public double BurnerState { get; set; }
        public double PtTemperatureSend { get; set; }
    }
}