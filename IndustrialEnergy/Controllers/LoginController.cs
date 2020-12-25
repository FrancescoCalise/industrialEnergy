using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialEnergy.Controllers
{
    [Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config { get; set; }
        private IUserService _userService { get; set; }

        public LoginController(
            IConfiguration config,
            IUserService userService)
        {
            _config = config;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string userId = "", string pass = "")
        {
            User login = new User()
            {
                UserName = userId,
                Password = pass
            };

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

        private IActionResult AuthenticateUser(User login)
        {
            User user = _userService.GetUserByUsername(login.UserName);
            IActionResult response = Unauthorized("User/password is wrong");

            if (user != null && !string.IsNullOrEmpty(user.Id))
            {
                //check pass
                //TODO add cryptography
                bool isPasswordCorrect = user.Password == login.Password;
                if (isPasswordCorrect)
                {
                    string tokenString = GenerateJSONWebToken(user);
                    response = Ok(tokenString);
                }
                else
                {
                    response = Unauthorized("User/password is wrong");
                }

            }

            return response;
        }

        [Authorize]
        [HttpPost]
        public IActionResult Post([FromBody] string value)
        {
            try
            {
                return Ok(value);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Authorize]
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
