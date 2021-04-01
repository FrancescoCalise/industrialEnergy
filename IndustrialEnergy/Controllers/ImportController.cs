using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using IndustrialEnergy.ServicesData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialEnergy.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private IInstallationServiceData _installationServiceData { get; set; }
        private ISocietyServiceData _societyServiceData { get; set; }
        public ImportController(
            IInstallationServiceData installationService,
            ISocietyServiceData societyServiceData
            )
        {
            _installationServiceData = installationService;
            _societyServiceData = societyServiceData;
        }

        [HttpPost]
        public async Task<IActionResult> SaveValues([FromBody] object values, string SocietyName)
        {
            var json = values.ToString();
            StrcutureSystem strcutureValues = JsonConvert.DeserializeObject<StrcutureSystem>(json);

            if (strcutureValues == null)
            {
                throw new Exception($"Values is missing");
            }

            if (string.IsNullOrEmpty(SocietyName))
            {
                throw new Exception($"Society Name is missing");
            }

            SocietyModel society = await _societyServiceData.GetSocietyByName(SocietyName);

            if (society == null)
            {
                throw new Exception($"Society NOT FOUND!!");
            }

            InstallationModel installation = await _installationServiceData.GetInstallationBySocietyId(society.Id);

            if (installation == null)
            {
                //Add
                installation = new InstallationModel { SocietyId = society.Id, Strucutres = strcutureValues };
            }
            else
            {
                // update
                installation.Strucutres = UpdateStructure(installation.Strucutres, strcutureValues);
            }

            ResponseContent messageResponse = new ResponseContent() { Message = "Error saving the company" };
            IActionResult response = BadRequest(messageResponse);

            var message = await _installationServiceData.SaveInstallation(installation);

            if (message != null)
            {
                var dic = new Dictionary<string, string>();
                dic.Add("installation", JsonConvert.SerializeObject(values));

                messageResponse = new ResponseContent("Save completed", dic);
                response = Ok(messageResponse);
            }

            return response;
        }

        private StrcutureSystem UpdateStructure(StrcutureSystem lastStrucutre, StrcutureSystem newStrucutre)
        {
            if (lastStrucutre == null || newStrucutre == null) throw new Exception("structures fields are mandatory");

            foreach (Boiler machine in newStrucutre.BoilerList)
            {
                var lastMachine = lastStrucutre.BoilerList.Where(x => x.ModelName == machine.ModelName).FirstOrDefault();
                if (lastMachine != null)
                {
                    foreach (BoilerValue value in machine.BoilerValue)
                    {
                        lastMachine.BoilerValue.Add(value);
                    }
                }
                else
                {
                    lastStrucutre.BoilerList.Add(machine);
                }
            }
            foreach (Cogenerator machine in newStrucutre.CogeneratorList)
            {
                var lastMachine = lastStrucutre.CogeneratorList.Where(x => x.ModelName == machine.ModelName).FirstOrDefault();
                if (lastMachine != null)
                {
                    foreach (CogeneratoreValue value in machine.CogeneratorValue)
                    {
                        lastMachine.CogeneratorValue.Add(value);
                    }
                }
                else
                {
                    lastStrucutre.CogeneratorList.Add(machine);
                }
            }
            foreach (HeatPump machine in newStrucutre.HeatPumpList)
            {
                var lastMachine = lastStrucutre.HeatPumpList.Where(x => x.ModelName == machine.ModelName).FirstOrDefault();
                if (lastMachine != null)
                {
                    foreach (HeatPumpValue value in machine.HeatPumpValue)
                    {
                        lastMachine.HeatPumpValue.Add(value);
                    }
                }
                else
                {
                    lastStrucutre.HeatPumpList.Add(machine);
                }
            }
            foreach (Sensor machine in newStrucutre.SensorList)
            {
                var lastSensor = lastStrucutre.SensorList.Where(x => x.ModelName == machine.ModelName).FirstOrDefault();
                if (lastSensor != null)
                {
                    if (machine.EnergyMeterList != null && machine.EnergyMeterList.Count > 0)
                    {
                        foreach (var energyMeter in machine.EnergyMeterList)
                        {

                            lastSensor.EnergyMeterList.Add(energyMeter);
                        }
                    }
                    if (machine.PumpSensorList != null && machine.PumpSensorList.Count > 0)
                    {
                        foreach (var pumpSensor in machine.PumpSensorList)
                        {
                            lastSensor.PumpSensorList.Add(pumpSensor);
                        }
                    }
                }
                else
                {
                    lastStrucutre.SensorList.Add(machine);
                }
            }
            return lastStrucutre;
        }
    }
}