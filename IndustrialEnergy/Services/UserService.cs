using IndustrialEnergy.Models;
using IndustrialEnergy.MongoDB;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{

    public interface IUserService
    {
        List<User> GetAllUser();
        User GetUserByUsername(string username);
    }

    public class UserService : IUserService
    {
        //private NavigationManager _navigationManager { get; set; }
        private IConfiguration _configuration { get; set; }
        private IMongoDatabase _mongoDBContex { get; set; }
        private bool isMockEnabled { get; set; }
        private const string pathFileMockup = "MongoDB/Mockup/userCollection.json";
        private string collectionName = "Users";

        public UserService(
            //NavigationManager navigationManager,
            IConfiguration configuration,
            MongoDBContext mongoDBContex,
            MockupService mockupService)
        {
            _configuration = configuration;
            //_navigationManager = navigationManager;
            _mongoDBContex= mongoDBContex._mongoDbContex;
            isMockEnabled = mockupService.IsMockupEnabled;
        }

        public List<User> GetAllUser()
        {
            List<User> users = new List<User>();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                users = JsonConvert.DeserializeObject<List<User>>(json);

            }
            else
            {

            }
            return users;
        }

        public User GetUserByUsername(string username)
        {
            User user = new User();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<User> users = JsonConvert.DeserializeObject<UsersCollection>(json).Users;
                user = users.Find(u => u.UserName == username);

            }
            else
            {
                //TODO TIRARE FUORI L'UTENTE DALLA COLLEZIONE DI MONGO
            }

            return user;
        }
    }

}
