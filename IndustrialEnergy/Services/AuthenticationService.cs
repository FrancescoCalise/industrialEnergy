using Blazored.LocalStorage;
using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.UtilityClass.Toast;
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
        Task<IRestResponse> Login(LoginUser login);
        Task<IRestResponse> SignUp(User user);
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

        public  bool IsValidToken()
        {
            if (string.IsNullOrEmpty(Token))
            {
                //Token = await _localStore.GetItemAsync<string>("jwt");
                //User = await _localStore.GetItemAsync<string>("user");

            }
            if (!string.IsNullOrEmpty(Token))
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();
                SecurityToken validatedToken;
                try
                {
                    IPrincipal principal = tokenHandler.ValidateToken(Token, validationParameters, out validatedToken);
                    if (validatedToken != null)
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
        public async Task<IRestResponse> Login(LoginUser login)
        {
            login.Password = SecurityService.Encrypt(login.Password);

            var response = await _serviceComponent.InvokeMiddlewareAsync("/Autenticate", "/Login", login, null, Method.POST, ToastModalityShow.OnlySuccess);
            ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Token = responseContent.Content["Token"];
                User = JsonConvert.DeserializeObject<User>(responseContent.Content["User"]);
                await _localStore.SetItemAsync<User>("user", User);
                await _localStore.SetItemAsync<string>("jwt", "Bearer " + Token);
                _navigationManager.NavigateTo("");
            }

            return response;
        }

        public async Task Logout()
        {
            Token = null;
            await _localStore.RemoveItemAsync("Token");
            _navigationManager.NavigateTo("login");
        }

        public async Task<IRestResponse> SignUp(User user)
        {
            LoginUser login = new LoginUser() { Username = user.UserName, Password = user.Password };
            user.Password = SecurityService.Encrypt(login.Password);

            var response = await _serviceComponent.InvokeMiddlewareAsync("/Autenticate", "/Signup", user, null, Method.POST, ToastModalityShow.OnlyError);
            ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response = await Login(login);

            }


            return response;
        }
    }
}