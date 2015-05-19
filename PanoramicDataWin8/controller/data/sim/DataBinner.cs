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
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.common;

namespace PanoramicDataWin8.controller.data.sim
{
    public class DataBinner
    {
        public bool Incremental { get; set; }
        public List<AxisType> AxisTypes { get; set; }
        public List<bool> IsAxisAggregated { get; set; }
        public List<double> NrOfBins { get; set; }
        public BinStructure BinStructure { get; set; }
        public List<InputFieldModel> Dimensions { get; set; }

        public void BinStep(List<DataRow> dataRows)
        {
            List<double> dataMins = new List<double>();
            List<double> dataMaxs = new List<double>();
            List<BinRange> binRanges = null;

            if (dataRows.Count == 0)
            {
                return;
            }

            getMinMaxValuesFromSamples(dataRows, dataMins, dataMaxs);

            if (BinStructure != null)
            {
                for (int d = 0; d < Dimensions.Count; d++)
                {
                    dataMins[d] = Math.Min(BinStructure.BinRanges[d].DataMinValue, dataMins[d]);
                    dataMaxs[d] = Math.Max(BinStructure.BinRanges[d].DataMaxValue, dataMaxs[d]);
                }
                binRanges = BinStructure.BinRanges.ToArray().ToList();
            }
            else
            {
                binRanges = createBinRanges(dataMins, dataMaxs);
                BinStructure = initializeBinStructure(binRanges);
            }
            for (int i = 0; i < BinStructure.BinRanges.Count; i++)
            {
                binRanges[i] = BinStructure.BinRanges[i].GetUpdatedBinRange(dataMins[i], dataMaxs[i]);
            }

            BinStructure tempBinStructure = initializeBinStructure(binRanges);
            binSamples(tempBinStructure, dataRows);

            // re-map old bins
            tempBinStructure.Map(BinStructure);
            BinStructure = tempBinStructure;
        }

        private List<BinRange> createBinRanges(List<double> dataMins, List<double> dataMax)
        {
            List<BinRange> binRanges = new List<BinRange>();
            for (int d = 0; d < Dimensions.Count; d++)
            {
                BinRange scale = null;
                if (IsAxisAggregated[d])
                {
                    scale = AggregateBinRange.Initialize();
                }
                else if (AxisTypes[d] == AxisType.Time || AxisTypes[d] == AxisType.Date)
                {
                    scale = DateTimeBinRange.Initialize(dataMins[d], dataMax[d], NrOfBins[d]);
                }
                else if (AxisTypes[d] == AxisType.Quantitative)
                {
                    scale = QuantitativeBinRange.Initialize(dataMins[d], dataMax[d], NrOfBins[d], Dimensions[d].InputDataType == InputDataTypeConstants.INT);
                }
                else
                {
                    scale = NominalBinRange.Initialize(dataMins[d], dataMax[d], NrOfBins[d]);
                }
                binRanges.Add(scale);
            }
            return binRanges;
        }

        private void getMinMaxValuesFromSamples(List<DataRow> dataRows, List<double> dataMins, List<double> dataMaxs)
        {
            for (int d = 0; d < Dimensions.Count; d++)
            {
                var dimension = Dimensions[d];
                var dataMin = dataRows.Select(dp => dp.VisualizationValues[dimension]).Where(dp => dp.HasValue).Min(dp => dp.Value);
                var dataMax = dataRows.Select(dp => dp.VisualizationValues[dimension]).Where(dp => dp.HasValue).Max(dp => dp.Value);

                if (dataMax == dataMin)
                {
                    if (AxisTypes[d] != AxisType.Quantitative)
                    {
                        dataMin -= 0;
                        dataMax += 0.1;
                    }
                    else
                    {
                        dataMin -= 1;
                        dataMax += 1;
                    }
                }
                dataMins.Add(dataMin);
                dataMaxs.Add(dataMax);
            }
        }
        
        private void binSamples(BinStructure binStructure, List<DataRow> samples)
        {
            foreach (var sample in samples)
            {
                BinIndex binIndex = new BinIndex();

                for (int d = 0; d < binStructure.BinRanges.Count; d++)
                {
                    double? value = sample.VisualizationValues[Dimensions[d]];
                    binIndex.Indices.Add(value.HasValue ? binStructure.BinRanges[d].GetIndex(value.Value) : -1);
                }
                Bin bin = null;
                if (binStructure.Bins.TryGetValue(binIndex, out bin))
                {
                    bin.Samples.Add(sample);
                }
                else
                {
                    binStructure.NullCount++;
                }
            }
        }

        private BinStructure initializeBinStructure(List<BinRange> binRanges)
        {
            BinStructure binStructure = new BinStructure();
            binStructure.BinRanges = binRanges;

            Dictionary<BinIndex, Bin> bins = new Dictionary<BinIndex, Bin>();
            recursiveCreateBins(binRanges.ToArray().ToList(), bins, new List<Span>());
            binStructure.Bins = bins;

            return binStructure;
        }

        private void recursiveCreateBins(List<BinRange> previousBinRangesLeft, Dictionary<BinIndex, Bin> bins, List<Span> previousSpans)
        {
            List<BinRange> binRangesLeft = new List<BinRange>();
            binRangesLeft.AddRange(previousBinRangesLeft);

            BinRange binRange = binRangesLeft[0];
            binRangesLeft.RemoveAt(0);

            foreach (double x in binRange.GetBins())
            {
                Span span = new Span() { Min = x, Max = binRange.AddStep(x), Index = binRange.GetIndex(x) };
                List<Span> spans = new List<Span>();
                spans.AddRange(previousSpans);
                spans.Add(span);
                if (binRangesLeft.Count == 0)
                {
                    Bin bin = new Bin()
                    {
                        Spans = spans,
                        BinIndex = new BinIndex(spans.Select(s => s.Index).ToArray())
                    };
                    bins.Add(bin.BinIndex, bin);
                }
                else
                {
                    recursiveCreateBins(binRangesLeft, bins, spans);
                }
            }
        }
    }
}
