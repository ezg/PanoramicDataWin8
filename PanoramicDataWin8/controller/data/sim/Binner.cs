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
            double dataMinX = sampleQueryResultItemModels.Min(dp => dp.XValue);
            double dataMinY = sampleQueryResultItemModels.Min(dp => dp.YValue);
            double dataMaxX = sampleQueryResultItemModels.Max(dp => dp.XValue);
            double dataMaxY = sampleQueryResultItemModels.Max(dp => dp.YValue);

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

            if (LastBinStructure != null)
            {
                dataMinX = Math.Min(LastBinStructure.DataMinX, dataMinX);
                dataMinY = Math.Min(LastBinStructure.DataMinY, dataMinY);
                dataMaxX = Math.Max(LastBinStructure.DataMaxX, dataMaxX);
                dataMaxY = Math.Max(LastBinStructure.DataMaxY, dataMaxY);
            }
            else
            {
                double[] xExtent = getExtent(dataMinX, dataMaxX, XAxisType != AxisType.Quantitative ? (dataMaxX - dataMinX) : NrOfXBins);
                double[] yExtent = getExtent(dataMinY, dataMaxY, YAxisType != AxisType.Quantitative ? (dataMaxY - dataMinY) : NrOfYBins);
                LastBinStructure =
                    initializeBinStructure(
                        xExtent[0], yExtent[0],
                        xExtent[2], yExtent[2],
                        XAxisType != AxisType.Quantitative ? (dataMaxX - dataMinX) : NrOfXBins,
                        YAxisType != AxisType.Quantitative ? (dataMaxY - dataMinY) : NrOfYBins);

                LastBinStructure.DataMinX = dataMinX;
                LastBinStructure.DataMinY = dataMinY;
                LastBinStructure.DataMaxX = dataMaxX;
                LastBinStructure.DataMaxY = dataMaxY;
            }
            double plusX = 0;
            double plusY = 0;
            calculateNrOfBinsToAdd(out plusX, out plusY, LastBinStructure, dataMinX, dataMaxX, dataMinY, dataMaxY);

            double newBinMinX = LastBinStructure.BinMinX - (Math.Ceiling((LastBinStructure.BinMinX - dataMinX) / LastBinStructure.BinSizeX) * LastBinStructure.BinSizeX);
            double newBinMinY = LastBinStructure.BinMinY - (Math.Ceiling((LastBinStructure.BinMinY - dataMinY) / LastBinStructure.BinSizeY) * LastBinStructure.BinSizeY);

            BinStructure tempBinStructure =
                initializeBinStructure(
                    newBinMinX, newBinMinY,
                    LastBinStructure.BinSizeX, LastBinStructure.BinSizeY,
                    (LastBinStructure.Bins.Count - 1) + plusX, (LastBinStructure.Bins[0].Count - 1) + plusY);

            binSamples2(tempBinStructure, sampleQueryResultItemModels);

            // re-map old bins
            if (Incremental)
            {
                foreach (var oldBin in LastBinStructure.Bins.SelectMany(b => b))
                {
                    int containCount = 0;
                    foreach (var newBin in tempBinStructure.Bins.SelectMany(b => b))
                    {
                        if (newBin.ContainsBin(oldBin))
                        {
                            newBin.Count += oldBin.Count;
                            containCount++;
                        }
                    }
                    if (containCount != 1)
                    {

                    }
                }
            }

            int xBinsToMerge = XAxisType == AxisType.Quantitative ? (int)((tempBinStructure.Bins.Count - 1) / (NrOfXBins)) : 1;
            int yBinsToMerge = YAxisType == AxisType.Quantitative ? (int)((tempBinStructure.Bins[0].Count - 1) / (NrOfYBins)) : 1;

            BinStructure mergedBinStructure = new BinStructure();
            mergedBinStructure.BinSizeX = tempBinStructure.BinSizeX * xBinsToMerge;
            mergedBinStructure.BinSizeY = tempBinStructure.BinSizeY * yBinsToMerge;

            mergeBinStructure(mergedBinStructure, tempBinStructure, xBinsToMerge, yBinsToMerge);

            adjustNormalizedCount(mergedBinStructure);

            LastBinStructure = mergedBinStructure;
            LastBinStructure.DataMinX = dataMinX;
            LastBinStructure.DataMinY = dataMinY;
            LastBinStructure.DataMaxX = dataMaxX;
            LastBinStructure.DataMaxY = dataMaxY;
        }

        private void mergeBinStructure(BinStructure mergedBinStructure, BinStructure binStructure, int xBinsToMerge, int yBinsToMerge)
        {
            // merge bins
            for (int col = 0; col < binStructure.Bins.Count; col += xBinsToMerge)
            {
                List<Bin> newBinCol = new List<Bin>();
                for (int row = 0; row < binStructure.Bins[col].Count; row += yBinsToMerge)
                {
                    List<Bin> bins = binStructure.Bins.Skip(col).Take(xBinsToMerge).SelectMany(b => b.Skip(row).Take(yBinsToMerge)).ToList();

                    Bin bin = new Bin()
                    {
                        BinMinX = bins.Min(b => b.BinMinX),
                        BinMaxX = bins.Max(b => b.BinMaxX),
                        BinMinY = bins.Min(b => b.BinMinY),
                        BinMaxY = bins.Max(b => b.BinMaxY),
                        Count = bins.Sum(b => b.Count)
                    };

                    newBinCol.Add(bin);
                }
                mergedBinStructure.Bins.Add(newBinCol);
            }
            mergedBinStructure.BinMinX = mergedBinStructure.Bins.SelectMany(b => b).Min(b => b.BinMinX);
            mergedBinStructure.BinMinY = mergedBinStructure.Bins.SelectMany(b => b).Min(b => b.BinMinY);
            mergedBinStructure.BinMaxX = mergedBinStructure.Bins.SelectMany(b => b).Max(b => b.BinMaxX);
            mergedBinStructure.BinMaxY = mergedBinStructure.Bins.SelectMany(b => b).Max(b => b.BinMaxY);
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
            foreach (var bin in binStructure.Bins.SelectMany(b => b))
            {
                List<QueryResultItemModel> intersectingQueryResultItemModels = new List<QueryResultItemModel>();
                foreach (var dp in samples)
                {
                    if (bin.BinIntersects(dp.XValue, dp.YValue))
                    {
                        intersectingQueryResultItemModels.Add(dp);
                    }
                }
                bin.Count += intersectingQueryResultItemModels.Count;
            }
        }

        private void binSamples2(BinStructure binStructure, List<QueryResultItemModel> samples)
        {
            foreach (var dp in samples)
            {
                int x = (int)Math.Floor((dp.XValue - binStructure.BinMinX) / binStructure.BinSizeX);
                int y = (int)Math.Floor((dp.YValue - binStructure.BinMinY) / binStructure.BinSizeY);
                Bin bin = binStructure.Bins[x][y];
                bin.Count += 1;
            }
        }

        private double[] getExtent(double dataMin, double dataMax, double m)
        {
            double span = dataMax - dataMin;

            double step = Math.Pow(10, Math.Floor(Math.Log10(span / m)));
            double err = m / span * step;

            if (err <= .15)
                step *= 10;
            else if (err <= .35)
                step *= 5;
            else if (err <= .75)
                step *= 2;

            double[] ret = new double[3];
            ret[0] = (double)(Math.Floor(dataMin / step) * step);
            ret[1] = (double)(Math.Floor(dataMax / step) * step + step);
            ret[2] = (double)step;

            return ret;
        }

        private BinStructure initializeBinStructure(double binMinX, double binMinY, double sizeX, double sizeY, double nrOfXBins, double nrOfYBins)
        {
            BinStructure binStructure = new BinStructure();
            binStructure.BinSizeX = sizeX;
            binStructure.BinSizeY = sizeY;

            for (int xIndex = 0; xIndex <= nrOfXBins; xIndex++)
            {
                double x = binMinX + xIndex * sizeX;
                List<Bin> newBinCol = new List<Bin>();
                for (int yIndex = 0; yIndex <= nrOfYBins; yIndex++)
                {
                    double y = binMinY + yIndex * sizeY;
                    Bin bin = new Bin()
                    {
                        BinMinX = x,
                        BinMaxX = x + sizeX,
                        BinMinY = y,
                        BinMaxY = y + sizeY,
                        Count = 0
                    };
                    newBinCol.Add(bin);
                }
                binStructure.Bins.Add(newBinCol);
            }

            binStructure.BinMinX = binMinX;
            binStructure.BinMinY = binMinY;
            binStructure.BinMaxX = binMinX + sizeX * (nrOfXBins + 1);
            binStructure.BinMaxY = binMinY + sizeY * (nrOfYBins + 1);

            return binStructure;
        }

        private void calculateNrOfBinsToAdd(out double plusX, out double plusY, BinStructure binStructure, double dataMinX, double dataMaxX, double dataMinY, double dataMaxY)
        {
            plusX = 0;
            plusY = 0;
            if (XAxisType == AxisType.Quantitative)
            {
                if (dataMinX < binStructure.BinMinX)
                {
                    double newBinMinX = binStructure.BinMinX - (Math.Ceiling((binStructure.BinMinX - dataMinX) / binStructure.BinSizeX) * binStructure.BinSizeX);
                    plusX += Math.Ceiling((binStructure.BinMinX - newBinMinX) / binStructure.BinSizeX);
                }
                if (dataMaxX >= binStructure.BinMaxX)
                {
                    double newBinMaxX = binStructure.BinMaxX + (Math.Ceiling((dataMaxX - binStructure.BinMaxX) / binStructure.BinSizeX) * binStructure.BinSizeX) + binStructure.BinSizeX;
                    plusX += Math.Ceiling((newBinMaxX - binStructure.BinMaxX) / binStructure.BinSizeX);
                }
            }
            else
            {
                plusX = Math.Max(0, (dataMaxX - dataMinX) - (binStructure.DataMaxX - binStructure.DataMinX));
            }

            if (YAxisType == AxisType.Quantitative)
            {
                if (dataMinY < binStructure.BinMinY)
                {
                    double newBinMinY = binStructure.BinMinY - (Math.Ceiling((binStructure.BinMinY - dataMinY) / binStructure.BinSizeY) * binStructure.BinSizeY);
                    plusY += Math.Ceiling((binStructure.BinMinY - newBinMinY) / binStructure.BinSizeY);
                }
                if (dataMaxY >= binStructure.BinMaxY)
                {
                    double newBinMaxY = binStructure.BinMaxY + (Math.Ceiling((dataMaxY - binStructure.BinMaxY) / binStructure.BinSizeY) * binStructure.BinSizeY) + binStructure.BinSizeY;
                    plusY += Math.Ceiling((newBinMaxY - binStructure.BinMaxY) / binStructure.BinSizeY);
                }
            }
            else
            {
                plusY = Math.Max(0, (dataMaxY - dataMinY) - (binStructure.DataMaxY - binStructure.DataMinY));
            }
        }
    }
}
