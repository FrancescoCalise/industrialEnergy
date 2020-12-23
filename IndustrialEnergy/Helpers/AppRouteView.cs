using IndustrialEnergy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Net;

namespace IndustrialEnergy.Helpers
{
    public class AppRouteView : RouteView
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IAuthenticationService AuthenticationService { get; set; }

        protected override void Render(RenderTreeBuilder builder)
        {
            var authorize = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute)) != null;
            if (authorize && !AuthenticationService.IsValidToken())
            {
                NavigationManager.NavigateTo(NavigationManager.BaseUri + "login");
            }
            else
            {
                base.Render(builder);
            }
        }
    }
}