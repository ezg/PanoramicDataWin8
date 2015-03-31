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
            double dataMinX = sampleQueryResultItemModels.Where(dp => dp.XValue.HasValue).Min(dp => dp.XValue.Value);
            double dataMinY = sampleQueryResultItemModels.Where(dp => dp.YValue.HasValue).Min(dp => dp.YValue.Value);
            double dataMaxX = sampleQueryResultItemModels.Where(dp => dp.XValue.HasValue).Max(dp => dp.XValue.Value);
            double dataMaxY = sampleQueryResultItemModels.Where(dp => dp.YValue.HasValue).Max(dp => dp.YValue.Value);

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
                double[] xExtent = getExtent(XAxisType, dataMinX, dataMaxX, getNumberOfBins(XAxisType, dataMinX, dataMaxX, NrOfXBins));
                double[] yExtent = getExtent(YAxisType, dataMinY, dataMaxY, getNumberOfBins(YAxisType, dataMinY, dataMaxY, NrOfYBins));
                DateTimeStep dateTimeSizeX = null;
                DateTimeStep dateTimeSizeY = null;

                if (XAxisType == AxisType.Time || XAxisType == AxisType.Date)
                {
                    DateTimeUtil.GetDataTimeExtent(dataMinX, dataMaxX, NrOfXBins, out dateTimeSizeX);
                } 
                if (YAxisType == AxisType.Time || YAxisType == AxisType.Date)
                {
                    DateTimeUtil.GetDataTimeExtent(dataMinY, dataMaxY, NrOfYBins, out dateTimeSizeY);
                }

                LastBinStructure =
                    initializeBinStructure(
                        xExtent[0], yExtent[0],
                        xExtent[2], yExtent[2],
                        dateTimeSizeX, dateTimeSizeY,
                        getNumberOfBins(XAxisType, dataMinX, dataMaxX, NrOfXBins),
                        getNumberOfBins(YAxisType, dataMinY, dataMaxY, NrOfYBins));

                LastBinStructure.DataMinX = dataMinX;
                LastBinStructure.DataMinY = dataMinY;
                LastBinStructure.DataMaxX = dataMaxX;
                LastBinStructure.DataMaxY = dataMaxY;
            }
            double plusX = calculateNrOfBinsToAdd(XAxisType, dataMinX, dataMaxX, LastBinStructure.DataMinX, LastBinStructure.DataMaxX, LastBinStructure.BinMinX, LastBinStructure.BinMaxX, LastBinStructure.BinSizeX, LastBinStructure.DateTimeStepX);
            double plusY = calculateNrOfBinsToAdd(YAxisType, dataMinY, dataMaxY, LastBinStructure.DataMinY, LastBinStructure.DataMaxY, LastBinStructure.BinMinY, LastBinStructure.BinMaxY, LastBinStructure.BinSizeY, LastBinStructure.DateTimeStepY);

            double newBinMinX = 0;
            if (dataMinX < LastBinStructure.BinMinX && (XAxisType == AxisType.Time || XAxisType == AxisType.Date))
            {
                int stepsTaken = 0;
                DateTimeUtil.IncludeDateTime(LastBinStructure.BinMinX, dataMinX, LastBinStructure.DateTimeStepX, false, out stepsTaken);
                newBinMinX = DateTimeUtil.RemoveFromDateTime(
                    LastBinStructure.BinMinX,
                    new DateTimeStep()
                    {
                        DateTimeStepGranularity = LastBinStructure.DateTimeStepX.DateTimeStepGranularity,
                        DateTimeStepValue = LastBinStructure.DateTimeStepX.DateTimeStepValue * stepsTaken
                    }).Ticks;
            }
            else
            {
                newBinMinX = LastBinStructure.BinMinX - (Math.Ceiling((LastBinStructure.BinMinX - dataMinX) / LastBinStructure.BinSizeX) * LastBinStructure.BinSizeX);
            }

            double newBinMinY = 0;
            if (dataMinY < LastBinStructure.BinMinY && (YAxisType == AxisType.Time || YAxisType == AxisType.Date))
            {
                int stepsTaken = 0;
                DateTimeUtil.IncludeDateTime(LastBinStructure.BinMinY, dataMinY, LastBinStructure.DateTimeStepY, false, out stepsTaken);
                newBinMinX = DateTimeUtil.RemoveFromDateTime(
                    LastBinStructure.BinMinY,
                    new DateTimeStep()
                    {
                        DateTimeStepGranularity = LastBinStructure.DateTimeStepY.DateTimeStepGranularity,
                        DateTimeStepValue = LastBinStructure.DateTimeStepY.DateTimeStepValue * stepsTaken
                    }).Ticks;
            }
            else
            {
                newBinMinY = LastBinStructure.BinMinY - (Math.Ceiling((LastBinStructure.BinMinY - dataMinY) / LastBinStructure.BinSizeY) * LastBinStructure.BinSizeY);
            }

            BinStructure tempBinStructure =
                initializeBinStructure(
                    newBinMinX, newBinMinY,
                    LastBinStructure.BinSizeX, LastBinStructure.BinSizeY,
                    LastBinStructure.DateTimeStepX, LastBinStructure.DateTimeStepY,
                    (LastBinStructure.Bins.Count - 1) + plusX, (LastBinStructure.Bins[0].Count - 1) + plusY);

            binSamples2(tempBinStructure, sampleQueryResultItemModels);

            // re-map old bins
            if (Incremental)
            {
                tempBinStructure.XNullCount += LastBinStructure.XNullCount;
                tempBinStructure.YNullCount += LastBinStructure.YNullCount;
                tempBinStructure.XAndYNullCount += LastBinStructure.XAndYNullCount;

                foreach (var oldBin in LastBinStructure.Bins.SelectMany(b => b))
                {
                    int x = (int)Math.Floor((oldBin.BinMinX - tempBinStructure.BinMinX) / tempBinStructure.BinSizeX);
                    int y = (int)Math.Floor((oldBin.BinMinY - tempBinStructure.BinMinY) / tempBinStructure.BinSizeY);
                    Bin newBin = tempBinStructure.Bins[x][y];

                    if (newBin.ContainsBin(oldBin))
                    {
                        newBin.Count += oldBin.Count;
                    }
                }
            }

            int xBinsToMerge = getNumberOfBinsToMerge(XAxisType, tempBinStructure.Bins.Count - 1, NrOfXBins);
            int yBinsToMerge = getNumberOfBinsToMerge(YAxisType, tempBinStructure.Bins[0].Count - 1, NrOfYBins);

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

        private double getNumberOfBins(AxisType axisType, double dataMin, double dataMax, double defaultNrBins)
        {
            return axisType == AxisType.Nominal || axisType == AxisType.Ordinal ? (dataMax - dataMin) : defaultNrBins;
        }

        private int getNumberOfBinsToMerge(AxisType axisType, int binCount, double defaultNrBins)
        {
            return axisType == AxisType.Quantitative ? (int)(binCount / defaultNrBins) : 1;
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

            mergedBinStructure.XNullCount += binStructure.XNullCount;
            mergedBinStructure.YNullCount += binStructure.YNullCount;
            mergedBinStructure.XAndYNullCount += binStructure.XAndYNullCount;

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
                    if (dp.XValue.HasValue && dp.YValue.HasValue)
                    {
                        if (bin.BinIntersects(dp.XValue.Value, dp.YValue.Value))
                        {
                            intersectingQueryResultItemModels.Add(dp);
                        }
                    }
                    else
                    {
                        binStructure.XNullCount += !dp.XValue.HasValue ? 1 : 0;
                        binStructure.YNullCount += !dp.YValue.HasValue ? 1 : 0;
                        binStructure.XAndYNullCount += !dp.XValue.HasValue && !dp.YValue.HasValue ? 1 : 0;
                    }
                }
                bin.Count += intersectingQueryResultItemModels.Count;
            }
        }

        private void binSamples2(BinStructure binStructure, List<QueryResultItemModel> samples)
        {
            foreach (var dp in samples)
            {
                if (dp.XValue.HasValue && dp.YValue.HasValue)
                {
                    int x = (int)Math.Floor((dp.XValue.Value - binStructure.BinMinX) / binStructure.BinSizeX);
                    int y = (int)Math.Floor((dp.YValue.Value - binStructure.BinMinY) / binStructure.BinSizeY);
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

        private double[] getExtent(AxisType axisType, double dataMin, double dataMax, double m)
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

        private BinStructure initializeBinStructure(double binMinX, double binMinY, double sizeX, double sizeY,
            DateTimeStep dateTimeSizeX, DateTimeStep dateTimeSizeY,
            double nrOfXBins, double nrOfYBins)
        {
            BinStructure binStructure = new BinStructure();
            binStructure.BinSizeX = sizeX;
            binStructure.BinSizeY = sizeY;
            binStructure.DateTimeStepX = dateTimeSizeX;
            binStructure.DateTimeStepY = dateTimeSizeY;

            for (int xIndex = 0; xIndex <= nrOfXBins; xIndex++)
            {
                double x = binMinX + xIndex * sizeX;
                double maxX = x + sizeX;
                if (dateTimeSizeX != null)
                {
                    x = DateTimeUtil.AddToDateTime(binMinX, dateTimeSizeX.DateTimeStepGranularity, (double)(dateTimeSizeX.DateTimeStepValue * xIndex)).Ticks;
                    maxX = DateTimeUtil.AddToDateTime(x, dateTimeSizeX).Ticks;
                }

                List<Bin> newBinCol = new List<Bin>();
                for (int yIndex = 0; yIndex <= nrOfYBins; yIndex++)
                {
                    double y = binMinY + yIndex * sizeY; 
                    double maxY = y + sizeY;
                    if (dateTimeSizeY != null)
                    {
                        y = DateTimeUtil.AddToDateTime(binMinY, dateTimeSizeY.DateTimeStepGranularity, (double)(dateTimeSizeY.DateTimeStepValue * yIndex)).Ticks;
                        maxY = DateTimeUtil.AddToDateTime(y, dateTimeSizeY).Ticks;
                    }

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

            binStructure.BinMinX = binMinX;
            binStructure.BinMinY = binMinY;
            binStructure.BinMaxX = binMinX + sizeX * (nrOfXBins + 1);

            if (dateTimeSizeX != null)
            {
                binStructure.BinMaxX = DateTimeUtil.AddToDateTime(binMinX, dateTimeSizeX.DateTimeStepGranularity, (double)(dateTimeSizeX.DateTimeStepValue * (nrOfXBins + 1))).Ticks;
            }
            binStructure.BinMaxY = binMinY + sizeY * (nrOfYBins + 1);

            if (dateTimeSizeY != null)
            {
                binStructure.BinMaxY = DateTimeUtil.AddToDateTime(binMinY, dateTimeSizeY.DateTimeStepGranularity, (double)(dateTimeSizeY.DateTimeStepValue * (nrOfYBins + 1))).Ticks;
            }

            return binStructure;
        }

        private double calculateNrOfBinsToAdd(AxisType axisType, double dataMin, double dataMax, double binDataMin, double binDataMax, double binMin, double binMax, double binSize, DateTimeStep dateTimeSize)
        {
            double plus = 0;
            if (dateTimeSize == null)
            {
                if (axisType == AxisType.Quantitative)
                {
                    if (dataMin < binMin)
                    {
                        double newBinMinX = binMin - (Math.Ceiling((binMin - dataMin) / binSize) * binSize);
                        plus += Math.Ceiling((binMin - newBinMinX) / binSize);
                    }
                    if (dataMax >= binMax)
                    {
                        double newBinMaxX = binMax + (Math.Ceiling((dataMax - binMax) / binSize) * binSize) + binSize;
                        plus += Math.Ceiling((newBinMaxX - binMax) / binSize);
                    }
                }
                else
                {
                    plus = Math.Max(0, (dataMax - dataMin) - (binDataMax - binDataMin));
                }
            }
            else
            {
                if (dataMin < binMin)
                {
                    int stepsTaken = 0;
                    DateTimeUtil.IncludeDateTime(binMin, dataMin, dateTimeSize, false, out stepsTaken);
                    plus += stepsTaken;
                }
                if (dataMax >= binMax)
                {
                    int stepsTaken = 0;
                    DateTimeUtil.IncludeDateTime(binMax, dataMax, dateTimeSize, true, out stepsTaken);
                    plus += stepsTaken;
                }
            }
            return plus;
        }
    }
}
