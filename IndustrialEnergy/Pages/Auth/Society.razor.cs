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
    public class SocietyBase : ComponentBase
    {
        [Inject] private ISystemComponent _system { get; set; }
        public List<Society> Societies { get; set; }

        public async Task GetAllCurrentSociety()
        {
            if (Societies == null)
            {
                List<Society> societies = new List<Society>();

                var response = await _system.ServiceComponenet.InvokeMiddlewareAsync("/Society", "/GetAllSocietiesByUser", null, null, Method.GET, ToastModalityShow.No);
                ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Societies = JsonConvert.DeserializeObject<List<Society>>(responseContent.Content["Societies"]);
                }
            }
        }
       
    }
}
