using PanoramicData.model.data;
using PanoramicData.model.data.sim;
using PanoramicData.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Dynamic;
using System.Diagnostics;
using PanoramicDataWin8.utils;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PanoramicData.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.controller.data.sim.binrange;

namespace PanoramicDataWin8.controller.data.sim
{
    public class DataBinner
    {
        public bool Incremental { get; set; }
        public AxisType XAxisType { get; set; }
        public bool IsXAxisAggregated { get; set; }
        public AxisType YAxisType { get; set; }
        public bool IsYAxisAggregated { get; set; }
        public double NrOfXBins { get; set; }
        public double NrOfYBins { get; set; }
        public DataBinStructure DataBinStructure { get; set; }

        public void ProcessStep(List<DataRow> dataRows)
        {
            double dataMinX = 0;
            double dataMinY = 0;
            double dataMaxX = 0;
            double dataMaxY = 0;
            BinRange xBinRange = null;
            BinRange yBinRange = null;

            getMinMaxValuesFromSamples(dataRows, out dataMinX, out dataMinY, out dataMaxX, out dataMaxY);

            if (DataBinStructure != null)
            {
                dataMinX = Math.Min(DataBinStructure.XBinRange.DataMinValue, dataMinX);
                dataMinY = Math.Min(DataBinStructure.YBinRange.DataMinValue, dataMinY);
                dataMaxX = Math.Max(DataBinStructure.XBinRange.DataMaxValue, dataMaxX);
                dataMaxY = Math.Max(DataBinStructure.YBinRange.DataMaxValue, dataMaxY);
            }
            else
            {
                xBinRange = CreateBinRange(XAxisType, dataMinX, dataMaxX, NrOfXBins, IsXAxisAggregated);
                yBinRange = CreateBinRange(YAxisType, dataMinY, dataMaxY, NrOfYBins, IsYAxisAggregated);

                DataBinStructure = initializeBinStructure(xBinRange, yBinRange);
            }
            xBinRange = DataBinStructure.XBinRange.GetUpdatedBinRange(dataMinX, dataMaxX);
            yBinRange = DataBinStructure.YBinRange.GetUpdatedBinRange(dataMinY, dataMaxY);

            DataBinStructure tempBinStructure = initializeBinStructure(xBinRange, yBinRange);
            binSamples(tempBinStructure, dataRows);

            // re-map old bins
            tempBinStructure.XNullCount += DataBinStructure.XNullCount;
            tempBinStructure.YNullCount += DataBinStructure.YNullCount;
            tempBinStructure.XAndYNullCount += DataBinStructure.XAndYNullCount;

            foreach (var oldBin in DataBinStructure.Bins.SelectMany(b => b))
            {
                int x = tempBinStructure.XBinRange.GetIndex(oldBin.BinMinX);
                int y = tempBinStructure.YBinRange.GetIndex(oldBin.BinMinY);
                Bin newBin = tempBinStructure.Bins[x][y];

                if (newBin.ContainsBin(oldBin))
                {
                    newBin.Count += oldBin.Count;
                }
            }
            DataBinStructure = tempBinStructure;
        }

        private BinRange CreateBinRange(AxisType axisType, double dataMinValue, double dataMaxValue, double nrBins, bool isAxisAggregated)
        {
            BinRange scale = null;
            if (isAxisAggregated)
            {
                scale = AggregateBinRange.Initialize();
            }
            if (axisType == AxisType.Time || axisType == AxisType.Date)
            {
                scale = DateTimeBinRange.Initialize(dataMinValue, dataMaxValue, nrBins);
            }
            else if (axisType == AxisType.Quantitative)
            {
                scale = QuantitativeBinRange.Initialize(dataMinValue, dataMaxValue, nrBins);
            }
            else
            {
                scale = NominalBinRange.Initialize(dataMinValue, dataMaxValue, nrBins);
            }
            return scale;
        }

