using IndustrialEnergy.Components;
using IndustrialEnergy.Models;
using IndustrialEnergy.Services;
using IndustrialEnergy.Utility;
using Microsoft.AspNetCore.Components;
using MongoDB.Bson;
using Newtonsoft.Json;
using OfficeOpenXml;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrialEnergy.Pages.Auth
{
    public class InstallationComponentBase : LayoutComponentBase
    {
        [Inject] private ISystemComponent _system { get; set; }

        public InstallationModel Model { get; set; }

        public async Task GetInstallationConfiguration(string SocietyId)
        {

            var response = await _system.InvokeMiddlewareAsync("/Installation", "/GetInstallationConfigurationBySocietyId?SocietyId=" + SocietyId, null, _system.Headers, Method.GET);

            try
            {
                ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Model = JsonConvert.DeserializeObject<InstallationModel>(responseContent.Content["Installation"]);
                }
            }
            catch (Exception ex)
            {

            }


        }

        public async Task<IRestResponse> SaveInstallation(InstallationModel installation)
        {
            var response = await _system.InvokeMiddlewareAsync("/Installation", "/SaveInstallation", installation, _system.Headers, Method.POST);

            return response;
        }
        public StrcutureSystem GetStrucutesByExcel()
        {
            StrcutureSystem strcuture = new StrcutureSystem();
            string filePath = "Files/ReportSmall.xlsx";

            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            strcuture.CogeneratorList = new List<Cogenerator>();
            using (ExcelPackage excel = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Where(p => p.Name.Contains("Small")).FirstOrDefault();
                if (worksheet != null)
                {
                    int totalColumn = worksheet.Dimension.End.Column;
                    int totalRow = worksheet.Dimension.End.Row;
                    Cogenerator cogenerator = new Cogenerator();
                    cogenerator.CogeneratorValue = new List<CogeneratoreValue>();

                    for (int row = 2; row <= totalRow; row++)
                    {
                        if (row == 2)
                        {
                            string name = worksheet.Cells[row, 2].Value.ToString();
                            var indexFirstChar = name.IndexOf('\'');
                            var firstSub = name.Substring(indexFirstChar + 1);
                            var indexEndChar = firstSub.IndexOf('\'');
                            cogenerator.ModelName = name.Substring(indexFirstChar, indexEndChar - 1);
                        }
                        else
                        {
                            CogeneratoreValue value = new CogeneratoreValue();

                            for (int col = 1; col <= totalColumn + 1; col++)
                            {
                                switch (col)
                                {
                                    case 1: value.DetectionDate = (DateTime)worksheet.Cells[row, col].Value; break;
                                    case 2: value.GeneratorePower = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case 3: value.Cosphi = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case 4: value.InstantPower = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case 5:
                                    case 7:
                                    case 9:
                                        {
                                            if (value.GeneratorCurrent == null)
                                            {
                                                value.GeneratorCurrent = new List<double>();
                                            }
                                            value.GeneratorCurrent.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                                        }
                                        break;
                                    case 6:
                                    case 8:
                                    case 10:
                                        {
                                            if (value.GeneratorVoltage == null)
                                            {
                                                value.GeneratorVoltage = new List<double>();
                                            }
                                            value.GeneratorVoltage.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                                        }
                                        break;
                                    default:
                                        {
                                            if (totalColumn + 1 == col)
                                            {
                                                cogenerator.CogeneratorValue.Add(value);
                                                value = new CogeneratoreValue();
                                            }
                                            break;
                                        }
                                }
                            }
                        }

                    }
                    if (cogenerator != null)
                    {
                        cogenerator.Id = ObjectId.GenerateNewId().ToString();
                        if (strcuture.CogeneratorList.Find(c => c.ModelName == cogenerator.ModelName) == null)
                            strcuture.CogeneratorList.Add(cogenerator);
                    }
                }
            }
            return strcuture;
        }
    }
}
