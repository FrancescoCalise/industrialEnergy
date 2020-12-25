using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace IndustrialEnergy.UtilityClass.Spinner
{
    public class SpinnerService 
    {
        public event Action OnShow;
        public event Action OnHide;

        public void ShowSpinner()
        {
            OnShow?.Invoke();
        }
        public void HideSpinner()
        {
            OnHide?.Invoke();
        }
    }
}
