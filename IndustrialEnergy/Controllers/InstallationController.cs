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
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InstallationController : ControllerBase
    {
        private IInstallationServiceData _installationServiceData { get; set; }

        public InstallationController(
            IInstallationServiceData installationService
            )
        {
            _installationServiceData = installationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInstallationConfigurationBySocietyId(string societyId)
        {
            //TODO IDML
            ResponseContent messageResponse = new ResponseContent() { Message = "Imposibile to get all societies" };
            IActionResult response = Unauthorized(messageResponse);
            var installations = await _installationServiceData.GetInstallationBySocietyId(societyId);

            if (installations != null)
            {
                var content = new Dictionary<string, string>();
                content.Add("Installation", JsonConvert.SerializeObject(installations));
                messageResponse.Content = content;
                //TODO IDML
                messageResponse.Message = "Get Installation, Ok";
                response = Ok(messageResponse);
            }

            return response;
        }

        [HttpPost]
        public async Task<IActionResult> SaveInstallation([FromBody] object installation)
        {

            var json = installation.ToString();
            InstallationModel installationModel = JsonConvert.DeserializeObject<InstallationModel>(json);

            ResponseContent messageResponse = new ResponseContent() { Message = "Error saving the company" };
            IActionResult response = BadRequest(messageResponse);

            installation = await _installationServiceData.SaveInstallation(installationModel);

            if (installation != null)
            {
                var dic = new Dictionary<string, string>();
                dic.Add("installation", JsonConvert.SerializeObject(installation));

                messageResponse = new ResponseContent("Save completed", dic);
                response = Ok(messageResponse);
            }

            return response;
        }
    }
}
