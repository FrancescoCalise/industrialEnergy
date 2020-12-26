using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IndustrialEnergy.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AutenticateController : ControllerBase
    {
        private IConfiguration _config { get; set; }
        private IUserService _userService { get; set; }

        public AutenticateController(
            IConfiguration config,
            IUserService userService)
        {
            _config = config;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginUser login)
        {
            IActionResult response = AuthenticateUser(login);

            return response;
        }

        private string GenerateJSONWebToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role),

            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            var encodeToken = new JwtSecurityTokenHandler().WriteToken(token);

            return encodeToken;
        }

        private IActionResult AuthenticateUser(LoginUser login)
        {
            User user = _userService.GetUserByUsername(login.Username);
            //TODO IDML
            ResponseContent messageResponse = new ResponseContent() { Message = "User/password is wrong" };
            IActionResult response = Unauthorized(messageResponse);

            if (user != null && !string.IsNullOrEmpty(user.Id))
            {

                bool isPasswordCorrect = user.Password == login.Password;
                if (isPasswordCorrect)
                {
                    string tokenString = GenerateJSONWebToken(user);
                    var content = new Dictionary<string, string>();
                    content.Add("Token", tokenString);
                    messageResponse.Content = content;
                    //TODO IDML
                    messageResponse.Message = "Login Ok";
                    response = Ok(messageResponse);
                }


            }

            return response;
        }

        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                //invalidare il token bo?
                return Ok();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
