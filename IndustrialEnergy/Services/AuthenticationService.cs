using Blazored.LocalStorage;
using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{
    public interface IAuthenticationService
    {
        //Task CheckToken(bool authorize, Type pageType);
        Task CheckIfTokenIsValid();
        Task Initialize();
        Task<IRestResponse> Login(LoginUser login);
        Task<IRestResponse> SignUp(User user);
        Task Logout();
    }

    public class AuthenticationService : ComponentBase, IAuthenticationService
    {
        private ISystemComponent _system { get; set; }

        public AuthenticationService(ISystemComponent system)
        {
            _system = system;
        }

        public async Task Initialize()
        {
            _system.Token = await _system.LocalStore.GetItemAsync<string>("jwt");
            _system.User = await _system.LocalStore.GetItemAsync<User>("user");
        }

        public async Task CheckIfTokenIsValid()
        {
            _system.IsValidToken = false;
            bool conteinsKey = await _system.LocalStore.ContainKeyAsync("jwt");
            if (string.IsNullOrEmpty(_system.Token) && conteinsKey)
            {
                await Initialize();
            }

            if (!string.IsNullOrEmpty(_system.Token))
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();
                SecurityToken validatedToken;
                try
                {
                    var converToken = _system.Token.Contains("Bearer") ? _system.Token.Substring(7) : _system.Token;
                    IPrincipal principal = tokenHandler.ValidateToken(converToken, validationParameters, out validatedToken);
                    if (validatedToken != null)
                    {
                        _system.IsValidToken = true;
                        _system.MenuService.ShowAutorize();
                    }
                }
                catch (Exception ex)
                {
                    await _system.LocalStore.RemoveItemAsync("jwt");
                    await _system.LocalStore.RemoveItemAsync("user");
                    _system.Token = null;
                    _system.User = null;
                    _system.MenuService.HideAutorize();
                }     
            }
            if (!_system.IsValidToken) _system.MenuService.HideAutorize();
        }

        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _system.Config["Jwt:Issuer"],
                ValidAudience = _system.Config["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_system.Config["Jwt:Key"]))
            };
        }
        public async Task<IRestResponse> Login(LoginUser login)
        {
            login.Password = SecurityService.Encrypt(login.Password);

            var response = await _system.InvokeMiddlewareAsync("/Autenticate", "/Login", login, null, Method.POST, ToastModalityShow.OnlySuccess);
            ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _system.Token = responseContent.Content["Token"];
                _system.User = JsonConvert.DeserializeObject<User>(responseContent.Content["User"]);
                _system.IsValidToken = true;
                await _system.LocalStore.SetItemAsync<User>("user", _system.User);
                await _system.LocalStore.SetItemAsync<string>("jwt", "Bearer " + _system.Token);

                _system.ToastService.ShowToast("Login", "ok", ToastLevel.Success);
                _system.NavigationManager.NavigateTo("");
            }

            return response;
        }
        public async Task Logout()
        {
            _system.Token = null;
            _system.User = null;
            _system.IsValidToken = false;
            await _system.LocalStore.ClearAsync();
            _system.NavigationManager.NavigateTo("");
        }

        public async Task<IRestResponse> SignUp(User user)
        {
            LoginUser login = new LoginUser() { Username = user.UserName, Password = user.Password };
            user.Password = SecurityService.Encrypt(login.Password);

            var response = await _system.InvokeMiddlewareAsync("/Autenticate", "/Signup", user, null, Method.POST, ToastModalityShow.OnlyError);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                response = await Login(login);

            }
            return response;
        }
    }
}