using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IndustrialEnergy.Models
{
    public class Sensor
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string InstallationId { get; set; }

        public string Model { get; set; }
        public string InstallationArea { get; set; }
        public string Father { get; set; }
        public List<SensorValue> sensorValueList { get; set; }
    }

    public class SensorValue
    {
        public DateTime DetectionDate { get; set; }
        public double DeltaTemperature { get; set; }
        public double Energy { get; set; }
        public double M3Instant { get; set; }
        public double InstantPower { get; set; }
        public double TemperatureSend { get; set; }
        public double TemperatureBack { get; set; }
        public double M3Total { get; set; }
    }
}