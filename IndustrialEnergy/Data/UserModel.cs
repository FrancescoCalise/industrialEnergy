using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IndustrialEnergy.MongoDB.Collections.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

    }

    public class UserCollection : UserModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}


