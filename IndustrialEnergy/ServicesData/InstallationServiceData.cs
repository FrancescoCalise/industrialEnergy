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

    public interface IInstallationServiceData
    {
        Task<InstallationModel> GetInstallationBySocietyId(string societyId);
        Task<InstallationModel> SaveInstallation(InstallationModel installation);
        Task<StatusCodeResult> DeleteInstallation(string societyId);
    }

    public class InstallationServiceData : ControllerBase, IInstallationServiceData
    {
        private IMongoDatabase _mongoDBContex { get; set; }
        private bool isMockEnabled { get; set; }
        private const string pathFileMockup = "MongoDB/Mockup/installationsCollection.json";
        private string collectionName = "Installations";

        public InstallationServiceData(
            MongoDBContext mongoDBContex,
            MockupServiceData mockupService
            )
        {
            _mongoDBContex = mongoDBContex._mongoDbContex;
            isMockEnabled = mockupService.IsMockupEnabled;
        }

        public async Task<InstallationModel> GetInstallationBySocietyId(string societyId)
        {
            InstallationModel installation = new InstallationModel();
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                var installations = JsonConvert.DeserializeObject<CollectionInstallation>(json).Installations;
                installation = installations.FirstOrDefault(s => s.SocietyId == societyId);

            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<InstallationModel>(collectionName);
                    IAsyncCursor<InstallationModel> task = await collection.FindAsync(x => x.SocietyId == societyId);
                    installation = await task.FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }

            return installation;
        }
    
        public async Task<InstallationModel> SaveInstallation(InstallationModel installation)
        {
            if (isMockEnabled)
            {
                string json = System.IO.File.ReadAllText(pathFileMockup);
                List<InstallationModel> installations = JsonConvert.DeserializeObject<CollectionInstallation>(json).Installations;
                var installationFound = await GetInstallationBySocietyId(installation.SocietyId);

                if (installationFound == null)
                {
                    installation.Id = ObjectId.GenerateNewId().ToString();
                    FillIdInListStrcture(installation);
                    installations.Add(installation);
                }
                else if (installation.SocietyId == installationFound.SocietyId)
                {
                    installations.Remove(installationFound);
                    installation.Id = installationFound.Id;
                    installations.Add(installation);
                }

                CollectionInstallation collection = new CollectionInstallation();
                collection.Installations = installations;
                string jsonUpdate = JsonConvert.SerializeObject(collection);
                //write string to file
                System.IO.File.WriteAllText(pathFileMockup, jsonUpdate);
            }
            else
            {
                try
                {
                    var collection = _mongoDBContex.GetCollection<InstallationModel>(collectionName);
                    var installationFound = await GetInstallationBySocietyId(installation.SocietyId);

                    if (installationFound == null)
                    {
                        installation.Id = ObjectId.GenerateNewId().ToString();
                        FillIdInListStrcture(installation);

                        await collection.InsertOneAsync(installation);

                    }
                    else if (installation.SocietyId == installationFound.SocietyId)
                    {
                        installation.Id = installationFound.Id;
                        var optionsAndReplace = new FindOneAndReplaceOptions<InstallationModel>
                        {
                            ReturnDocument = ReturnDocument.After
                        };
                        var up = await collection.FindOneAndReplaceAsync<InstallationModel>(u => u.SocietyId == installation.SocietyId, installation, optionsAndReplace);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return installation;
        }

        private InstallationModel FillIdInListStrcture(InstallationModel installation)
        {
            for (var i = 0; i < installation.Strucutres.CogeneratorList.Count(); i++)
            {
                if (string.IsNullOrEmpty(installation.Strucutres.CogeneratorList[i].InstallationId))
                {
                    installation.Strucutres.CogeneratorList[i].InstallationId = installation.Id;
                }
            }

            for (var i = 0; i < installation.Strucutres.BoilerList.Count(); i++)
            {
                if (string.IsNullOrEmpty(installation.Strucutres.BoilerList[i].InstallationId))
                {
                    installation.Strucutres.BoilerList[i].InstallationId = installation.Id;
                }
            }

            for (var i = 0; i < installation.Strucutres.SensorList.Count(); i++)
            {
                if (string.IsNullOrEmpty(installation.Strucutres.SensorList[i].InstallationId))
                {
                    installation.Strucutres.SensorList[i].InstallationId = installation.Id;
                }
            }

            for (var i = 0; i < installation.Strucutres.HeatPumpList.Count(); i++)
            {
                if (string.IsNullOrEmpty(installation.Strucutres.HeatPumpList[i].InstallationId))
                {
                    installation.Strucutres.HeatPumpList[i].InstallationId = installation.Id;
                }
            }

            return installation;
        }

        public async Task<StatusCodeResult> DeleteInstallation(string societyId)
        {
            StatusCodeResult result = BadRequest();

            var installation = await GetInstallationBySocietyId(societyId);

            if (installation != null)
            {
                if (isMockEnabled)
                {
                    string json = System.IO.File.ReadAllText(pathFileMockup);
                    List<InstallationModel> installationModels = JsonConvert.DeserializeObject<CollectionInstallation>(json).Installations;
                    installationModels.Remove(installation);
                    result = Ok();

                    CollectionInstallation collection = new CollectionInstallation();
                    collection.Installations = installationModels;
                    string jsonUpdate = JsonConvert.SerializeObject(collection);
                    //write string to file
                    System.IO.File.WriteAllText(pathFileMockup, jsonUpdate);
                }
                else
                {
                    try
                    {
                        var collection = _mongoDBContex.GetCollection<InstallationModel>(collectionName);
                        var deleteFilter = Builders<InstallationModel>.Filter.Where(f => f.SocietyId == societyId);
                        var count = await collection.DeleteOneAsync(deleteFilter);
                        result = Ok();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else
            {
                result = Ok();
            }
            return result;
        }
    }
}
