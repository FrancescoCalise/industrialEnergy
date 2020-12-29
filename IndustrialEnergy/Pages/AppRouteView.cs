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
        public IAuthenticationService AuthenticationService { get; set; }

        protected override void Render(RenderTreeBuilder builder)
        {

            var authorize = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute)) != null;
            AuthenticationService.CheckToken(authorize);
            base.Render(builder);

        }
    }
}