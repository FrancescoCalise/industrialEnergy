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
    public class SocietyController : ControllerBase
    {
        private ISocietyServiceData _societyServiceData { get; set; }
        public SocietyController(
            ISocietyServiceData societyService
            )
        {
            _societyServiceData = societyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSocietiesByUser(string userId)
        {
            //TODO IDML
            ResponseContent messageResponse = new ResponseContent() { Message = "Imposibile to get all societies" };
            IActionResult response = Unauthorized(messageResponse);
            var societies = await _societyServiceData.GetAllSocietiesByUser(userId);

            if (societies != null)
            {
                var content = new Dictionary<string, string>();
                content.Add("Societies", JsonConvert.SerializeObject(societies));
                messageResponse.Content = content;
                //TODO IDML
                messageResponse.Message = "Get All Societies, Ok";
                response = Ok(messageResponse);
            }

            return response;
        }

        [HttpPost]
        public async Task<IActionResult> SaveSociety([FromBody] SocietyModel society)
        {

            ResponseContent messageResponse = new ResponseContent() { Message = "Error saving the company" };
            IActionResult response = BadRequest(messageResponse);

            society = await _societyServiceData.SaveSociety(society);

            if (society != null)
            {
                var dic = new Dictionary<string, string>();
                dic.Add("society", JsonConvert.SerializeObject(society));

                messageResponse = new ResponseContent("Save completed", dic);
                response = Ok(messageResponse);
            }

            return response;
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSociety([FromBody] SocietyModel society)
        {
            IActionResult response = await _societyServiceData.DeleteSociety(society);

            return response;
        }
    }
}
