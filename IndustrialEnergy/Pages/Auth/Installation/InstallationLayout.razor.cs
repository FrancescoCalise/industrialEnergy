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
        public SocietyModel Society { get; set; }
        private string filePath = "Files/ReportFull.xlsx";

        public async Task GetSocietyById(string societyId)
        {
            if (!string.IsNullOrEmpty(societyId))
            {
                var response = await _system.InvokeMiddlewareAsync("/Society", "/GetSocietyById?SocietyId=" + societyId, null, _system.Headers, Method.GET);
                ResponseContent responseContent = JsonConvert.DeserializeObject<ResponseContent>(response.Content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Society = JsonConvert.DeserializeObject<SocietyModel>(responseContent.Content["Society"]);
                }
            }
        }
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
            installation.Strucutres = SetValuesByExcel();

            var response = await _system.InvokeMiddlewareAsync("/Installation", "/SaveInstallation", installation, _system.Headers, Method.POST);

            return response;
        }

        private StrcutureSystem SetValuesByExcel()
        {
            StrcutureSystem structure = GetStrucutesByExcel();

            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excel = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    int totalColumn = worksheet.Dimension.End.Column;
                    int totalRow = worksheet.Dimension.End.Row;
                    for (int col = 1; col <= totalColumn; col++)
                    {
                        int indexHeadingRow = 2; // è 2 perchè sto assumendo che l'intestazione sia nella seconda riga.

                        var valueColumn = worksheet.Cells[indexHeadingRow, col].Value != null ? worksheet.Cells[indexHeadingRow, col].Value.ToString() : string.Empty;
                        if (!string.IsNullOrEmpty(valueColumn))
                        {
                            var valueColumnSplitted = GetLayout(valueColumn);
                            string labelValue = valueColumnSplitted.Value;
                            string labelType = valueColumnSplitted.Type;

                            if (!string.IsNullOrEmpty(labelType))
                            {
                                for (int row = 3; row <= totalRow; row++)
                                {
                                    var value = worksheet.Cells[row, col].Value != null ? worksheet.Cells[row, col].Value.ToString() : string.Empty;
                                    if (value != null)
                                    {
                                        switch (labelType.ToLower())
                                        {
                                            case "data_ora": { structure = SetDateInAllStructure(structure, value); break; }
                                            case "cogenerator":
                                                {
                                                    structure = SetValueInCogeneratorValueList(valueColumnSplitted, structure, value, row);
                                                    break;
                                                }
                                            case "boiler":
                                                {
                                                    structure = SetValueInBoilerValueList(valueColumnSplitted, structure, value, row);
                                                    break;
                                                }
                                            case "heatpump":
                                                {
                                                    structure = SetValueInHeatPupValueList(valueColumnSplitted, structure, value, row);
                                                    break;
                                                }
                                            case "energymeter":
                                            case "pump":
                                                {
                                                    structure = SetValueInSensorValueList(valueColumnSplitted, structure, value, row);
                                                    break;
                                                }

                                            //todo absorber
                                            default: throw new Exception();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return structure;
        }

        private StrcutureSystem SetValueInSensorValueList(LayoutColumn valueColumnSplitted, StrcutureSystem structure, string value, int row)
        {
            var sensorFound = new Sensor();

            if (structure.SensorList.Count() > 0)
            {
                foreach (string usedBy in valueColumnSplitted.Father)
                {
                    sensorFound = structure.SensorList.Find(c => c.ModelName == valueColumnSplitted.ModelName && c.UsedBy.Contains(usedBy));
                    if (sensorFound != null) break;
                }
            }

            if (sensorFound != null)
            {
                if (valueColumnSplitted.Type == "energymeter")
                {
                    var valueBag = sensorFound.EnergyMeterList[row - 3];
                    if (valueBag != null)
                    {
                        switch (valueColumnSplitted.Value)
                        {
                            case "EgH": valueBag.Energy = Convert.ToDouble(value); break;
                            case "Fi": valueBag.M3Instant = Convert.ToDouble(value); break;
                            case "P": valueBag.InstantPower = Convert.ToDouble(value); break;
                            case "TFl": valueBag.TemperatureSend = Convert.ToDouble(value); break;
                            case "TRt": valueBag.TemperatureBack = Convert.ToDouble(value); break;
                            case "Vlm": valueBag.M3Total = Convert.ToDouble(value); break;
                            case "DT": valueBag.DeltaTemperature = Convert.ToDouble(value); break;
                        }
                    }
                }
                if (valueColumnSplitted.Type == "pump")
                {
                    var valueBag = sensorFound.PumpSensorList[row - 3];

                    if (valueBag != null)
                    {
                        switch (valueColumnSplitted.Value)
                        {
                            case "CMD":
                                {
                                    if (valueBag.CommandValue == null)
                                    {
                                        valueBag.CommandValue = new List<CommandPump>();

                                        CommandPump command = new CommandPump();
                                        command.Father = valueColumnSplitted.Father.FirstOrDefault(); // non funziona se ci sono piu padri
                                        command.Value = Convert.ToDouble(value);
                                        valueBag.CommandValue.Add(command);
                                    }
                                    else
                                    {
                                        var search = valueBag.CommandValue.Find(c => c.Father == valueColumnSplitted.Father.FirstOrDefault());
                                        if (search != null)
                                        {
                                            search.Value= Convert.ToDouble(value);
                                        }
                                        else
                                        {
                                            CommandPump command = new CommandPump();
                                            command.Father = valueColumnSplitted.Father.FirstOrDefault(); // non funziona se ci sono piu padri
                                            command.Value = Convert.ToDouble(value);
                                            valueBag.CommandValue.Add(command);
                                        }

                                    }

                                    break;
                                }
                            case "TMnFI": valueBag.TemperatureSend = Convert.ToDouble(value); break;
                            case "TMnRt": valueBag.TemperatureBack = Convert.ToDouble(value); break;
                            case "TOa": valueBag.TemperatureExternal = Convert.ToDouble(value); break;

                        }
                    }
                }


            }
            return structure;
        }

        private StrcutureSystem SetValueInHeatPupValueList(LayoutColumn valueColumnSplitted, StrcutureSystem structure, string value, int row)
        {
            var heatPump = structure.HeatPumpList.Find(c => c.ModelName == valueColumnSplitted.ModelName);
            if (heatPump != null)
            {
                var valueBag = heatPump.HeatPumpValue[row - 3];
                if (valueBag != null)
                {
                    switch (valueColumnSplitted.Value)
                    {
                        case "Amp": valueBag.CurrentAbsorbed = Convert.ToDouble(value); break;
                        case "FlRate": valueBag.PompFlow = Convert.ToDouble(value); break;
                        case "PwrLO": valueBag.InstantPower = Convert.ToDouble(value); break;
                    }
                }
            }
            return structure;
        }

        private StrcutureSystem SetValueInBoilerValueList(LayoutColumn valueColumnSplitted, StrcutureSystem structure, string value, int row)
        {
            var boilerFound = structure.BoilerList.Find(c => c.ModelName == valueColumnSplitted.ModelName);
            if (boilerFound != null)
            {
                var valueBag = boilerFound.BoilerValue[row - 3];
                if (valueBag != null)
                {
                    switch (valueColumnSplitted.Value)
                    {
                        case "TFl": valueBag.SetPointTemperatureSend = Convert.ToDouble(value); break;
                        case "Mod": valueBag.ModulationFlame = Convert.ToDouble(value); break;
                        case "PT1Te": valueBag.PtTemperatureSend = Convert.ToDouble(value); break;
                        case "StCBu": valueBag.BurnerState = Convert.ToDouble(value); break;
                    }
                }
            }
            return structure;
        }

        private StrcutureSystem SetDateInAllStructure(StrcutureSystem structure, string value)
        {
            if (structure.CogeneratorList.Count > 0)
            {
                foreach (var cogenerator in structure.CogeneratorList)
                {
                    if (cogenerator.CogeneratorValue.Count > 0)
                    {
                        if (cogenerator.CogeneratorValue.FirstOrDefault().DetectionDate == DateTime.MinValue)
                        {
                            cogenerator.CogeneratorValue.FirstOrDefault().DetectionDate = DateTime.Parse(value);
                        }
                        else
                        {
                            var backup = cogenerator.CogeneratorValue.FirstOrDefault();
                            CogeneratoreValue newBag = new CogeneratoreValue();
                            newBag.LabelValues = new List<string>();
                            newBag.LabelValues = backup.LabelValues;
                            newBag.DetectionDate = DateTime.Parse(value);
                            cogenerator.CogeneratorValue.Add(newBag);

                        }
                    }
                }
            }

            if (structure.SensorList.Count > 0)
            {
                foreach (var sensor in structure.SensorList)
                {
                    if (sensor.PumpSensorList != null && sensor.PumpSensorList.Count > 0)
                    {
                        if (sensor.PumpSensorList.FirstOrDefault().DetectionDate == DateTime.MinValue)
                        {
                            sensor.PumpSensorList.FirstOrDefault().DetectionDate = DateTime.Parse(value);
                        }
                        else
                        {
                            var backup = sensor.PumpSensorList.FirstOrDefault();
                            PumpSensor newBag = new PumpSensor();
                            newBag.LabelValues = new List<string>();
                            newBag.LabelValues = backup.LabelValues;
                            newBag.DetectionDate = DateTime.Parse(value);
                            sensor.PumpSensorList.Add(newBag);

                        }
                    }
                    if (sensor.EnergyMeterList != null && sensor.EnergyMeterList.Count > 0)
                    {
                        if (sensor.EnergyMeterList.FirstOrDefault().DetectionDate == DateTime.MinValue)
                        {
                            sensor.EnergyMeterList.FirstOrDefault().DetectionDate = DateTime.Parse(value);
                        }
                        else
                        {
                            var backup = sensor.EnergyMeterList.FirstOrDefault();
                            EnergyMeter newBag = new EnergyMeter();
                            newBag.LabelValues = new List<string>();
                            newBag.LabelValues = backup.LabelValues;
                            newBag.DetectionDate = DateTime.Parse(value);
                            sensor.EnergyMeterList.Add(newBag);

                        }
                    }
                }
            }

            if (structure.BoilerList.Count > 0)
            {
                foreach (var boiler in structure.BoilerList)
                {
                    if (boiler.BoilerValue.Count > 0)
                    {
                        if (boiler.BoilerValue.FirstOrDefault().DetectionDate == DateTime.MinValue)
                        {
                            boiler.BoilerValue.FirstOrDefault().DetectionDate = DateTime.Parse(value);
                        }
                        else
                        {
                            var backup = boiler.BoilerValue.FirstOrDefault();
                            BoilerValue newBag = new BoilerValue();
                            newBag.LabelValues = new List<string>();
                            newBag.LabelValues = backup.LabelValues;
                            newBag.DetectionDate = DateTime.Parse(value);
                            boiler.BoilerValue.Add(newBag);

                        }
                    }
                }
            }

            if (structure.HeatPumpList.Count > 0)
            {
                foreach (var heatPump in structure.HeatPumpList)
                {
                    if (heatPump.HeatPumpValue.Count > 0)
                    {
                        if (heatPump.HeatPumpValue.FirstOrDefault().DetectionDate == DateTime.MinValue)
                        {
                            heatPump.HeatPumpValue.FirstOrDefault().DetectionDate = DateTime.Parse(value);
                        }
                        else
                        {
                            var backup = heatPump.HeatPumpValue.FirstOrDefault();
                            HeatPumpValue newBag = new HeatPumpValue();
                            newBag.LabelValues = new List<string>();
                            newBag.LabelValues = backup.LabelValues;
                            newBag.DetectionDate = DateTime.Parse(value);
                            heatPump.HeatPumpValue.Add(newBag);

                        }
                    }
                }
            }

            //TODO ABSORBER

            return structure;
        }

        private StrcutureSystem SetValueInCogeneratorValueList(LayoutColumn valueColumnSplitted, StrcutureSystem structure, string value, int index)
        {
            var cogeFound = structure.CogeneratorList.Find(c => c.ModelName == valueColumnSplitted.ModelName);
            if (cogeFound != null)
            {
                var valueBag = cogeFound.CogeneratorValue[index - 3];
                if (valueBag != null)
                {
                    switch (valueColumnSplitted.Value)
                    {
                        case "PotGe": valueBag.GeneratorePower = Convert.ToDouble(value); break;
                        case "Cosfi": valueBag.Cosphi = Convert.ToDouble(value); break;
                        case "CoGeL1":
                        case "CoGeL2":
                        case "CoGeL3":
                            {
                                if (valueBag.GeneratorCurrent == null)
                                {
                                    valueBag.GeneratorCurrent = new List<double>();
                                }
                                valueBag.GeneratorCurrent.Add(Convert.ToDouble(value));
                            }
                            break;
                        case "TeGeL1":
                        case "TeGeL2":
                        case "TeGeL3":
                            {
                                if (valueBag.GeneratorVoltage == null)
                                {
                                    valueBag.GeneratorVoltage = new List<double>();
                                }
                                valueBag.GeneratorVoltage.Add(Convert.ToDouble(value));
                            }
                            break;
                    }
                }
            }
            return structure;
        }

        public StrcutureSystem GetStrucutesByExcel()
        {
            StrcutureSystem structure = SetEmptyStrcuture();
            FileInfo fileInfo = new FileInfo(filePath);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excel = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    int totalColumn = worksheet.Dimension.End.Column;
                    int totalRow = worksheet.Dimension.End.Row;
                    for (int i = 2; i <= totalColumn; i++)
                    {
                        //i = 2, perchè non mi serve controllare la colonna data_ora

                        int indexHeadingRow = 2; // è 2 perchè sto assumendo che l'intestazione sia nella seconda riga.
                        var value = worksheet.Cells[indexHeadingRow, i].Value != null ? worksheet.Cells[indexHeadingRow, i].Value.ToString() : string.Empty;

                        if (!string.IsNullOrEmpty(value))
                        {
                            var valueSplitted = GetLayout(value);
                            switch (valueSplitted.Type.ToLower())
                            {
                                case "cogenerator":
                                    {
                                        structure = SetCogeneratorList(valueSplitted, structure);
                                        break;
                                    }
                                case "boiler":
                                    {
                                        structure = SetBoilerList(valueSplitted, structure);
                                        break;
                                    }
                                case "energymeter":
                                case "pump":
                                    {
                                        structure = SetSensorList(valueSplitted, structure);
                                        break;
                                    }
                                case "heatpump":
                                    {
                                        structure = SetHeatPump(valueSplitted, structure);
                                        break;
                                    }
                                //TODO ABSORBER
                                default:
                                    throw new Exception("Type not found in row: " + indexHeadingRow.ToString() + " col: " + i.ToString());
                            }
                        }
                    }

                }
            }
            return structure;
        }

        private StrcutureSystem SetHeatPump(LayoutColumn valueSplitted, StrcutureSystem structure)
        {
            var heatPumpFund = structure.HeatPumpList.Find(c => c.ModelName == valueSplitted.ModelName);
            if (heatPumpFund == null)
            {
                var item = new HeatPump()
                {
                    InstallationArea = valueSplitted.Area,
                    ModelName = valueSplitted.ModelName
                };
                if (!string.IsNullOrEmpty(valueSplitted.Value))
                {
                    if (item.HeatPumpValue == null)
                    {
                        item.HeatPumpValue = new List<HeatPumpValue>();
                    }
                    if (item.HeatPumpValue.Count == 0)
                    {
                        HeatPumpValue heatPumpValue = new HeatPumpValue();
                        heatPumpValue.LabelValues = new List<string>() { valueSplitted.Value };
                        item.HeatPumpValue.Add(heatPumpValue);
                    }
                }
                structure.HeatPumpList.Add(item);
            }
            else
            {
                if (!string.IsNullOrEmpty(valueSplitted.Value))
                {
                    var valueBag = heatPumpFund.HeatPumpValue.LastOrDefault();
                    if (valueBag != null && !valueBag.LabelValues.Contains(valueSplitted.Value))
                    {
                        valueBag.LabelValues.Add(valueSplitted.Value);
                    }
                }
            }
            return structure;
        }

        private StrcutureSystem SetSensorList(LayoutColumn valueSplitted, StrcutureSystem structure)
        {
            var sensorFound = new Sensor();
            if (valueSplitted.Value != "CMD")
            {
                if (structure.SensorList.Count() > 0)
                {
                    foreach (string usedBy in valueSplitted.Father)
                    {
                        sensorFound = structure.SensorList.Find(c => c.ModelName == valueSplitted.ModelName && c.UsedBy.Contains(usedBy));
                        if (sensorFound != null) break;
                    }
                }
            }
            else
            {
                sensorFound = structure.SensorList.Find(c => c.ModelName == valueSplitted.ModelName);
            }

            if (sensorFound == null || structure.SensorList.Count() == 0)
            {
                var item = new Sensor()
                {
                    InstallationArea = valueSplitted.Area,
                    ModelName = valueSplitted.ModelName
                };
                item.UsedBy = new List<string>();
                item.UsedBy = valueSplitted.Father;

                if (valueSplitted.Type == "energymeter")
                {
                    if (item.EnergyMeterList == null)
                    {
                        item.EnergyMeterList = new List<EnergyMeter>();
                        EnergyMeter energyMeter = new EnergyMeter();
                        energyMeter.LabelValues = new List<string>() { valueSplitted.Value };
                        item.EnergyMeterList.Add(energyMeter);
                    }
                    else
                    {
                        var valueBag = sensorFound.EnergyMeterList.LastOrDefault();
                        valueBag.LabelValues.Add(valueSplitted.Value);
                    }
                }
                if (valueSplitted.Type == "pump")
                {
                    if (item.PumpSensorList == null)
                    {
                        item.PumpSensorList = new List<PumpSensor>();
                        PumpSensor pump = new PumpSensor();
                        pump.LabelValues = new List<string>() { valueSplitted.Value };
                        item.PumpSensorList.Add(pump);
                    }
                    else
                    {
                        var valueBag = sensorFound.PumpSensorList.LastOrDefault();
                        valueBag.LabelValues.Add(valueSplitted.Value);

                    }
                }
                structure.SensorList.Add(item);
            }
            else
            {
                if (!string.IsNullOrEmpty(valueSplitted.Value))
                {
                    if (valueSplitted.Type == "energymeter")
                    {
                        var valueBag = sensorFound.EnergyMeterList.LastOrDefault();
                        if (valueBag != null && !valueBag.LabelValues.Contains(valueSplitted.Value))
                        {
                            valueBag.LabelValues.Add(valueSplitted.Value);
                        }
                    }
                    if (valueSplitted.Type == "pump")
                    {
                        var valueBag = sensorFound.PumpSensorList.LastOrDefault();
                        if (valueBag != null && !valueBag.LabelValues.Contains(valueSplitted.Value))
                        {
                            valueBag.LabelValues.Add(valueSplitted.Value);
                        }
                        if (valueBag != null && valueSplitted.Value == "CMD")
                        {
                            foreach (string s in valueSplitted.Father)
                            {
                                if (!sensorFound.UsedBy.Contains(s))
                                {
                                    sensorFound.UsedBy.Add(s);
                                }
                            }
                        }
                    }
                }
            }

            return structure;
        }

        private StrcutureSystem SetBoilerList(LayoutColumn valueSplitted, StrcutureSystem structure)
        {
            var boilerFund = structure.BoilerList.Find(c => c.ModelName == valueSplitted.ModelName);
            if (boilerFund == null)
            {
                var item = new Boiler()
                {
                    InstallationArea = valueSplitted.Area,
                    ModelName = valueSplitted.ModelName
                };
                if (!string.IsNullOrEmpty(valueSplitted.Value))
                {
                    if (item.BoilerValue == null)
                    {
                        item.BoilerValue = new List<BoilerValue>();
                    }
                    if (item.BoilerValue.Count == 0)
                    {
                        BoilerValue boilerValue = new BoilerValue();
                        boilerValue.LabelValues = new List<string>() { valueSplitted.Value };
                        item.BoilerValue.Add(boilerValue);
                    }
                }
                structure.BoilerList.Add(item);
            }
            else
            {
                if (!string.IsNullOrEmpty(valueSplitted.Value))
                {
                    var valueBag = boilerFund.BoilerValue.LastOrDefault();
                    if (valueBag != null && !valueBag.LabelValues.Contains(valueSplitted.Value))
                    {
                        valueBag.LabelValues.Add(valueSplitted.Value);
                    }
                }
            }
            return structure;
        }

        private StrcutureSystem SetCogeneratorList(LayoutColumn valueSplitted, StrcutureSystem structure)
        {
            var cogeFind = structure.CogeneratorList.Find(c => c.ModelName == valueSplitted.ModelName);
            if (cogeFind == null)
            {
                var item = new Cogenerator()
                {
                    InstallationArea = valueSplitted.Area,
                    ModelName = valueSplitted.ModelName
                };
                if (!string.IsNullOrEmpty(valueSplitted.Value))
                {
                    if (item.CogeneratorValue == null)
                    {
                        item.CogeneratorValue = new List<CogeneratoreValue>();
                    }
                    if (item.CogeneratorValue.Count == 0)
                    {
                        CogeneratoreValue cogVal = new CogeneratoreValue();
                        cogVal.LabelValues = new List<string>() { valueSplitted.Value };
                        item.CogeneratorValue.Add(cogVal);
                    }
                }
                structure.CogeneratorList.Add(item);
            }
            else
            {
                if (!string.IsNullOrEmpty(valueSplitted.Value))
                {
                    var valueBag = cogeFind.CogeneratorValue.LastOrDefault();
                    if (valueBag != null && !valueBag.LabelValues.Contains(valueSplitted.Value))
                    {
                        valueBag.LabelValues.Add(valueSplitted.Value);
                    }
                }
            }
            return structure;
        }

        private StrcutureSystem SetEmptyStrcuture()
        {
            StrcutureSystem structure = new StrcutureSystem();
            structure.CogeneratorList = new List<Cogenerator>();
            structure.BoilerList = new List<Boiler>();
            structure.HeatPumpList = new List<HeatPump>();
            structure.SensorList = new List<Sensor>();

            return structure;
        }

        private LayoutColumn GetLayout(string sectionName)
        {
            LayoutColumn layout = new LayoutColumn();

            if (!string.IsNullOrEmpty(sectionName))
            {
                if (sectionName == "data_ora")
                {
                    layout.Type = sectionName;
                }
                else
                {
                    var array = sectionName.Split('\'');

                    layout.Area = array[0];
                    layout.Type = array[1].ToLower();
                    // questa gestione è dovuta dal fatto che nei sensori mi aspetto il nome dei modelli di riferimento.
                    if (array[2].Contains('-'))
                    {
                        var split = array[2].Split('-');
                        for (int i = 0; i < split.Length; i++)
                        {
                            if (i == 0) { layout.ModelName = split[i]; }
                            else
                            {
                                if (layout.Father == null) layout.Father = new List<string>();
                                layout.Father.Add(split[i]);
                            }
                        }
                    }
                    else
                    {
                        layout.ModelName = array[2];
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
            public string ModelName { get; set; }
            public List<string> Father { get; set; }
            public string Value { get; set; }
        }
    }
}
