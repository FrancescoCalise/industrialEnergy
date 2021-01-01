﻿using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
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
        private ISocietyService _societyService { get; set; }

        public SocietyController(
            ISocietyService societyService)
        {
            _societyService = societyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSocietiesByUser()
        {
            //TODO IDML
            ResponseContent messageResponse = new ResponseContent() { Message = "Imposibile to get all societies" };
            IActionResult response = Unauthorized(messageResponse);
            var societies = await _societyService.GetAllSocietiesByUser();

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
        public async Task<IActionResult> SaveSociety([FromBody] Society society)
        {
            return await _societyService.SaveSociety(society);
        }

    }
}
