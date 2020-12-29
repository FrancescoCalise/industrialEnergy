using IndustrialEnergy.Utility;
using System;
using System.Linq;
using System.Timers;

namespace IndustrialEnergy.Services
{
    public class ToastService : IDisposable
    {
        public event Action<string, string, ToastLevel> OnShow;
        public event Action OnHide;
        private Timer Countdown;

        public void ShowToast(string header, string message, ToastLevel level)
        {
            OnShow?.Invoke(header, message, level);
            StartCountdown();
        }

        private void StartCountdown()
        {
            SetCountdown();

            if (Countdown.Enabled)
            {
                Countdown.Stop();
                Countdown.Start();
            }
            else
            {
                Countdown.Start();
            }
        }

        private void SetCountdown()
        {
            if (Countdown == null)
            {
                Countdown = new Timer(3000);
                Countdown.Elapsed += HideToast;
                Countdown.AutoReset = false;
            }
        }

        private void HideToast(object source, ElapsedEventArgs args)
        {
            OnHide?.Invoke();
        }

        public void Dispose()
        {
            Countdown?.Dispose();
        }
    }
}
