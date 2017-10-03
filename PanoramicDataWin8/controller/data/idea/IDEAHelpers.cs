using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IDEA_common.aggregates;
using IDEA_common.binning;
using IDEA_common.catalog;
using IDEA_common.operations;
using IDEA_common.operations.example;
using IDEA_common.operations.histogram;
using IDEA_common.operations.ml.optimizer;
using IDEA_common.operations.recommender;
using IDEA_common.operations.risk;
using IDEA_common.range;
using IDEA_common.util;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.controller.data.progressive
{
    public static class IDEAHelpers
    {
        public static AttributeParameters GetAttributeParameters(AttributeModel atm)
        {
            if (atm.FuncModel == null)
            {
                return new AttributeCodeParameters();
            }
            if (atm.FuncModel is AttributeModel.AttributeFuncModel.AttributeColumnFuncModel)
            {
                return new AttributeColumnParameters()
                {
                    RawName = atm.RawName,
                    VisualizationHints = atm.VisualizationHints
                };
            }
            if (atm.FuncModel is AttributeModel.AttributeFuncModel.AttributeGroupFuncModel)
            {
                return new AttributeCodeParameters();
            }
            if (atm.FuncModel is AttributeModel.AttributeFuncModel.AttributeBackendFuncModel)
            {
                return new AttributeBackendParameters()
                {
                    RawName = atm.RawName,
                    VisualizationHints = atm.VisualizationHints,
                    Id = ((AttributeModel.AttributeFuncModel.AttributeBackendFuncModel)atm.FuncModel).Id
                };
            }

            return new AttributeCodeParameters()
            {
                Code = ((AttributeModel.AttributeFuncModel.AttributeCodeFuncModel) atm.FuncModel).Code,
                RawName = atm.RawName,
                VisualizationHints = atm.VisualizationHints
            };
        }

        public static List<AttributeParameters> GetAttributeParameters(IEnumerable<AttributeModel> models)
        {
            return models.Select(am => GetAttributeParameters(am)).ToList();
        }

        public static AddComparisonParameters GetAddComparisonOperationParameters(StatisticalComparisonOperationModel model, int sampleSize)
        {
            var addComparison = new AddComparisonParameters
            {
                ComparisonOrder = model.ComparisonOrder,
                ModelId = model.ModelId
            };

            if (model.TestType == TestType.chi2)
            {
                addComparison.ChildOperationParameters = new List<OperationParameters>()
                {
                    new ChiSquaredTestOperationParameters
                    {
                        ChildOperationParameters = new List<OperationParameters>
                        {
                            GetHistogramOperationParameters(model.StatisticallyComparableOperationModels[0] as HistogramOperationModel, sampleSize),
                            GetHistogramOperationParameters(model.StatisticallyComparableOperationModels[1] as HistogramOperationModel, sampleSize)
                        }.OrderBy(h => (h as HistogramOperationParameters).Filter.Length).ToList()
                    }
                };
            }
            else if (model.TestType == TestType.ttest)
            {
                addComparison.ChildOperationParameters = new List<OperationParameters>()
                {
                    new TTestOperationParameters()
                    {
                        ChildOperationParameters = new List<OperationParameters>
                        {
                            GetEmpiricalDistOperationParameters(model.StatisticallyComparableOperationModels[0] as HistogramOperationModel, sampleSize),
                            GetEmpiricalDistOperationParameters(model.StatisticallyComparableOperationModels[1] as HistogramOperationModel, sampleSize)
                        }.OrderBy(h => (h as EmpiricalDistOperationParameters).Filter.Length).ToList()
                    }
                };
            }
            else if (model.TestType == TestType.corr)
            {
                addComparison.ChildOperationParameters = new List<OperationParameters>()
                {
                    new CorrelationTestOperationParameters()
                    {
                        ChildOperationParameters = new List<OperationParameters>
                        {
                            GetEmpiricalDistOperationParameters(
                                model.StatisticallyComparableOperationModels[0] as HistogramOperationModel,
                                model.StatisticallyComparableOperationModels[1] as HistogramOperationModel, sampleSize)
                        }.OrderBy(h => (h as EmpiricalDistOperationParameters).Filter.Length).ToList()
                    }
                };
            }

            return addComparison;
        }

        public static GetModelStateParameters GetModelStateParameters(StatisticalComparisonDecisionOperationModel model)
        {

            var getDecisionsParameters = new GetModelStateParameters()
            {
                ModelId = model.ModelId,
                ComparisonIds = model.ComparisonIds,
                RiskControlType = model.RiskControlType
            };
            return getDecisionsParameters;
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
                AttributeParameters = GetAttributeParameters(model.AttributeUsageTransformationModels.Select(atm => atm.AttributeModel)),
                DummyValue = model.DummyValue,
                ExampleType = model.ExampleOperationType.ToString(),
                SampleStreamBlockSize = sampleSize
            };
            return parameters;
        }

        public static EmpiricalDistOperationParameters GetEmpiricalDistOperationParameters(HistogramOperationModel model, int sampleSize)
        {
            var psm = model.SchemaModel as IDEASchemaModel;
            var filter = "";
            var filterModels = new List<FilterModel>();
            filter = FilterModel.GetFilterModelsRecursive(model, new List<IFilterProviderOperationModel>(), filterModels, true);
            
            var xIom = model.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();
            var yIom = model.GetAttributeUsageTransformationModel(AttributeUsage.Y).FirstOrDefault();
            

            var parameters = new EmpiricalDistOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                AttributeParameters = new[] { xIom, yIom }.Select(a => GetAttributeParameters(a.AttributeModel)).Distinct().ToList(),
                SampleStreamBlockSize = sampleSize,
                KeepSamples = false
            };
            return parameters;
        }

        public static EmpiricalDistOperationParameters GetEmpiricalDistOperationParameters(HistogramOperationModel m1, HistogramOperationModel m2, int sampleSize)
        {
            var psm = m1.SchemaModel as IDEASchemaModel;

            var filter1 = "";
            var filterModels = new List<FilterModel>();
            filter1 = FilterModel.GetFilterModelsRecursive(m1, new List<IFilterProviderOperationModel>(), filterModels, true);

            var filter2 = "";
            filterModels = new List<FilterModel>();
            filter2 = FilterModel.GetFilterModelsRecursive(m2, new List<IFilterProviderOperationModel>(), filterModels, true);

            var m1Iom = m1.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();
            var m2Iom = m2.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();


            var parameters = new EmpiricalDistOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = FilterModel.And(filter1, filter2),
                AttributeParameters = new[] { m1Iom, m2Iom }.Select(a => GetAttributeParameters(a.AttributeModel)).ToList(),
                SampleStreamBlockSize = sampleSize,
                KeepSamples = false
            };
            return parameters;
        }

        public static OptimizerOperationParameters GetOptimizerOperationParameters(PredictorOperationModel model, int sampleSize)
        {
            var filterModels = new List<FilterModel>();
            var filter = FilterModel.GetFilterModelsRecursive(model, new List<IFilterProviderOperationModel>(), filterModels, true);

            var psm = model.SchemaModel as IDEASchemaModel;
            var param = new OptimizerOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                LabelAttribute = GetAttributeParameters(model.TargetAttributeUsageTransformationModel.AttributeModel),
                ProblemType = ProblemType.Undefined,
                SampleStreamBlockSize = sampleSize,
                FeatureAttributes = psm.RootOriginModel.InputModels
                    .Where(im => im.DataType != DataType.String && im != model.TargetAttributeUsageTransformationModel.AttributeModel)
                    .Select(im => GetAttributeParameters(im)).ToList(),
                NrOfBanditRuns = 100,
                NrOfCrossValidations = 1,
                AttributeCalculatedParameters = IDEAAttributeModel.GetAllCalculatedAttributeModels().Select(a => GetAttributeParameters(a)).OfType<AttributeCaclculatedParameters>().ToList()
            };
            
            var aa = new AttributeCodeParameters();
            var bb = (AttributeCaclculatedParameters) aa;
            return param;
        }

        public static RecommenderOperationParameters GetRecommenderOperationParameters(RecommenderOperationModel model, int sampleSize)
        {
            var psm = model.SchemaModel as IDEASchemaModel;
            var param = new RecommenderOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                SampleStreamBlockSize = sampleSize, 
                ModelId = model.ModelId,
                ExcludeAttributeParameters = GetAttributeParameters(model.Exlude),
                IncludeAttributeParameters = GetAttributeParameters(model.Include),
                Target = GetHistogramOperationParameters(model.Target, sampleSize), 
                RiskControlType = HypothesesViewController.Instance.RiskOperationModel.RiskControlType,
                Budget = model.Budget
            };
            param.ExcludeAttributeParameters.Add(GetAttributeParameters(model.Target.GetAttributeUsageTransformationModel(AttributeUsage.X).Select(atm => atm.AttributeModel)).First());
            return param;
        }

        public static string GetHistogramRawOperationParameters(BaseVisualizationOperationModel model, out List<AttributeCaclculatedParameters> attributeCodeParameters, out List<string> brushes, out List<AttributeTransformationModel> aggregates)
        {
            attributeCodeParameters = new List<AttributeCaclculatedParameters>();
            brushes                 = new List<string>();

            var filterModels = new List<FilterModel>();
            var filter = FilterModel.GetFilterModelsRecursive(model, new List<IFilterProviderOperationModel>(), filterModels, true);
            foreach (var brushOperationModel in model.BrushOperationModels)
            {
                var brush = "";
                if ((brushOperationModel as IFilterProviderOperationModel).FilterModels.Any())
                {
                    var brushFilterModels = new List<FilterModel>();
                    brush = FilterModel.GetFilterModelsRecursive(brushOperationModel, new List<IFilterProviderOperationModel>(), brushFilterModels, false);
                    filterModels.AddRange(brushFilterModels);
                }
                brushes.Add(brush);
            }

             aggregates = model.GetAttributeUsageTransformationModel(AttributeUsage.Value).Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue)).Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).ToList();
            aggregates = aggregates.Distinct().ToList();

            attributeCodeParameters.AddRange(
                filterModels.SelectMany(fm => fm.ValueComparisons)
                    .Select(vc => vc.AttributeTransformationModel)
                    .Where((agg) => agg.AttributeModel.FuncModel is AttributeModel.AttributeFuncModel.AttributeCodeFuncModel)
                    .Select((agg) => GetAttributeParameters(agg.AttributeModel) as AttributeCodeParameters).Distinct());
            attributeCodeParameters.AddRange(aggregates.Where((agg) => agg.AttributeModel.FuncModel is AttributeModel.AttributeFuncModel.AttributeCodeFuncModel)
                .Select((agg) => GetAttributeParameters(agg.AttributeModel) as AttributeCodeParameters).Distinct().ToList());
            attributeCodeParameters = attributeCodeParameters.Distinct().ToList();
            return filter;
        }

        public static HistogramOperationParameters GetHistogramOperationParameters(BaseVisualizationOperationModel model, int sampleSize)
        {
            List<AttributeCaclculatedParameters>      attributeCodeParameters;
            List<string>                       brushes;
            List<AttributeTransformationModel> aggregates;
            var filter = GetHistogramRawOperationParameters(model, out attributeCodeParameters, out brushes, out aggregates);
            attributeCodeParameters = IDEAAttributeModel.GetAllCalculatedAttributeModels().Select(a => GetAttributeParameters(a)).OfType<AttributeCaclculatedParameters>().ToList();

            var nrOfBins = new List<double>();

            nrOfBins = new[] { MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins }.Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList();

            if ((model.GetAttributeUsageTransformationModel(AttributeUsage.X).First().AttributeModel.RawName == "long") ||
                (model.GetAttributeUsageTransformationModel(AttributeUsage.X).First().AttributeModel.RawName == "lat"))
                nrOfBins[0] = 20;

            if ((model.GetAttributeUsageTransformationModel(AttributeUsage.Y).First().AttributeModel.RawName == "long") ||
                (model.GetAttributeUsageTransformationModel(AttributeUsage.Y).First().AttributeModel.RawName == "lat"))
                nrOfBins[0] = 20;

            var xIom = model.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();
            var yIom = model.GetAttributeUsageTransformationModel(AttributeUsage.Y).FirstOrDefault();

            var xBinning = xIom.AggregateFunction == AggregateFunction.None
                ? new EquiWidthBinningParameters
                {
                    AttributeParameters = GetAttributeParameters( xIom.AttributeModel ),
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins, 
                }
                : (BinningParameters) new SingleBinBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(xIom.AttributeModel),
                };


            var yBinning = 
                yIom.AggregateFunction == AggregateFunction.None
                ? new EquiWidthBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(yIom.AttributeModel),
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfYBins
                }
                : (BinningParameters) new SingleBinBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(yIom.AttributeModel)
                };

            var psm = model.SchemaModel as IDEASchemaModel;
            AggregateParameters sortAggregateParam = null;
            var aggregateParameters = new List<AggregateParameters>();
            foreach (var agg in aggregates)
            {
                AggregateParameters aggParam = null;
                if (agg.AggregateFunction == AggregateFunction.Avg)
                {
                    aggParam = new AverageAggregateParameters
                    {
                        AttributeParameters = GetAttributeParameters(agg.AttributeModel),
                        DistinctAttributeParameters = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctAttributeParameters
                    };
                }
                else if (agg.AggregateFunction == AggregateFunction.Count)
                {
                    aggParam = new CountAggregateParameters
                    {
                        AttributeParameters = GetAttributeParameters(agg.AttributeModel),
                        DistinctAttributeParameters = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctAttributeParameters
                    };
                }
                else if (agg.AggregateFunction == AggregateFunction.Sum)
                {
                    aggParam = new SumAggregateParameters()
                    {
                        AttributeParameters = GetAttributeParameters(agg.AttributeModel),
                        DistinctAttributeParameters = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctAttributeParameters
                    };
                }
                else if (agg.AggregateFunction == AggregateFunction.SumE)
                {
                    aggParam = new SumEstimationAggregateParameters()
                    {
                        AttributeParameters = GetAttributeParameters(agg.AttributeModel),
                        DistinctAttributeParameters = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctAttributeParameters
                    };
                }
                aggregateParameters.Add(aggParam);

                if (agg == model.GetAttributeUsageTransformationModel(AttributeUsage.Value).Concat(model.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue)).FirstOrDefault())
                    sortAggregateParam = aggParam;

                aggregateParameters.Add(new MarginAggregateParameters
                {
                    AttributeParameters = GetAttributeParameters(agg.AttributeModel),
                    DistinctAttributeParameters = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctAttributeParameters,
                    AggregateFunction = agg.AggregateFunction
                });
            }

            var numericDataTypes = new[] {DataType.Int, DataType.Double, DataType.Float}.ToList();
            var globalAggregates = new List<AggregateParameters>();
            foreach (var index in new[] { xIom, yIom }.Where(a => numericDataTypes.Contains(a.AttributeModel.DataType)).Select(a => GetAttributeParameters(a.AttributeModel)).Distinct())
            {
                /*globalAggregates.Add(new KDEAggregateParameters
                {
                    Dimension = index,
                    NrOfSamples = 50,
                    DistinctDimension = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctDimension
                });
                globalAggregates.Add(new CountAggregateParameters
                {
                    Dimension = index,
                    DistinctDimension = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctDimension
                });*/
            }

            foreach (var iom in new[] { xIom, yIom }.Where(i => i.AggregateFunction == AggregateFunction.None && 
                numericDataTypes.Contains(i.AttributeModel.DataType)))
            {
                globalAggregates.Add(new AverageAggregateParameters()
                {
                    AttributeParameters = GetAttributeParameters(iom.AttributeModel)
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
                SortPerBinAggregateParameter = sortAggregateParam,
                GlobalAggregateParameters = globalAggregates,
                AttributeCalculatedParameters = attributeCodeParameters.OfType<AttributeCaclculatedParameters>().ToList()
            };
            return parameters;
        }

        public static HistogramOperationParameters GetRawDataOperationParameters(BaseVisualizationOperationModel model, int sampleSize)
        {
            List<AttributeCaclculatedParameters> attributeCodeParameters;
            List<AttributeTransformationModel> aggregates;
            aggregates = model.GetAttributeUsageTransformationModel(AttributeUsage.Value).Concat(
               model.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue)).Concat(
               model.GetAttributeUsageTransformationModel(AttributeUsage.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
               model.GetAttributeUsageTransformationModel(AttributeUsage.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).ToList();
            aggregates = aggregates.Distinct().ToList();
            attributeCodeParameters = IDEAAttributeModel.GetAllCalculatedAttributeModels().Select(a => GetAttributeParameters(a)).OfType<AttributeCaclculatedParameters>().ToList();

            var nrOfBins = new List<double>();

            nrOfBins = new[] { MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins }.Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.Group).Select(qom => MainViewController.Instance.MainModel.NrOfGroupBins)).ToList();

            if ((model.GetAttributeUsageTransformationModel(AttributeUsage.X).First().AttributeModel.RawName == "long") ||
                (model.GetAttributeUsageTransformationModel(AttributeUsage.X).First().AttributeModel.RawName == "lat"))
                nrOfBins[0] = 20;
            

            var xIom = model.GetAttributeUsageTransformationModel(AttributeUsage.X).FirstOrDefault();

            var xBinning = xIom.AggregateFunction == AggregateFunction.None
                ? new EquiWidthBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(xIom.AttributeModel),
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins,
                }
                : (BinningParameters)new SingleBinBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(xIom.AttributeModel),
                };
            

            var psm = model.SchemaModel as IDEASchemaModel;
            AggregateParameters sortAggregateParam = null;
            var aggregateParameters = new List<AggregateParameters>();
            foreach (var agg in aggregates)
            {
                AggregateParameters aggParam = null;
                aggParam = new AverageAggregateParameters
                {
                    AttributeParameters = GetAttributeParameters(agg.AttributeModel),
                    DistinctAttributeParameters = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctAttributeParameters
                };
                aggregateParameters.Add(aggParam);
            }

            var numericDataTypes = new[] { DataType.Int, DataType.Double, DataType.Float }.ToList();
            var globalAggregates = new List<AggregateParameters>();
            var axes = new[] { xIom };
         
            foreach (var iom in axes.Where(i => i.AggregateFunction == AggregateFunction.None &&
               numericDataTypes.Contains(i.AttributeModel.DataType)))
            {
                globalAggregates.Add(new AverageAggregateParameters()
                {
                    AttributeParameters = GetAttributeParameters(iom.AttributeModel)
                });
            }

            var parameters = new HistogramOperationParameters
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = "",
                Brushes = new List<string>(),
                BinningParameters = Extensions.Yield(xBinning, xBinning).ToList(),
                SampleStreamBlockSize = sampleSize,
                PerBinAggregateParameters = aggregateParameters,
                SortPerBinAggregateParameter = sortAggregateParam,
                GlobalAggregateParameters = globalAggregates,
                AttributeCalculatedParameters = attributeCodeParameters.OfType<AttributeCaclculatedParameters>().ToList()
            };
            return parameters;
        }

        private static AggregateParameters createAggregateParameters(AttributeTransformationModel iom)
        {
            if (iom.AggregateFunction == AggregateFunction.Count)
                return new CountAggregateParameters { AttributeParameters = GetAttributeParameters(iom.AttributeModel) };
            if (iom.AggregateFunction == AggregateFunction.Avg)
                return new AverageAggregateParameters { AttributeParameters = GetAttributeParameters(iom.AttributeModel) };
            if (iom.AggregateFunction == AggregateFunction.Max)
                return new MaxAggregateParameters { AttributeParameters = GetAttributeParameters(iom.AttributeModel) };
            if (iom.AggregateFunction == AggregateFunction.Min)
                return new MinAggregateParameters { AttributeParameters = GetAttributeParameters(iom.AttributeModel) };
            if (iom.AggregateFunction == AggregateFunction.Sum)
                return new SumAggregateParameters { AttributeParameters = GetAttributeParameters(iom.AttributeModel) };
            if (iom.AggregateFunction == AggregateFunction.Count)
                return new CountAggregateParameters { AttributeParameters = GetAttributeParameters(iom.AttributeModel) };
            if (iom.AggregateFunction == AggregateFunction.SumE)
                return new SumEstimationAggregateParameters() { AttributeParameters = GetAttributeParameters(iom.AttributeModel) };
            return null;
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom, HistogramResult histogramResult,
            int brushIndex)
        {
            return new AggregateKey
            {
                AggregateParameterIndex = histogramResult.GetAggregateParametersIndex(createAggregateParameters(iom)),
                BrushIndex = brushIndex
            };
        }

        public static AggregateKey CreateAggregateKey(AttributeTransformationModel iom,
            SingleDimensionAggregateParameters aggParameters, HistogramResult histogramResult, int brushIndex)
        {
            aggParameters.AttributeParameters = GetAttributeParameters(iom.AttributeModel);
            return new AggregateKey
            {
                AggregateParameterIndex = histogramResult.GetAggregateParametersIndex(aggParameters),
                BrushIndex = brushIndex
            };
        }

        public static AttributeModel GetAttributeModelFromAttribute(Attribute attribute)
        {
            var attributeModel = IDEAAttributeModel.AddColumnField(
                attribute.RawName,
                attribute.DisplayName, 
                attribute.DataType,
                attribute.DataType == DataType.String ? "enum" : "numeric",
                attribute.VisualizationHints);
            return attributeModel;
        }

        public static List<FilterModel> GetFilterModelsFromSelections(List<Selection> selections)
        {
            var ret = new List<FilterModel>();
            foreach (var selection in selections)
            {
                var fm = new FilterModel();
                foreach (var statement in selection.Statements)
                {
                    fm.ValueComparisons.Add(new ValueComparison()
                    {
                        AttributeTransformationModel = new AttributeTransformationModel(GetAttributeModelFromAttribute(statement.Attribute)),
                        Predicate = statement.Predicate,
                        Value = statement.Value
                    });
                }
                ret.Add(fm);
            }

            return ret;
        }

        public static FilterModel GetBinFilterModel(
            Bin bin, int brushIndex, HistogramResult histogramResult,
            AttributeTransformationModel xAom, AttributeTransformationModel yAom)
        {
            AttributeTransformationModel[] dimensions = new AttributeTransformationModel[] {xAom, yAom};
            FilterModel filterModel = new FilterModel();
            
            for (int i = 0; i < histogramResult.BinRanges.Count; i++)
            {
                if (!(histogramResult.BinRanges[i] is AggregateBinRange))
                {
                    var dataFrom = histogramResult.BinRanges[i].GetValueFromIndex(bin.BinIndex.Indices[i]);
                    var dataTo = histogramResult.BinRanges[i].AddStep(dataFrom);

                    if (histogramResult.BinRanges[i] is NominalBinRange)
                    {
                        var tt = histogramResult.BinRanges[i].GetLabel(dataFrom);

                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.EQUALS, tt));
                    }
                    else if (histogramResult.BinRanges[i] is AlphabeticBinRange)
                    {
                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.STARTS_WITH,
                            histogramResult.BinRanges[i].GetLabel(dataFrom)));
                    }
                    else
                    {
                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.GREATER_THAN_EQUAL, dataFrom));
                        filterModel.ValueComparisons.Add(new ValueComparison(dimensions[i], Predicate.LESS_THAN, dataTo));
                    }
                }
            }

            return filterModel;
        }

    }
}