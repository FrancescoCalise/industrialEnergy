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
        private string filePath = "Files/ReportSmall.xlsx";

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
            installation.Strucutres = GetStrucutesByExcel();

            var response = await _system.InvokeMiddlewareAsync("/Installation", "/SaveInstallation", installation, _system.Headers, Method.POST);

            return response;
        }

        public StrcutureSystem GetStrucutesByExcel()
        {
            StrcutureSystem structure = new StrcutureSystem();

            structure.CogeneratorList = GetCogeneratorListFormExcel();
            structure.AbsorberList = new List<Absorber>();
            structure.BoilerList = new List<Boiler>();
            structure.HeatPumpList = new List<HeatPump>();

            structure = SetSensorListInStrucutreFormExcel(structure);

            return structure;
        }

        private StrcutureSystem SetSensorListInStrucutreFormExcel(StrcutureSystem structure)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<Sensor> sensorList = new List<Sensor>();

            using (ExcelPackage excel = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Where(p => p.Name.Contains("Sensor_Small")).FirstOrDefault();
                if (worksheet != null)
                {
                    int totalColumn = worksheet.Dimension.End.Column;
                    int totalRow = worksheet.Dimension.End.Row;
                    Sensor sensor = new Sensor();
                    sensor.sensorValueList = new List<SensorValue>();
                    List<SensorValue> valueList = SetDefaultSensorValueList(totalRow);
                    for (int col = 1; col <= totalColumn + 1; col++)
                    {
                        var sectionName = worksheet.Cells[2, col].Value != null ? worksheet.Cells[2, col].Value.ToString() : string.Empty;

                        if (!string.IsNullOrEmpty(sectionName))
                        {
                            LayoutColumn layout = GetLayout(sectionName);

                            if (layout.Value != "data_ora")
                            {
                                bool isNewSensor = sensor.Model != layout.Name || sensor.Father != layout.Father;

                                if (isNewSensor)
                                {
                                    if (col != 2)
                                    {
                                        sensor.sensorValueList = valueList;
                                        sensorList = AddSensorFound(sensorList, sensor);
                                        sensor = new Sensor();
                                    }
                                    sensor.Model = layout.Name;
                                    sensor.Father = layout.Father;
                                    sensor.InstallationArea = layout.Area;
                                }


                            }
                            int index = 0;
                            for (int row = 3; row <= totalRow; row++)
                            {

                                switch (layout.Value)
                                {
                                    case "data_ora": valueList[index].DetectionDate = (DateTime)worksheet.Cells[row, col].Value; break;
                                    case "DT": valueList[index].DeltaTemperature = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "EgH": valueList[index].Energy = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "FI": valueList[index].M3Instant = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "P": valueList[index].InstantPower = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "TFI": valueList[index].TemperatureSend = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "TRt": valueList[index].TemperatureBack = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "Vlm": valueList[index].M3Total = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                }
                                index++;
                            }
                        }
                        else
                        {
                            sensor.sensorValueList = valueList;
                            sensorList = AddSensorFound(sensorList, sensor);
                            break;
                        }
                    }
                }
            }
            if (sensorList.Count > 0)
            {
                
                foreach (Sensor s in sensorList)
                {
                    bool found = false;
                    var item = s.Father;
                    if (!found)
                    {
                        foreach (var i in structure.CogeneratorList)
                        {
                            if (i.ModelName == item)
                            {
                                structure.CogeneratorList.Find(c => c.ModelName == item).SensorList.Add(s);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        foreach (var i in structure.BoilerList)
                        {
                            if (i.ModelName == item)
                            {
                                structure.BoilerList.Find(c => c.ModelName == item).SensorList.Add(s);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        foreach (var i in structure.AbsorberList)
                        {
                            if (i.ModelName == item)
                            {
                                structure.AbsorberList.Find(c => c.ModelName == item).SensorList.Add(s);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        foreach (var i in structure.HeatPumpList)
                        {
                            if (i.ModelName == item)
                            {
                                structure.HeatPumpList.Find(c => c.ModelName == item).SensorList.Add(s);
                                found = true;
                                break;
                            }
                        }
                    }
                }
            }
            return structure;
        }
        private List<Cogenerator> GetCogeneratorListFormExcel()
        {
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            List<Cogenerator> cogenerators = new List<Cogenerator>();

            using (ExcelPackage excel = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Where(p => p.Name.Contains("Cogenerator_Small")).FirstOrDefault();
                if (worksheet != null)
                {
                    int totalColumn = worksheet.Dimension.End.Column;
                    int totalRow = worksheet.Dimension.End.Row;
                    Cogenerator cogenerator = new Cogenerator();
                    cogenerator.SensorList = new List<Sensor>();
                    cogenerator.CogeneratorValue = new List<CogeneratoreValue>();
                    List<CogeneratoreValue> valueList = SetDefaultCogeneratorValueList(totalRow);

                    for (int col = 1; col <= totalColumn + 1; col++)
                    {
                        var sectionName = worksheet.Cells[2, col].Value != null ? worksheet.Cells[2, col].Value.ToString() : string.Empty;
                        var layout = GetLayout(sectionName);

                        if (!string.IsNullOrEmpty(sectionName))
                        {
                            if (sectionName != "data_ora")
                            {
                                cogenerator.InstallationArea = layout.Area;

                                if (string.IsNullOrEmpty(cogenerator.ModelName))
                                {
                                    cogenerator.ModelName = layout.Name;
                                }
                                else if (cogenerator.ModelName != layout.Name)
                                {
                                    cogenerator.CogeneratorValue = valueList;
                                    cogenerators = AddCogeneratorFound(cogenerators, cogenerator);

                                    cogenerator = new Cogenerator();
                                    cogenerator.SensorList = new List<Sensor>();
                                    cogenerator.ModelName = layout.Name;
                                }
                            }
                            int index = 0;
                            for (int row = 3; row <= totalRow; row++)
                            {

                                switch (layout.Value)
                                {
                                    case "data_ora": valueList[index].DetectionDate = (DateTime)worksheet.Cells[row, col].Value; break;
                                    case "PotGe": valueList[index].GeneratorePower = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "Cosfi": valueList[index].Cosphi = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    //case "PoIs": valueList[index].InstantPower = Convert.ToDouble(worksheet.Cells[row, col].Value); break;
                                    case "CoGeL1":
                                    case "CoGeL2":
                                    case "CoGeL3":
                                        {
                                            if (valueList[index].GeneratorCurrent == null)
                                            {
                                                valueList[index].GeneratorCurrent = new List<double>();
                                            }
                                            valueList[index].GeneratorCurrent.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                                        }
                                        break;
                                    case "TeGeL1":
                                    case "TeGeL2":
                                    case "TeGeL3":
                                        {
                                            if (valueList[index].GeneratorVoltage == null)
                                            {
                                                valueList[index].GeneratorVoltage = new List<double>();
                                            }
                                            valueList[index].GeneratorVoltage.Add(Convert.ToDouble(worksheet.Cells[row, col].Value));
                                        }
                                        break;

                                }
                                index++;
                            }
                        }
                        else
                        {
                            cogenerator.CogeneratorValue = valueList;
                            cogenerators = AddCogeneratorFound(cogenerators, cogenerator);
                            break;
                        }

                    }
                }
            }
            return cogenerators;
        }




        //OTHER FUNCTION
        private List<CogeneratoreValue> SetDefaultCogeneratorValueList(int totalRow)
        {
            var list = new List<CogeneratoreValue>();
            for (int i = 0; i < totalRow - 2; i++)
            {
                list.Add(new CogeneratoreValue());
            }
            return list;
        }
        private List<SensorValue> SetDefaultSensorValueList(int totalRow)
        {
            var list = new List<SensorValue>();
            for (int i = 0; i < totalRow - 2; i++)
            {
                list.Add(new SensorValue());
            }
            return list;
        }

        private List<Cogenerator> AddCogeneratorFound(List<Cogenerator> list, Cogenerator lastCoge)
        {
            if (list.Find(c => c.ModelName == lastCoge.ModelName) == null)
            {
                list.Add(lastCoge);
            }
            return list;
        }

        private List<Sensor> AddSensorFound(List<Sensor> list, Sensor lastCoge)
        {
            if (list.Find(c => c.Model == lastCoge.Model && c.Father == lastCoge.Father) == null)
            {
                list.Add(lastCoge);
            }
            return list;
        }
        private LayoutColumn GetLayout(string sectionName)
        {
            LayoutColumn layout = new LayoutColumn();

            if (!string.IsNullOrEmpty(sectionName))
            {
                if (sectionName == "data_ora")
                {
                    layout.Value = sectionName;
                }
                else
                {
                    var array = sectionName.Split('\'');

                    layout.Area = array[0];
                    layout.Type = array[1];
                    if (array[2].Contains('-'))
                    {
                        var split = array[2].Split('-');
                        layout.Name = split[0];
                        layout.Father = split[1];
                    }
                    else
                    {
                        layout.Name = array[2];

                    }
                    layout.Value = array[3];

                }
            }
            return layout;
        }

        private class LayoutColumn
        {
            public string Area { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Father { get; set; }
            public string Value { get; set; }
        }
    }
}
