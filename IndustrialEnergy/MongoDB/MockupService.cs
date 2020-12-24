using IndustrialEnergy.MongoDB;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{

    public class MockupService 
    {        
        public bool IsMockupEnabled { get; private set; }
      
        public MockupService(IConfiguration config)
        {
            IsMockupEnabled = config["UseMockup"] == "true";
        }
    }

}
