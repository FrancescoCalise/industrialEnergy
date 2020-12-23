using Blazored.LocalStorage;
using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{
    public interface IAuthenticationService
    {
        User User { get; }
        Task Initialize();
        Task<string> Login(string username, string password);
        Task Logout();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private NavigationManager _navigationManager;
        private IServiceComponent _serviceComponent;
        private ILocalStorageService _localStore;

        public User User { get; private set; }

        public AuthenticationService(
            NavigationManager navigationManager,
            ILocalStorageService localStore,
            IServiceComponent serviceComponent
        )
        {
            _navigationManager = navigationManager;
            _serviceComponent = serviceComponent;
            _localStore = localStore;

        }

        public async Task Initialize()
        {
            User = await _localStore.GetItemAsync<User>("user");
        }

        public async Task<string> Login(string username, string password)
        {
            string message = string.Empty;
            var response = _serviceComponent.ResponseJson("http://localhost:61171/api/login?userId=" + username + "&pass=" + password + "", null, null, null, RestSharp.Method.GET);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content)["token"].ToString();
                await _localStore.SetItemAsync<string>("jwt", "Bearer " + token);

                message = "Create Token Success:" + response.StatusCode.ToString();
            }
            else
            {
                message = "Create Token Error:" + response.StatusCode.ToString();

            }

            return message;
        //    User = await _httpService.Post<User>("/users/authenticate", new { username, password });
        //    await _localStorageService.SetItem("user", User);
        }

        public async Task Logout()
        {
            User = null;
            await _localStore.RemoveItemAsync("user");
            _navigationManager.NavigateTo("login");
        }
    }
}