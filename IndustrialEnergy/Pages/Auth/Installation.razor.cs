using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using IndustrialEnergy.Utility;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrialEnergy.Pages.Auth
{
    public class InstallationComponentBase : ComponentBase
    {
        [Inject] private ISystemComponent _system { get; set; }

        public InstallationModel Installations { get; set; }

        public async Task GetInstallationConfiguration(string SocietyId)
        {

            var response = await _system.InvokeMiddlewareAsync("/Installation", "/GetInstallationConfigurationBySocietyId?SocietyId=" + SocietyId, null, _system.Headers, Method.GET);
            ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Installations = JsonConvert.DeserializeObject<InstallationModel>(responseContent.Content["Installation"]);
            }
        }

        public async Task<IRestResponse> SaveInstallation(InstallationModel installation)
        {
            var response = await _system.InvokeMiddlewareAsync("/Installation", "/SaveInstallation", installation, _system.Headers, Method.POST);

            return response;
        }

    }
}
