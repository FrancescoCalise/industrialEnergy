﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IndustrialEnergy.Models
{
    public class Boiler
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string InstallationId { get; set; }
        public string ModelName { get; set; }
        public List<Sensor> SensorList { get; set; }
    }
}