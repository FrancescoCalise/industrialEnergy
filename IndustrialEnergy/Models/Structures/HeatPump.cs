using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IndustrialEnergy.Models
{
    public class HeatPump
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string InstallationId { get; set; }
        public string ModelName { get; set; }
        public string InstallationArea { get; set; }
        public List<HeatPumpValue> HeatPumpValue { get; set; }
    }

    public class HeatPumpValue
    {
        public List<string> LabelValues { get; set; }
        public DateTime DetectionDate { get; set; }
        public double CurrentAbsorbed { get; set; }
        public double PompFlow { get; set; }
        public double InstantPower { get; set; }
    }
}