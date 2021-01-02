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
        //Task CheckToken(bool authorize, Type pageType);
        Task CheckIfTokenIsValid();
        Task<IRestResponse> Login(LoginUser login);
        Task<IRestResponse> SignUp(UserModel user);
        Task Logout();
    }

    public class AuthenticationService : ComponentBase, IAuthenticationService
    {
        private ISystemComponent _system { get; set; }

        public AuthenticationService(ISystemComponent system)
        {
            _system = system;
        }

        public async Task CheckIfTokenIsValid()
        {
            _system.IsValidToken = false;

            if (!string.IsNullOrEmpty(_system.Token))
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = TokenService.GetValidationParameters();
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
                    await _system.ClearInit();
                }
            }
            if (!_system.IsValidToken) _system.MenuService.HideAutorize();
        }

        public async Task<IRestResponse> Login(LoginUser login)
        {
            login.Password = SecurityService.Encrypt(login.Password);

            var tokenResponse = await _system.InvokeMiddlewareAsync("/Autenticate", "/Login", login, null, Method.POST, ToastModalityShow.No);
            ResponseContent tokenResponseContent = JsonConvert.DeserializeObject<ResponseContent>(tokenResponse.Content);
            if (tokenResponse.StatusCode == HttpStatusCode.OK)
            {
                var token = tokenResponseContent.Content["Token"];
                try
                {
                    await _system.LocalStore.SetItemAsync<string>("jwt", token);
                    await _system.Init();
                }
                catch (Exception ex)
                {
                    var x = ex;
                }
            }
            _system.ToastService.ShowToast("Login", "ok", ToastLevel.Success);
            _system.NavigationManager.NavigateTo("");

            return tokenResponse;
        }
        public async Task Logout()
        {
            await _system.ClearInit();

            _system.NavigationManager.NavigateTo("");
        }

        public async Task<IRestResponse> SignUp(UserModel user)
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