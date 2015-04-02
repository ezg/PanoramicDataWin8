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

namespace PanoramicDataWin8.controller.data.sim
{
    public class Binner
    {
        public bool Incremental { get; set; }
        public AxisType XAxisType { get; set; }
        public AxisType YAxisType { get; set; }
        public double NrOfXBins { get; set; }
        public double NrOfYBins { get; set; }
        public BinStructure LastBinStructure { get; set; }

        public void ProcessStep(List<QueryResultItemModel> sampleQueryResultItemModels)
        {
            double dataMinX = 0;
            double dataMinY = 0;
            double dataMaxX = 0;
            double dataMaxY = 0;
            Scale xScale = null;
            Scale yScale = null;

            getMinMaxValuesFromSamples(sampleQueryResultItemModels, out dataMinX, out dataMinY, out dataMaxX, out dataMaxY);

            if (LastBinStructure != null)
            {
                dataMinX = Math.Min(LastBinStructure.XScale.DataMinValue, dataMinX);
                dataMinY = Math.Min(LastBinStructure.YScale.DataMinValue, dataMinY);
                dataMaxX = Math.Max(LastBinStructure.XScale.DataMaxValue, dataMaxX);
                dataMaxY = Math.Max(LastBinStructure.YScale.DataMaxValue, dataMaxY);
            }
            else
            {
                xScale = CreateScale(XAxisType, dataMinX, dataMaxX, NrOfXBins);
                yScale = CreateScale(YAxisType, dataMinY, dataMaxY, NrOfYBins);

                LastBinStructure = initializeBinStructure(xScale, yScale);
            }
            xScale = LastBinStructure.XScale.GetUpdatedScale(dataMinX, dataMaxX);
            yScale = LastBinStructure.YScale.GetUpdatedScale(dataMinY, dataMaxY);

            BinStructure tempBinStructure = initializeBinStructure(xScale, yScale);
            binSamples(tempBinStructure, sampleQueryResultItemModels);

            // re-map old bins
            if (Incremental)
            {
                tempBinStructure.XNullCount += LastBinStructure.XNullCount;
                tempBinStructure.YNullCount += LastBinStructure.YNullCount;
                tempBinStructure.XAndYNullCount += LastBinStructure.XAndYNullCount;

                foreach (var oldBin in LastBinStructure.Bins.SelectMany(b => b))
                {
                    int x = tempBinStructure.XScale.GetIndex(oldBin.BinMinX);
                    int y = tempBinStructure.YScale.GetIndex(oldBin.BinMinY);
                    Bin newBin = tempBinStructure.Bins[x][y];

                    if (newBin.ContainsBin(oldBin))
                    {
                        newBin.Count += oldBin.Count;
                    }
                }
            }

            adjustNormalizedCount(tempBinStructure);
            LastBinStructure = tempBinStructure;
        }

        private Scale CreateScale(AxisType axisType, double dataMinValue, double dataMaxValue, double nrBins)
        {
            Scale scale = null;
            if (axisType == AxisType.Time || axisType == AxisType.Date)
            {
                scale = DateTimeScale.Initialize(dataMinValue, dataMaxValue, nrBins);
            }
            else if (axisType == AxisType.Quantitative)
            {
                scale = QuantitativeScale.Initialize(dataMinValue, dataMaxValue, nrBins);
            }
            else
            {
                scale = NominalScale.Initialize(dataMinValue, dataMaxValue, nrBins);
            }
            return scale;
        }

        private void getMinMaxValuesFromSamples(List<QueryResultItemModel> sampleQueryResultItemModels, out double dataMinX, out double dataMinY, out double dataMaxX, out double dataMaxY)
        {
            dataMinX = sampleQueryResultItemModels.Where(dp => dp.XValue.HasValue).Min(dp => dp.XValue.Value);
            dataMinY = sampleQueryResultItemModels.Where(dp => dp.YValue.HasValue).Min(dp => dp.YValue.Value);
            dataMaxX = sampleQueryResultItemModels.Where(dp => dp.XValue.HasValue).Max(dp => dp.XValue.Value);
            dataMaxY = sampleQueryResultItemModels.Where(dp => dp.YValue.HasValue).Max(dp => dp.YValue.Value);

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

        private void adjustNormalizedCount(BinStructure binStructure)
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

        private void binSamples(BinStructure binStructure, List<QueryResultItemModel> samples)
        {
            foreach (var dp in samples)
            {
                if (dp.XValue.HasValue && dp.YValue.HasValue)
                {
                    int x = binStructure.XScale.GetIndex(dp.XValue.Value);
                    int y = binStructure.YScale.GetIndex(dp.YValue.Value);
                    Bin bin = binStructure.Bins[x][y];
                    bin.Count += 1;
                }
                else
                {
                    binStructure.XNullCount += !dp.XValue.HasValue ? 1 : 0;
                    binStructure.YNullCount += !dp.YValue.HasValue ? 1 : 0;
                    binStructure.XAndYNullCount += !dp.XValue.HasValue && !dp.YValue.HasValue ? 1 : 0;
                }
            }
        }

        private BinStructure initializeBinStructure(Scale xScale, Scale yScale)
        {
            BinStructure binStructure = new BinStructure();
            binStructure.XScale = xScale;
            binStructure.YScale = yScale;

            foreach (double x in xScale.GetScale())
            {
                double minX = x;
                double maxX = xScale.AddStep(x);
            
                List<Bin> newBinCol = new List<Bin>();
                foreach (double y in yScale.GetScale())
                {
                    double minY = y;
                    double maxY = yScale.AddStep(y);

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
