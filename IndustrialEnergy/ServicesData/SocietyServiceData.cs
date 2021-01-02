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
        Task<IActionResult> SaveSociety(SocietyModel society);
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
                var collection = _mongoDBContex.GetCollection<SocietyModel>(collectionName);
                IAsyncCursor<SocietyModel> task = await collection.FindAsync(x => x.UserId == "");
                societies = await task.ToListAsync();
            }

            return societies;
        }

        public async Task<IActionResult> SaveSociety(SocietyModel society)
        {
            ResponseContent message = new ResponseContent();
            IActionResult result;

            if (isMockEnabled)
            {

                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<SocietyModel> societies = JsonConvert.DeserializeObject<SocietyCollection>(json).Societies;

                if (societies.Find(u => u.Name == society.Name) == null)
                {
                    society.UserId = "";
                    societies.Add(society);
                    SocietyCollection collection = new SocietyCollection();
                    collection.Societies = societies;

                    string jsonUpdate = JsonConvert.SerializeObject(collection);
                    //write string to file
                    System.IO.File.WriteAllText(pathFileMockup, jsonUpdate);

                    //TODO IDML
                    message = new ResponseContent("society added");
                    result = Ok(message);

                }
                else
                {
                    var lastSociety = societies.Find(u => u.Name == society.Name && u.UserId != null);
                    society.UserId = "";
                    society.Id = lastSociety.Id;
                    societies.Remove(lastSociety);
                    societies.Add(society);

                    //TODO IDML
                    message = new ResponseContent("society updated");
                    result = Ok(message);
                }

            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<SocietyModel>(collectionName);

                    FindOptions<SocietyModel> options = new FindOptions<SocietyModel> { Limit = 1 };
                    IAsyncCursor<SocietyModel> task = await collection.FindAsync(x => x.Name.Equals(society.Name), options);
                    List<SocietyModel> list = await task.ToListAsync();
                    SocietyModel societyFind = list.FirstOrDefault();

                    if (societyFind == null)
                    {
                        await collection.InsertOneAsync(societyFind);
                        //TODO IDML
                        message = new ResponseContent("society Adedd");
                        result = Ok(message);

                    }
                    else
                    {
                        societyFind.Name = society.Name;
                        societyFind.Contact = society.Contact;
                        var optionsAndReplace = new FindOneAndReplaceOptions<SocietyModel>
                        {
                            ReturnDocument = ReturnDocument.After
                        };
                        var up = await collection.FindOneAndReplaceAsync<SocietyModel>(u => u.Id == society.Id, society, optionsAndReplace);

                        //TODO IDML
                        message = new ResponseContent("Society updated");
                        result = Ok(message);
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
