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

        public string ModelName { get; set; }
        public string InstallationArea { get; set; }
        public List<string> UsedBy { get; set; }
        public List<EnergyMeter> EnergyMeterList { get; set; }
        public List<PumpSensor> PumpSensorList { get; set; }
    }

    public class PumpSensor
    {
        public List<string> LabelValues { get; set; }

        public DateTime DetectionDate { get; set; }
        public List<CommandPump> CommandValue { get; set; }
        public double TemperatureSend { get; set; }
        public double TemperatureBack { get; set; }
        public double TemperatureExternal { get; set; }
    }
    public class CommandPump
    {
        public string Father { get; set; }
        public double Value { get; set; }
    }
    public class EnergyMeter
    {
        public List<string> LabelValues { get; set; }
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