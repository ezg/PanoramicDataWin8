using System.Collections.Generic;
using System.Linq;
using IDEA_common.aggregates;
using IDEA_common.binning;
using IDEA_common.operations.example;
using IDEA_common.operations.histogram;
using IDEA_common.operations.risk;
using IDEA_common.util;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;

namespace PanoramicDataWin8.controller.data.progressive
{
    public static class IDEAHelpers
    {
        public static ChiSquaredTestOperationParameters GetChiSquaredTestOperationParameters(StatisticalComparisonOperationModel model, int sampleSize)
        {
            var psm = model.SchemaModel as IDEASchemaModel;
            var parameters = new ChiSquaredTestOperationParameters
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                SampleStreamBlockSize = sampleSize,
                DistObserved = GetHistogramOperationParameters(model.StatisticallyComparableOperationModels[0] as HistogramOperationModel, sampleSize),
                DistTarget = GetHistogramOperationParameters(model.StatisticallyComparableOperationModels[1] as HistogramOperationModel, sampleSize)
            };
            return parameters;
        }

        public static ExampleOperationParameters GetExampleOperationParameters(ExampleOperationModel model, int sampleSize)
        {
            var psm = model.SchemaModel as IDEASchemaModel;
            var filter = "";
            var filterModels = new List<FilterModel>();
            filter = FilterModel.GetFilterModelsRecursive(model, new List<IFilterProviderOperationModel>(), filterModels, true);

            var parameters = new ExampleOperationParameters
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Dimensions = model.AttributeUsageTransformationModels.Select(atm => atm.AttributeModel.Index).ToList(),
                DummyValue = model.DummyValue,
                ExampleType = model.ExampleOperationType.ToString(),
                SampleStreamBlockSize = sampleSize
            };
            return parameters;
        }

        public static HistogramOperationParameters GetHistogramOperationParameters(HistogramOperationModel model, int sampleSize)
        {
            var psm = model.SchemaModel as IDEASchemaModel;
            var filter = "";
            var filterModels = new List<FilterModel>();
            filter = FilterModel.GetFilterModelsRecursive(model, new List<IFilterProviderOperationModel>(), filterModels, true);

            var brushes = new List<string>();

            foreach (var brushOperationModel in model.BrushOperationModels)
            {
                filterModels = new List<FilterModel>();
                var brush = FilterModel.GetFilterModelsRecursive(brushOperationModel, new List<IFilterProviderOperationModel>(), filterModels, false);
                brushes.Add(brush);
            }

            var nrOfBins = new List<double>();

            nrOfBins = new[] {MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins}.Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList();

            if ((model.GetAttributeUsageTransformationModel(AttributeUsage.X).First().AttributeModel.RawName == "long") ||
                (model.GetAttributeUsageTransformationModel(AttributeUsage.X).First().AttributeModel.RawName == "lat"))
                nrOfBins[0] = 20;

            if ((model.GetAttributeUsageTransformationModel(AttributeUsage.Y).First().AttributeModel.RawName == "long") ||
                (model.GetAttributeUsageTransformationModel(AttributeUsage.Y).First().AttributeModel.RawName == "lat"))
                nrOfBins[0] = 20;

            var aggregates = model.GetAttributeUsageTransformationModel(AttributeUsage.Value).Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue)).Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Distinct().ToList();

            var xIom = model.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();
            var yIom = model.GetAttributeUsageTransformationModel(AttributeUsage.Y).FirstOrDefault();

            var xBinning = xIom.AggregateFunction == AggregateFunction.None
                ? new EquiWidthBinningParameters
                {
                    Dimension = xIom.AttributeModel.Index,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins
                }
                : (BinningParameters) new SingleBinBinningParameters
                {
                    Dimension = xIom.AttributeModel.Index
                };


            var yBinning = yIom.AggregateFunction == AggregateFunction.None
                ? new EquiWidthBinningParameters
                {
                    Dimension = yIom.AttributeModel.Index,
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfYBins
                }
                : (BinningParameters) new SingleBinBinningParameters
                {
                    Dimension = yIom.AttributeModel.Index
                };

            var aggregateParameters = new List<AggregateParameters>();
            foreach (var agg in aggregates)
            {
                if (agg.AggregateFunction == AggregateFunction.Avg)
                    aggregateParameters.Add(new AverageAggregateParameters
                    {
                        Dimension = agg.AttributeModel.Index
                    });
                else if (agg.AggregateFunction == AggregateFunction.Count)
                    aggregateParameters.Add(new CountAggregateParameters
                    {
                        Dimension = agg.AttributeModel.Index
                    });

                aggregateParameters.Add(new MarginAggregateParameters
                {
                    Dimension = agg.AttributeModel.Index,
                    AggregateFunction = agg.AggregateFunction.ToString()
                });
            }

            var kdeDatatypes = new[] {InputDataTypeConstants.INT, InputDataTypeConstants.FLOAT}.ToList();
            var globalAggregates = new List<AggregateParameters>();
            foreach (var index in new[] {xIom, yIom}.Where(a => kdeDatatypes.Contains((a.AttributeModel as AttributeFieldModel).InputDataType)).Select(a => a.AttributeModel.Index).Distinct())
            {
                globalAggregates.Add(new KDEAggregateParameters
                {
                    Dimension = index,
                    NrOfSamples = 50
                });
                globalAggregates.Add(new CountAggregateParameters
                {
                    Dimension = index
                });
            }

            var parameters = new HistogramOperationParameters
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Brushes = brushes,
                BinningParameters = Extensions.Yield(xBinning, yBinning).ToList(),
                SampleStreamBlockSize = sampleSize,
                PerBinAggregateParameters = aggregateParameters,
                GlobalAggregateParameters = globalAggregates
            };
            return parameters;
        }

        public static AggregateParameters CreateAggregateParameters(AttributeTransformationModel iom)
        {
            if (iom.AggregateFunction == AggregateFunction.Count)
                return new CountAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Avg)
                return new AverageAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Max)
                return new MaxAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Min)
                return new MinAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Sum)
                return new SumAggregateParameters {Dimension = iom.AttributeModel.Index};
            if (iom.AggregateFunction == AggregateFunction.Count)
                return new CountAggregateParameters {Dimension = iom.AttributeModel.Index};
            return null;
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom, HistogramResult histogramResult,
            int brushIndex)
        {
            return new AggregateKey
            {
                AggregateParameterIndex = histogramResult.GetAggregateParametersIndex(CreateAggregateParameters(iom)),
                BrushIndex = brushIndex
            };
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom,
            SingleDimensionAggregateParameters aggParameters, HistogramResult histogramResult, int brushIndex)
        {
            aggParameters.Dimension = iom.AttributeModel.Index;
            return new AggregateKey
            {
                AggregateParameterIndex = histogramResult.GetAggregateParametersIndex(aggParameters),
                BrushIndex = brushIndex
            };
        }
    }
}