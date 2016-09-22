using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Core;
using IDEA_common.aggregates;
using IDEA_common.binning;
using IDEA_common.operations;
using IDEA_common.operations.histogram;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.progressive;
using PanoramicDataWin8.model.data.result;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class HistogramOperationJob : OperationJob
    {
        public HistogramOperationJob(OperationModel operationModel, 
            HistogramOperationModel histogramOperationModelClone, 
            TimeSpan throttle, int sampleSize) : base(operationModel, throttle)
        {
            var psm = (histogramOperationModelClone.SchemaModel as ProgressiveSchemaModel);
            string filter = "";
            List<FilterModel> filterModels = new List<FilterModel>();
            filter = FilterModel.GetFilterModelsRecursive(histogramOperationModelClone, new List<IFilterProviderOperationModel>(), filterModels, true);
            
            List<string> brushes = new List<string>();
            
            foreach (var brushOperationModel in histogramOperationModelClone.BrushOperationModels)
            {
                filterModels = new List<FilterModel>();
                var brush = FilterModel.GetFilterModelsRecursive(brushOperationModel, new List<IFilterProviderOperationModel>(), filterModels, false);
                brushes.Add(brush);
            }

            List<double> nrOfBins = new List<double>();

            nrOfBins = new double[] {MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins}.Concat(
                histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList();

            if (histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).First().AttributeModel.RawName == "long" ||
                histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).First().AttributeModel.RawName == "lat")
            {
                nrOfBins[0] = 20;
            }

            if (histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).First().AttributeModel.RawName == "long" ||
               histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).First().AttributeModel.RawName == "lat")
            {
                nrOfBins[0] = 20;
            }

            var aggregates = histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Value).Concat(
                 histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.DefaultValue)).Concat(
                 histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                 histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Distinct().ToList();

            var xIom = histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.X).FirstOrDefault();
            var yIom = histogramOperationModelClone.GetUsageAttributeTransformationModel(InputUsage.Y).FirstOrDefault();

            BinningParameters xBinning = xIom.AggregateFunction == AggregateFunction.None
                ? (BinningParameters) new EquiWidthBinningParameters()
                {
                    Dimension = xIom.AttributeModel.Index,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins,
                }
                : (BinningParameters) new SingleBinBinningParameters()
                {
                    Dimension = xIom.AttributeModel.Index,
                };


            BinningParameters yBinning = yIom.AggregateFunction == AggregateFunction.None
                ? (BinningParameters)new EquiWidthBinningParameters()
                {
                    Dimension = yIom.AttributeModel.Index,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfYBins,
                }
                : (BinningParameters)new SingleBinBinningParameters()
                {
                    Dimension = yIom.AttributeModel.Index,
                };

            var aggregateParameters = new List<AggregateParameters>();
            foreach (var agg in aggregates)
            {
                if (agg.AggregateFunction == AggregateFunction.Avg)
                {
                    aggregateParameters.Add(new AverageAggregateParameters()
                    {
                        Dimension = agg.AttributeModel.Index
                    });
                }
                else if (agg.AggregateFunction == AggregateFunction.Count)
                {
                    aggregateParameters.Add(new CountAggregateParameters()
                    {
                        Dimension = agg.AttributeModel.Index
                    });
                }

                aggregateParameters.Add(new MarginAggregateParameters()
                {
                    Dimension = agg.AttributeModel.Index,
                    AggregateFunction = agg.AggregateFunction.ToString()
                });
            }

            var kdeDatatypes = new string[] {InputDataTypeConstants.INT, InputDataTypeConstants.FLOAT}.ToList();
            var globalAggregates = new List<AggregateParameters>();
            foreach (var index in aggregates.Where(a => kdeDatatypes.Contains((a.AttributeModel as AttributeFieldModel).InputDataType)).Select(a => a.AttributeModel.Index).Distinct())
            {
                globalAggregates.Add(new KDEAggregateParameters()
                {
                    Dimension = index,
                    NrOfSamples = 50
                });
                globalAggregates.Add(new CountAggregateParameters()
                {
                    Dimension = index
                });
            }

            OperationParameters = new HistogramOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Brushes = brushes,
                BinningParameters = IDEA_common.util.Extensions.Yield(xBinning, yBinning).ToList(),
                SampleStreamBlockSize = sampleSize,
                PerBinAggregateParameters = aggregateParameters,
                GlobalAggregateParameters = globalAggregates
            };
        }
       
    }
}
