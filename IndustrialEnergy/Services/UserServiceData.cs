using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.MongoDB;
using IndustrialEnergy.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{

    public interface IUserService
    {
        Task<List<User>> GetAllUser();
        Task<User> GetUserByUsername(string username);
        Task<IActionResult> SaveUser(User user);
    }

    public class UserService : ControllerBase, IUserService
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
            _mongoDBContex = mongoDBContex._mongoDbContex;
            isMockEnabled = mockupService.IsMockupEnabled;
        }

        public async Task<List<User>> GetAllUser()
        {
            List<User> users = new List<User>();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                users = JsonConvert.DeserializeObject<List<User>>(json);

            }
            else
            {
                var collection = _mongoDBContex.GetCollection<User>(collectionName);
                IAsyncCursor<User> task = await collection.FindAsync(x => !string.IsNullOrEmpty(x.Id));
                users = await task.ToListAsync();
            }

            return users;
        }

        public async Task<User> GetUserByUsername(string username)
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
                try
                {
                    var collection = _mongoDBContex.GetCollection<User>(collectionName);
                    FindOptions<User> options = new FindOptions<User> { Limit = 1 };
                    IAsyncCursor<User> task = await collection.FindAsync(x => x.UserName.Equals(username), options);
                    List<User> list = await task.ToListAsync();
                    user = list.FirstOrDefault();

                }
                catch (Exception ex)
                {
                    //todo
                }
            }

            return user;
        }

        public async Task<IActionResult> SaveUser(User user)
        {
            ResponseContent message = new ResponseContent();
            IActionResult result;

            if (isMockEnabled)
            {

                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<User> users = JsonConvert.DeserializeObject<UsersCollection>(json).Users;

                if (users.Find(u => u.UserName == user.UserName) == null)
                {
                    user.Id = ObjectId.GenerateNewId().ToString();
                    user.Role = Role.User;
                    users.Add(user);
                    UsersCollection uc = new UsersCollection();
                    uc.Users = users;

                    string jsonUpdate = JsonConvert.SerializeObject(uc);
                    //write string to file
                    System.IO.File.WriteAllText(pathFileMockup, jsonUpdate);

                    //TODO IDML
                    message = new ResponseContent("user Adedd");
                    result = Ok(message);

                }
                else
                {
                    //TODO IDML
                    message = new ResponseContent("User alredy exist");
                    result = BadRequest(message);
                }

            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<User>(collectionName);

                    FindOptions<User> options = new FindOptions<User> { Limit = 1 };
                    IAsyncCursor<User> task = await collection.FindAsync(x => x.UserName.Equals(user.UserName), options);
                    List<User> list = await task.ToListAsync();
                    User userFind = list.FirstOrDefault();

                    if (userFind == null)
                    {
                        user.Role = Role.User;
                        await collection.InsertOneAsync(user);
                        //TODO IDML
                        message = new ResponseContent("user Adedd");
                        result = Ok(message);

                    }
                    else
                    {
                        //TODO IDML
                        message = new ResponseContent("User alredy exist");
                        result = BadRequest(message);
                    }
                }
                catch (Exception ex)
                {
                    message.Message = ex.Message;
                    result = BadRequest(message);
                }
            }

            return result;
        }


    }
}
