using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace IndustrialEnergy.Services
{
    public class TopMenuService
    {
        public event Action OnShow;
        public event Action OnHide;

        public void ShowAutorize()
        {
            OnShow?.Invoke();
        }
        public void HideAutorize()
        {
            OnHide?.Invoke();
        }
    }
}
