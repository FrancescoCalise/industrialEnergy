using Blazored.LocalStorage;
using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Pages.Shared;
using IndustrialEnergy.UtilityClass;
using IndustrialEnergy.UtilityClass.Spinner;
using IndustrialEnergy.UtilityClass.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{
    public interface IAuthenticationService
    {
        User User { get; }
        bool IsValidToken { get; }
        Task CheckToken(bool authorize);
        Task<IRestResponse> Login(LoginUser login);
        Task<IRestResponse> SignUp(User user);
        Task Logout();
    }

    public class AuthenticationService : ComponentBase, IAuthenticationService
    {
        private NavigationManager _navigationManager;
        private IServiceComponent _serviceComponent;
        private ILocalStorageService _localStore;
        private IConfiguration _config;
        private ToastService _toastService;
        private TopMenuService _topNavBarService;

        public User User { get; private set; }
        public string Token { get; private set; }
        public bool IsValidToken { get; private set; }
        public AuthenticationService(
            NavigationManager navigationManager,
            ILocalStorageService localStore,
            IServiceComponent serviceComponent,
            IConfiguration config,
            ToastService toastService,
            TopMenuService topNavBarService
        )
        {
            _navigationManager = navigationManager;
            _serviceComponent = serviceComponent;
            _localStore = localStore;
            _config = config;
            _toastService = toastService;
            _topNavBarService = topNavBarService;
        }

        public async Task Initialize()
        {
            Token = await _localStore.GetItemAsync<string>("jwt");
            User = await _localStore.GetItemAsync<User>("user");
        }

        public async Task CheckToken(bool autorhize)
        {
            if (autorhize)
            {
                bool conteinsKey = await _localStore.ContainKeyAsync("jwt");

                if (string.IsNullOrEmpty(Token) && conteinsKey)
                {
                    await Initialize();
                }

                if (!string.IsNullOrEmpty(Token))
                {
                    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                    var validationParameters = GetValidationParameters();
                    SecurityToken validatedToken;
                    try
                    {

                        var converToken = Token.Contains("Bearer") ? Token.Substring(7) : Token;
                        IPrincipal principal = tokenHandler.ValidateToken(converToken, validationParameters, out validatedToken);
                        if (validatedToken == null)
                        {
                            _navigationManager.NavigateTo("login");
                            _topNavBarService.HideAutorize();
                        }
                        _topNavBarService.ShowAutorize();
                    }
                    catch (Exception ex)
                    {
                        await _localStore.RemoveItemAsync("jwt");
                        await _localStore.RemoveItemAsync("user");
                        Token = null;
                        User = null;
                        _toastService.ShowToast(HttpStatusCode.Unauthorized.ToString(), "Invalid Token", ToastLevel.Error);
                        //TODO LOG ERROR
                        _topNavBarService.HideAutorize();
                        _navigationManager.NavigateTo("login");
                    }
                }
                else
                {
                    _topNavBarService.HideAutorize();
                    _navigationManager.NavigateTo("login");
                }
            }
            else
            {
                _topNavBarService.HideAutorize();
            }

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

            if (response.StatusCode == HttpStatusCode.OK)
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