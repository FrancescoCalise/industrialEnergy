using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Net;
using System.Threading.Tasks;

namespace IndustrialEnergy.AppRouteAuth
{
    public class AppRouteView : RouteView
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ISystemComponent System { get; set; }

        protected override void Render(RenderTreeBuilder builder)
        {

            var authorize = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute)) != null;
            if (!System.IsValidToken && authorize)
            {
                NavigationManager.NavigateTo("login");
            }
            base.Render(builder);

        }
    }
}