using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialEnergy.Services
{
    public static class SecurityService
    {
        public static string Encrypt(string value)
        {
            byte[] data = Encoding.ASCII.GetBytes(value);
            data = new SHA256Managed().ComputeHash(data);
            string hash = Encoding.ASCII.GetString(data);

            return hash;
        }
    }
}
