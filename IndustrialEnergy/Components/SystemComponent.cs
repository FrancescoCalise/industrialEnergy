using Blazored.LocalStorage;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;

namespace IndustrialEnergy.Components
{
    public interface ISystemComponent
    {
        public SpinnerService SpinnerService { get; set; }
        public NavigationManager NavigationManager { get; set; }
        public ILocalStorageService LocalStore { get; set; }
        public IConfiguration Config { get; set; }
        public MenuService MenuService { get; set; }
        public UserModel User { get; set; }
        public string Token { get; set; }
        public bool IsValidToken { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        Task<IRestResponse> InvokeMiddlewareAsync(string controller, string action, object requestBody, Dictionary<string, string> requesteHeader, Method method);
        Task Init();
        Task ClearInit();
    }

    public class SystemComponent : ISystemComponent
    {
        public SpinnerService SpinnerService { get; set; }
        public NavigationManager NavigationManager { get; set; }
        public ILocalStorageService LocalStore { get; set; }
        public IConfiguration Config { get; set; }
        public MenuService MenuService { get; set; }
        public UserModel User { get; set; }
        public string Token { get; set; }
        public bool IsValidToken { get; set; }
        private string uri = string.Empty;
        public Dictionary<string, string> Headers { get; set; }

        public SystemComponent(
            SpinnerService spinnerService,
            NavigationManager navigationManager,
            MenuService menuService,
            ILocalStorageService localStore,
            IConfiguration config
            )
        {
            SpinnerService = spinnerService;
            NavigationManager = navigationManager;
            Config = config;
            LocalStore = localStore;
            MenuService = menuService;
            uri = NavigationManager.BaseUri + "api";

        }

        public async Task<IRestResponse> InvokeMiddlewareAsync(string controller, string action, object requestBody, Dictionary<string, string> requesteHeader, Method method)
        {
            SpinnerService.ShowSpinner();
            string resource = uri + controller + action;
            RestClient client = new RestClient();

            var request = new RestRequest(resource, method, DataFormat.Json);

            request.JsonSerializer = new CamelCaseSerializer();


            if (requesteHeader != null)
            {
                foreach (var item in requesteHeader)
                {
                    request.AddHeader(item.Key, item.Value);
                }
            }

            if (requestBody != null)
            {
                request.AddJsonBody(requestBody);
            }

            IRestResponse response = await client.ExecuteAsync(request);

            ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

            SpinnerService.HideSpinner();

            return response;
        }

        public class CamelCaseSerializer : ISerializer
        {
            public string ContentType { get; set; }

            public CamelCaseSerializer()
            {
                ContentType = "application/json";
            }

            public string Serialize(object obj)
            {
                var camelCaseSetting = new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                string json = JsonConvert.SerializeObject(obj, camelCaseSetting);
                return json;

            }
        }

        public async Task Init()
        {
            if (string.IsNullOrEmpty(Token))
            {
                bool islocalSaved = await LocalStore.ContainKeyAsync("jwt");
                if (islocalSaved)
                {
                    Token = await LocalStore.GetItemAsync<string>("jwt");
                }
            }

            if (!string.IsNullOrEmpty(Token))
            {
                Headers = new Dictionary<string, string>();
                Headers.Add("Authorization", "Bearer " + Token);

                if (User == null)
                {
                    bool islocalSaved = await LocalStore.ContainKeyAsync("user");
                    if (islocalSaved)
                    {
                        User = await LocalStore.GetItemAsync<UserModel>("user");
                    }
                    else
                    {
                        User = await GetUserByToken();
                        User.Password = null;
                        await LocalStore.SetItemAsync<UserModel>("user", User);
                    }

                    MenuService.ShowAutorize();
                }
            }

        }

        private async Task<UserModel> GetUserByToken()
        {
            UserModel user = new UserModel();
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = TokenService.GetValidationParameters();
            SecurityToken validatedToken;
            try
            {
                var converToken = Token.Contains("Bearer") ? Token.Substring(7) : Token;
                IPrincipal principal = tokenHandler.ValidateToken(converToken, validationParameters, out validatedToken);
                if (validatedToken != null)
                {
                    var token = tokenHandler.ReadJwtToken(converToken);
                    var username = token.Claims.First(claim => claim.Type == "sub").Value;

                    var userResponse = await InvokeMiddlewareAsync("/Autenticate", "/GetUserByUsername", username, Headers, Method.POST);
                    if (userResponse.StatusCode == HttpStatusCode.OK)
                    {
                        user = JsonConvert.DeserializeObject<UserModel>(userResponse.Content);
                    }
                }
            }
            catch { }

            return user;
        }

        public async Task ClearInit()
        {
            Token = null;
            User = null;
            IsValidToken = false;
            Headers = null;
            MenuService.HideAutorize();
            await LocalStore.ClearAsync();
        }
    }

    public class ResponseContent
    {
        public string Message { get; set; }
        public Dictionary<string, string> Content { get; set; }

        public ResponseContent()
        {
        }

        public ResponseContent(string message)
        {
            Message = message;
        }

        public ResponseContent(string message, Dictionary<string, string> content)
        {
            Message = message;
            Content = content;
        }
    }
}