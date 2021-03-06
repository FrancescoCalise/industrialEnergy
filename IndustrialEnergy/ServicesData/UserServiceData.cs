﻿using IndustrialEnergy.Components;
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

namespace IndustrialEnergy.ServicesData
{

    public interface IUserServiceData
    {
        Task<List<UserModel>> GetAllUser();
        Task<UserModel> GetUserByUsername(string username);
        Task<IActionResult> SaveUser(UserModel user);
    }

    public class UserServiceData : ControllerBase, IUserServiceData
    {
        //private NavigationManager _navigationManager { get; set; }
        private IConfiguration _configuration { get; set; }
        private IMongoDatabase _mongoDBContex { get; set; }
        private bool isMockEnabled { get; set; }
        private const string pathFileMockup = "MongoDB/Mockup/userCollection.json";
        private string collectionName = "Users";

        public UserServiceData(
            //NavigationManager navigationManager,
            IConfiguration configuration,
            MongoDBContext mongoDBContex,
            MockupServiceData mockupService)
        {
            _configuration = configuration;
            _mongoDBContex = mongoDBContex._mongoDbContex;
            isMockEnabled = mockupService.IsMockupEnabled;
        }

        public async Task<List<UserModel>> GetAllUser()
        {
            List<UserModel> users = new List<UserModel>();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                users = JsonConvert.DeserializeObject<List<UserModel>>(json);

            }
            else
            {
                var collection = _mongoDBContex.GetCollection<UserModel>(collectionName);
                IAsyncCursor<UserModel> task = await collection.FindAsync(x => !string.IsNullOrEmpty(x.Id));
                users = await task.ToListAsync();
            }

            return users;
        }

        public async Task<UserModel> GetUserByUsername(string username)
        {
            UserModel user = new UserModel();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<UserModel> users = JsonConvert.DeserializeObject<UsersCollection>(json).Users;
                user = users.Find(u => u.UserName == username);

            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<UserModel>(collectionName);
                    FindOptions<UserModel> options = new FindOptions<UserModel> { Limit = 1 };
                    IAsyncCursor<UserModel> task = await collection.FindAsync(x => x.UserName.Equals(username), options);
                    List<UserModel> list = await task.ToListAsync();
                    user = list.FirstOrDefault();

                }
                catch (Exception ex)
                {
                    //todo
                }
            }

            return user;
        }

        public async Task<IActionResult> SaveUser(UserModel user)
        {
            ResponseContent message = new ResponseContent();
            IActionResult result;

            if (isMockEnabled)
            {

                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<UserModel> users = JsonConvert.DeserializeObject<UsersCollection>(json).Users;

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
                    var collection = _mongoDBContex.GetCollection<UserModel>(collectionName);

                    FindOptions<UserModel> options = new FindOptions<UserModel> { Limit = 1 };
                    IAsyncCursor<UserModel> task = await collection.FindAsync(x => x.UserName.Equals(user.UserName), options);
                    List<UserModel> list = await task.ToListAsync();
                    UserModel userFind = list.FirstOrDefault();

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
