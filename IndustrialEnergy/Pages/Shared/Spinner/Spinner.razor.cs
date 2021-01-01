using IndustrialEnergy.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrialEnergy.Components
{
    public class SpinnerComponenet : ComponentBase
    {
        [Inject] private SpinnerService spinnerService { get; set; }

        protected bool SpinnerIsVisible { get; set; }
        protected string DisplayCss { get; set; }
        protected override void OnInitialized()
        {
            spinnerService.OnShow += ShowSpinner;
            spinnerService.OnHide += HideSpinner;
        }

        private void ShowSpinner()
        {
            SpinnerIsVisible = true;
            StateHasChanged();
        }

        private void HideSpinner()
        {
            SpinnerIsVisible = false;
            StateHasChanged();
        }
    }
}
