using IndustrialEnergy.MongoDB;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrialEnergy.ServicesData
{

    public class MockupServiceData 
    {        
        public bool IsMockupEnabled { get; private set; }
      
        public MockupServiceData(IConfiguration config)
        {
            IsMockupEnabled = config["UseMockup"] == "true";
        }
    }

}
