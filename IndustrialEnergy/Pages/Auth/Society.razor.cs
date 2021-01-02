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
        public List<SocietyModel> Societies { get; set; }

        public async Task GetAllCurrentSociety()
        {
            if (Societies == null)
            {
                List<SocietyModel> societies = new List<SocietyModel>();

                var response = await _system.InvokeMiddlewareAsync("/Society", "/GetAllSocietiesByUser?UserId=" + _system.User.Id, null, _system.Headers, Method.GET, ToastModalityShow.No);
                ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Societies = JsonConvert.DeserializeObject<List<SocietyModel>>(responseContent.Content["Societies"]);
                }
            }
        }

    }
}
