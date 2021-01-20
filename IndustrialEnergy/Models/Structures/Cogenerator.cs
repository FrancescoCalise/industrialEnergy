using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IndustrialEnergy.Models
{
    public class Cogenerator
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string InstallationId { get; set; }

        public string ModelName { get; set; }
        public string InstallationArea { get; set; }
        public List<CogeneratoreValue> CogeneratorValue { get; set; }
    }

    public class CogeneratoreValue
    {
        public List<string> LabelValues { get; set; }
        public DateTime DetectionDate { get; set; }
        public double GeneratorePower { get; set; }
        public double Cosphi { get; set; }
        public List<double> GeneratorCurrent { get; set; }
        public List<double> GeneratorVoltage { get; set; }

    }
}