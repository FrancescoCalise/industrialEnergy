﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrialEnergy.UtilityClass.Toast
{
    public enum ToastLevel
    {
        Info,
        Success,
        Warning,
        Error
    }
    public enum ToastModalityShow
    {
        All,
        OnlySuccess,
        OnlyError,
    }
}
