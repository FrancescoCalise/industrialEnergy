using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.PieChart;
using ChartJs.Blazor.Util;

namespace IndustrialEnergy.Services
{
    public static class ChartService
    {
        private static List<string> colors = new List<string>() {
            ColorUtil.FromDrawingColor(Color.Red),
            ColorUtil.FromDrawingColor(Color.Blue),
            ColorUtil.FromDrawingColor(Color.Orange),
            ColorUtil.FromDrawingColor(Color.Gray),
            ColorUtil.FromDrawingColor(Color.Yellow),
            ColorUtil.FromDrawingColor(Color.Violet),
            ColorUtil.FromDrawingColor(Color.LightGray),
            ColorUtil.FromDrawingColor(Color.Pink),
            ColorUtil.FromDrawingColor(Color.Gold)
        };

        public static PieConfig SetConfigPieChart(string title, Dictionary<string, double> values)
        {
            PieConfig config = new PieConfig()
            {
                Options = new PieOptions
                {
                    Responsive = true,
                    Title = new OptionsTitle
                    {
                        Display = true,
                        Text = title
                    }
                }
            };
            List<double> items = new List<double>();
            List<string> colorsChart = new List<string>();

            int i = 0;
            foreach (string key in values.Keys)
            {
                config.Data.Labels.Add(key);
                items.Add(values[key]);
                colorsChart.Add(colors[i]);
                i++;
            }

            PieDataset<double> dataset = new PieDataset<double>(items)
            {
                BackgroundColor = colors.ToArray()
            };

            config.Data.Datasets.Add(dataset);

            return config;
        }
    }
}
