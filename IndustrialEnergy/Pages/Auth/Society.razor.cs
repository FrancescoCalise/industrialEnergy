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
    public class SocietyComponentBase : ComponentBase
    {
        [Inject] private ISystemComponent _system { get; set; }
        public List<Society> Societies { get; set; }

        public async Task GetAllCurrentSociety()
        {
            if (Societies == null)
            {
                List<Society> societies = new List<Society>();
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Authorization", _system.Token);

                var response = await _system.InvokeMiddlewareAsync("/Society", "/GetAllSocietiesByUser", null, headers, Method.GET, ToastModalityShow.No);
                ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Societies = JsonConvert.DeserializeObject<List<Society>>(responseContent.Content["Societies"]);
                }
            }
        }
       
    }
}
