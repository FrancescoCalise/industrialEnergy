using Blazored.LocalStorage;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using IndustrialEnergy.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IndustrialEnergy.Components
{
    public interface ISystemComponent
    {
        public SpinnerService SpinnerService { get; set; }
        public ToastService ToastService { get; set; }
        public NavigationManager NavigationManager { get; set; }
        public IServiceComponent ServiceComponenet { get; set; }
        public ILocalStorageService LocalStore { get; set; }
        public IConfiguration Config { get; set; }
        public MenuService MenuService { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
    }

    public class SystemComponent : ISystemComponent
    {
        public SpinnerService SpinnerService { get; set; }
        public ToastService ToastService { get; set; }
        public NavigationManager NavigationManager { get; set; }
        public IServiceComponent ServiceComponenet { get; set; }
        public ILocalStorageService LocalStore { get; set; }
        public IConfiguration Config { get; set; }
        public MenuService MenuService { get; set; }
        public User User { get; set; }
        public string Token { get; set; }

        public SystemComponent(
            SpinnerService spinnerService,
            ToastService toastService,
            NavigationManager navigationManager,
            MenuService menuService,
            IServiceComponent serviceComponent,
            ILocalStorageService localStore,
            IConfiguration config
            )
        {
            SpinnerService = spinnerService;
            ToastService = toastService;
            NavigationManager = navigationManager;
            Config = config;
            LocalStore = localStore;
            ServiceComponenet = serviceComponent;
            MenuService = menuService;
        }

    }
}