        private void getMinMaxValuesFromSamples(List<DataRow> dataRows, out double dataMinX, out double dataMinY, out double dataMaxX, out double dataMaxY)
        {
            dataMinX = dataRows.Where(dp => dp.XValue.HasValue).Min(dp => dp.XValue.Value);
            dataMinY = dataRows.Where(dp => dp.YValue.HasValue).Min(dp => dp.YValue.Value);
            dataMaxX = dataRows.Where(dp => dp.XValue.HasValue).Max(dp => dp.XValue.Value);
            dataMaxY = dataRows.Where(dp => dp.YValue.HasValue).Max(dp => dp.YValue.Value);

            if (dataMaxX == dataMinX)
            {
                if (XAxisType != AxisType.Quantitative)
                {
                    dataMinX -= 0;
                    dataMaxX += 0.1;
                }
                else
                {
                    dataMinX -= 1;
                    dataMaxX += 1;
                }
            }
            if (dataMaxY == dataMinY)
            {
                if (YAxisType != AxisType.Quantitative)
                {
                    dataMinY -= 0;
                    dataMaxY += 0.1;
                }
                else
                {
                    dataMinY -= 1;
                    dataMaxY += 1;
                }
            }
        }

        private void adjustNormalizedCount(DataBinStructure binStructure)
        {
            // adjust normalized count
            double maxCount = binStructure.Bins.SelectMany(b => b).Max(b => b.Count);
            foreach (var bin in binStructure.Bins.SelectMany(b => b))
            {
                //bin.NormalizedCount = Math.Log(bin.Count) / Math.Log(maxCount);
                bin.NormalizedCount = bin.Count / maxCount;
                if (bin.NormalizedCount == 0)
                {
                    bin.Size = 0;
                }
                else
                {
                    //double r = sliderRange.Value;
                    // 0.1 * log10(x ) + 1
                    bin.Size = Math.Sqrt(bin.NormalizedCount);// 0.1 * Math.Log10(bin.NormalizedCount) + 1;// bin.NormalizedCount; // = (1.0 / (r + 1)) * Math.Ceiling(bin.NormalizedCount / (1.0 / r));
                }
            }
        }

        private void binSamples(DataBinStructure binStructure, List<DataRow> samples)
        {
            foreach (var sample in samples)
            {
                if (sample.XValue.HasValue && sample.YValue.HasValue)
                {
                    int x = binStructure.XBinRange.GetIndex(sample.XValue.Value);
                    int y = binStructure.YBinRange.GetIndex(sample.YValue.Value);
                    Bin bin = binStructure.Bins[x][y];
                    bin.Samples.Add(sample);
                }
                else
                {
                    binStructure.XNullCount += !sample.XValue.HasValue ? 1 : 0;
                    binStructure.YNullCount += !sample.YValue.HasValue ? 1 : 0;
                    binStructure.XAndYNullCount += !sample.XValue.HasValue && !sample.YValue.HasValue ? 1 : 0;
                }
            }
        }

        private DataBinStructure initializeBinStructure(BinRange xBinRange, BinRange yBinRange)
        {
            DataBinStructure binStructure = new DataBinStructure();
            binStructure.XBinRange = xBinRange;
            binStructure.YBinRange = yBinRange;

            foreach (double x in xBinRange.GetBins())
            {
                double minX = x;
                double maxX = xBinRange.AddStep(x);
            
                List<Bin> newBinCol = new List<Bin>();
                foreach (double y in yBinRange.GetBins())
                {
                    double minY = y;
                    double maxY = yBinRange.AddStep(y);

                    Bin bin = new Bin()
                    {
                        BinMinX = x,
                        BinMaxX = maxX,
                        BinMinY = y,
                        BinMaxY = maxY,
                        Count = 0
                    };
                    newBinCol.Add(bin);
                }
                binStructure.Bins.Add(newBinCol);
            }
            return binStructure;
        }
    }
}
