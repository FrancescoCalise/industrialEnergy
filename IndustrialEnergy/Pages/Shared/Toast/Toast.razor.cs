using IndustrialEnergy.Services;
using IndustrialEnergy.Utility;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IndustrialEnergy.Components
{
    public class ToastComponent : ComponentBase, IDisposable
    {
        [Inject] private ToastService ToastService { get; set; }

        protected string Heading { get; set; }
        protected string Message { get; set; }
        protected bool IsVisible { get; set; }
        protected string BackgroundCssClass { get; set; }
        protected string IconCssClass { get; set; }

        protected override void OnInitialized()
        {
            ToastService.OnShow += ShowToast;
            ToastService.OnHide += HideToast;
        }

        private void ShowToast(string header, string message, ToastLevel level)
        {
            BuildToastSettings(header, level, message);
            IsVisible = true;
            new Timer(_ =>
            {
                InvokeAsync(StateHasChanged);
            }, null, 1000, 1000); 
        }

        private void HideToast()
        {
            IsVisible = false;
            new Timer(_ =>
            {
                InvokeAsync(StateHasChanged);
            }, null, 1000, 1000);
        }

        private void BuildToastSettings(string header, ToastLevel level, string message)
        {
            switch (level)
            {
                case ToastLevel.Info:
                    BackgroundCssClass = "bg-info";
                    IconCssClass = "info";
                    Heading = header;
                    break;
                case ToastLevel.Success:
                    BackgroundCssClass = "bg-success";
                    IconCssClass = "check";
                    Heading = header;
                    break;
                case ToastLevel.Warning:
                    BackgroundCssClass = "bg-warning";
                    IconCssClass = "exclamation";
                    Heading = header;
                    break;
                case ToastLevel.Error:
                    BackgroundCssClass = "bg-danger";
                    IconCssClass = "times";
                    Heading = header;
                    break;
            }

            Message = message;
        }

        public void Dispose()
        {
            ToastService.OnShow -= ShowToast;
        }
    }
}
