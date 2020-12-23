using Blazored.LocalStorage;
using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{
    public interface IAuthenticationService
    {
        User User { get; }
        bool IsValidToken();
        Task Initialize();
        Task<IRestResponse> Login(string username, string password);
        Task Logout();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private NavigationManager _navigationManager;
        private IServiceComponent _serviceComponent;
        private ILocalStorageService _localStore;
        private IConfiguration _config;

        public User User { get; private set; }
        public string Token { get; private set; }

        public AuthenticationService(
            NavigationManager navigationManager,
            ILocalStorageService localStore,
            IServiceComponent serviceComponent,
            IConfiguration config
        )
        {
            _navigationManager = navigationManager;
            _serviceComponent = serviceComponent;
            _localStore = localStore;
            _config = config;
        }

        public async Task Initialize()
        {
            Token = await _localStore.GetItemAsync<string>("jwt");
        }

        public async Task<IRestResponse> Login(string username, string password)
        {
            string message = string.Empty;
            var response = _serviceComponent.ResponseJson(_navigationManager.BaseUri + "api/login?userId=" + username + "&pass=" + password + "", null, null, null, RestSharp.Method.GET);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Token = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content)["token"].ToString();
                await _localStore.SetItemAsync<string>("jwt", "Bearer " + Token);
            }

            return response;
        }

        public async Task Logout()
        {
            User = null;
            await _localStore.RemoveItemAsync("user");
            _navigationManager.NavigateTo("login");
        }

        public bool IsValidToken()
        {
            if (!string.IsNullOrEmpty(Token))
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();
                SecurityToken validatedToken;
                try
                {
                    IPrincipal principal = tokenHandler.ValidateToken(Token, validationParameters, out validatedToken);
                    if(validatedToken != null)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    //TODO LOG ERROR
                    return false;
                }
                
            }
            return false;
        }
        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true, 
                ValidateAudience = true, 
                ValidateIssuer = true,   
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]))
            };
        }
    }
}