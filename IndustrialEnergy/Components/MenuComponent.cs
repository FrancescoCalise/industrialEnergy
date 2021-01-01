using IndustrialEnergy.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrialEnergy.Components
{
    public class MenuComponent : ComponentBase
    {
        [Inject] private MenuService menuService { get; set; }

        protected bool IsAutorized { get; set; }
        protected override void OnInitialized()
        {
            menuService.OnShow += ShowAutorize;
            menuService.OnHide += HideAutorize;
        }

        private void ShowAutorize()
        {
            IsAutorized = true;
            StateHasChanged();
        }

        private void HideAutorize()
        {
            IsAutorized = false;
            StateHasChanged();
        }
    }
}
