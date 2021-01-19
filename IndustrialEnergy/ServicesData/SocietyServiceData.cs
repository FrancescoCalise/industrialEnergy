using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.MongoDB;
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

    public interface ISocietyServiceData
    {
        Task<List<SocietyModel>> GetAllSocietiesByUser(string userId);
        Task<SocietyModel> SaveSociety(SocietyModel society);
        Task<SocietyModel> GetSocietyByName(string name);
        Task<SocietyModel> GetSocietyById(string societyId);

        Task<IActionResult> DeleteSociety(SocietyModel society);
    }

    public class SocietyServiceData : ControllerBase, ISocietyServiceData
    {
        private IMongoDatabase _mongoDBContex { get; set; }
        private bool isMockEnabled { get; set; }
        private const string pathFileMockup = "MongoDB/Mockup/societyCollection.json";
        private string collectionName = "Societies";

        public SocietyServiceData(
            MongoDBContext mongoDBContex,
            MockupServiceData mockupService
            )
        {
            _mongoDBContex = mongoDBContex._mongoDbContex;
            isMockEnabled = mockupService.IsMockupEnabled;
        }

        public async Task<List<SocietyModel>> GetAllSocietiesByUser(string userId)
        {
            List<SocietyModel> societies = new List<SocietyModel>();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                societies = JsonConvert.DeserializeObject<SocietyCollection>(json).Societies;
                societies = societies.FindAll(s => s.UserId == userId);

            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<SocietyModel>(collectionName);
                    IAsyncCursor<SocietyModel> task = await collection.FindAsync(x => x.UserId == userId);
                    societies = await task.ToListAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            return societies;
        }
        public async Task<SocietyModel> GetSocietyByName(string name)
        {
            SocietyModel societyFuond = new SocietyModel();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<SocietyModel> societies = JsonConvert.DeserializeObject<SocietyCollection>(json).Societies;
                societyFuond = societies.Find(f => f.Name == name);
            }
            else
            {
                var collection = _mongoDBContex.GetCollection<SocietyModel>(collectionName);

                FindOptions<SocietyModel> options = new FindOptions<SocietyModel> { Limit = 1 };
                IAsyncCursor<SocietyModel> task = await collection.FindAsync(x => x.Name.Equals(name), options);
                List<SocietyModel> list = await task.ToListAsync();
                societyFuond = list.FirstOrDefault();
            }
            return societyFuond;
        }
        public async Task<SocietyModel> GetSocietyById(string societyId)
        {
            SocietyModel societyFuond = new SocietyModel();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<SocietyModel> societies = JsonConvert.DeserializeObject<SocietyCollection>(json).Societies;
                societyFuond = societies.Find(f => f.Id == societyId);
            }
            else
            {
                var collection = _mongoDBContex.GetCollection<SocietyModel>(collectionName);

                FindOptions<SocietyModel> options = new FindOptions<SocietyModel> { Limit = 1 };
                IAsyncCursor<SocietyModel> task = await collection.FindAsync(x => x.Id.Equals(societyId), options);
                List<SocietyModel> list = await task.ToListAsync();
                societyFuond = list.FirstOrDefault();
            }
            return societyFuond;
        }

        public async Task<SocietyModel> SaveSociety(SocietyModel society)
        {
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<SocietyModel> societies = JsonConvert.DeserializeObject<SocietyCollection>(json).Societies;
                var societyFound = await GetSocietyByName(society.Name);

                if (societyFound == null)
                {
                    society.Id = ObjectId.GenerateNewId().ToString();
                    societies.Add(society);
                }
                else if (societyFound.UserId == society.UserId)
                {
                    societies.Remove(societyFound);
                    society.Id = societyFound.Id;
                    societies.Add(society);
                }

                SocietyCollection collection = new SocietyCollection();
                collection.Societies = societies;
                string jsonUpdate = JsonConvert.SerializeObject(collection);
                //write string to file
                System.IO.File.WriteAllText(pathFileMockup, jsonUpdate);
            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<SocietyModel>(collectionName);
                    var societyFound = await GetSocietyByName(society.Name);

                    if (societyFound == null)
                    {
                        society.Id = ObjectId.GenerateNewId().ToString();
                        await collection.InsertOneAsync(society);

                    }
                    else if (society.UserId == societyFound.UserId)
                    {
                        society.Id = societyFound.Id;
                        var optionsAndReplace = new FindOneAndReplaceOptions<SocietyModel>
                        {
                            ReturnDocument = ReturnDocument.After
                        };
                        var up = await collection.FindOneAndReplaceAsync<SocietyModel>(u => u.Id == society.Id, society, optionsAndReplace);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return society;
        }
        public async Task<IActionResult> DeleteSociety(SocietyModel society)
        {
            ResponseContent message = new ResponseContent();
            IActionResult result = BadRequest();

            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<SocietyModel> societies = JsonConvert.DeserializeObject<SocietyCollection>(json).Societies;
                var societyFound = await GetSocietyByName(society.Name);

                if (societyFound != null)
                {
                    societies.Remove(societyFound);
                    message.Message = "1 row deleted";
                    result = Ok(message);
                }

                SocietyCollection collection = new SocietyCollection();
                collection.Societies = societies;
                string jsonUpdate = JsonConvert.SerializeObject(collection);
                //write string to file
                System.IO.File.WriteAllText(pathFileMockup, jsonUpdate);
            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<SocietyModel>(collectionName);
                    var societyFound = await GetSocietyByName(society.Name);

                    if (societyFound != null)
                    {
                        var deleteFilter = Builders<SocietyModel>.Filter.Where(f => f.Id == society.Id);
                        var count = await collection.DeleteOneAsync(deleteFilter);
                        message.Message = count.DeletedCount.ToString() + " row deleted";
                        result = Ok(message);

                    }
                }
                catch (Exception ex)
                {
                }
            }

            return result;
        }

    }
}
