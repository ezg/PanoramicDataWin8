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
using IDEA_common.operations.rawdata;

namespace PanoramicDataWin8.controller.data.progressive
{
    public static class IDEAHelpers
    {
        public static AttributeParameters GetAttributeParameters(AttributeTransformationModel atm) { return GetAttributeParameters(atm.AttributeModel);  }
        public static AttributeParameters GetAttributeParameters(AttributeModel am)
        {
            if (am.FuncModel == null)
            {
                return new AttributeCodeParameters();
            }
            if (am.FuncModel is AttributeModel.AttributeFuncModel.AttributeColumnFuncModel)
            {
                return new AttributeColumnParameters()
                {
                    RawName = am.RawName,
                    VisualizationHints = am.VisualizationHints
                };
            }
            if (am.FuncModel is AttributeModel.AttributeFuncModel.AttributeGroupFuncModel)
            {
                return new AttributeCodeParameters();
            }
            if (am.FuncModel is AttributeModel.AttributeFuncModel.AttributeBackendFuncModel)
            {
                return new AttributeBackendParameters()
                {
                    RawName = am.RawName,
                    VisualizationHints = am.VisualizationHints,
                    Id = ((AttributeModel.AttributeFuncModel.AttributeBackendFuncModel)am.FuncModel).Id
                };
            }

            return new AttributeCodeParameters()
            {
                Code = ((AttributeModel.AttributeFuncModel.AttributeCodeFuncModel) am.FuncModel).Code,
                RawName = am.RawName,
                VisualizationHints = am.VisualizationHints
            };
        }

        public static List<AttributeParameters> GetAttributeParameters(IEnumerable<AttributeTransformationModel> models) { return GetAttributeParameters(models.Select((atm) => atm.AttributeModel));  }
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
                AttributeParameters = GetAttributeParameters(model.AttributeTransformationModelParameters),
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

            var includes = model.AttributeTransformationModelParameters.Select(a => GetAttributeParameters(a.AttributeModel)).ToList();
            var excludes = model.IgnoredAttributeTransformationModels.Select(a => GetAttributeParameters(a.AttributeModel)).ToList();
            var all = psm.RootOriginModel.InputModels
                .Where(im => im != model.TargetAttributeUsageModel)
                .Select(im => GetAttributeParameters(im)).ToList();
            var used = all;
            if (excludes.Any())
            {
                used = used.Except(excludes).ToList();
            }
            if (includes.Any())
            {
                //used = all.Concat(includes).
            }

            var calculated = IDEAAttributeModel.GetAllCalculatedAttributeModels(psm.OriginModels.First())
                .Select(a => GetAttributeParameters(a)).OfType<AttributeCaclculatedParameters>().ToList();
            used = used.Except(calculated).ToList();

            var param = new OptimizerOperationParameters()
            {
                AdapterName = psm.RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                LabelAttribute = GetAttributeParameters(model.TargetAttributeUsageModel),
                ProblemType = ProblemType.Undefined,
                SampleStreamBlockSize = sampleSize,
                FeatureAttributes = used,
                NrOfBanditRuns = 100,
                NrOfCrossValidations = 1,
                AttributeCalculatedParameters = new List<AttributeCaclculatedParameters>()
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
                ExcludeAttributeParameters = GetAttributeParameters(model.Exlude.Select((am) => new AttributeTransformationModel(am))),
                IncludeAttributeParameters = GetAttributeParameters(model.Include.Select((am) => new AttributeTransformationModel(am))),
                Target = GetHistogramOperationParameters(model.Target, sampleSize), 
                RiskControlType = HypothesesViewController.Instance.RiskOperationModel.RiskControlType,
                Budget = model.Budget
            };
            param.ExcludeAttributeParameters.Add(GetAttributeParameters(model.Target.GetAttributeUsageTransformationModel(AttributeUsage.X).Select(atm => atm.AttributeModel)).First());
            return param;
        }

        public static string GetBaseOperationParameters(OperationModel model, out List<AttributeCaclculatedParameters> attributeCodeParameters, out List<string> brushes, List<object> brushOperationModels, List<AttributeTransformationModel> aggregates)
        {
            attributeCodeParameters = new List<AttributeCaclculatedParameters>();
            brushes                 = new List<string>();

            var filterModels = new List<FilterModel>();
            var filter = FilterModel.GetFilterModelsRecursive(model, new List<IFilterProviderOperationModel>(), filterModels, true);
            foreach (var brushOperationModel in brushOperationModels)
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

        public static HistogramOperationParameters GetRawDataComputedOperationParameters(RawDataOperationModel model, int sampleSize)
        {
            List<AttributeCaclculatedParameters> attributeCodeParameters;
            List<string> brushes;
            var aggregates = model.AttributeTransformationModelParameters.ToList();
            var filter = GetBaseOperationParameters(model, out attributeCodeParameters, out brushes, model.BrushOperationModels.Select((m) => (object)m).ToList(), aggregates);
            attributeCodeParameters = IDEAAttributeModel.GetAllCalculatedAttributeModels(model.SchemaModel.OriginModels.First()).Select(a => GetAttributeParameters(a)).OfType<AttributeCaclculatedParameters>().ToList();

            var nrOfBins = new List<double>();

            nrOfBins = new[] { MainViewController.Instance.MainModel.NrOfXBins, MainViewController.Instance.MainModel.NrOfYBins }.ToList();

            var groupBy = new List<BinningParameters>();
            foreach (var atm in model.AttributeTransformationModelParameters)
                if (atm.GroupBy)
                    groupBy.Add(new EquiWidthBinningParameters
                    {
                        AttributeParameters = GetAttributeParameters(atm.AttributeModel),
                        RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins,
                    });

            var aggregateParameters = getAggregateParameters(model, aggregates, model.AttributeTransformationModelParameters.Where((atm) => atm.GroupBy).ToList());

            var numericDataTypes = new[] { DataType.Int, DataType.Double, DataType.Float }.ToList();
            var globalAggregates = new List<AggregateParameters>();

            var parameters = new HistogramOperationParameters
            {
                AdapterName = (model.SchemaModel as IDEASchemaModel).RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Brushes = brushes,
                BinningParameters = groupBy,
                SampleStreamBlockSize = sampleSize,
                PerBinAggregateParameters = aggregateParameters,
                SortPerBinAggregateParameter = null,
                GlobalAggregateParameters = globalAggregates,
                AttributeCalculatedParameters = attributeCodeParameters.OfType<AttributeCaclculatedParameters>().ToList(),
                DegreeOfParallism = 4,
                IsCachable = false
            };
            return parameters;
        }

        public static HistogramOperationParameters GetHistogramOperationParameters(HistogramOperationModel model, int sampleSize)
        {
            List<AttributeCaclculatedParameters> attributeCodeParameters;
            List<string> brushes;
            List<AttributeTransformationModel> aggregates = model.GetAttributeUsageTransformationModel(AttributeUsage.Value).Concat(
                model.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue)).ToList();
            var filter = GetBaseOperationParameters(model, out attributeCodeParameters, out brushes, model.BrushOperationModels.Select((m) => (object)m).ToList(), aggregates);
            attributeCodeParameters = IDEAAttributeModel.GetAllCalculatedAttributeModels(model.SchemaModel.OriginModels.First()).Select(a => GetAttributeParameters(a)).OfType<AttributeCaclculatedParameters>().ToList();

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
                    AttributeParameters = GetAttributeParameters(xIom.AttributeModel),
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfXBins,
                }
                : (BinningParameters)new SingleBinBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(xIom.AttributeModel), // logically not needed, but needed
                };


            var yBinning =
                yIom.AggregateFunction == AggregateFunction.None
                ? new EquiWidthBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(yIom.AttributeModel),
                    RequestedNrOfBins = MainViewController.Instance.MainModel.NrOfYBins
                }
                : (BinningParameters)new SingleBinBinningParameters
                {
                    AttributeParameters = GetAttributeParameters(yIom.AttributeModel) // logically not needed, but needed
                };
            
            var aggregateParameters = getAggregateParameters(model, aggregates);

            var numericDataTypes = new[] { DataType.Int, DataType.Double, DataType.Float }.ToList();
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
                AdapterName = (model.SchemaModel as IDEASchemaModel).RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Brushes = brushes,
                BinningParameters = Extensions.Yield(xBinning, yBinning).ToList(),
                SampleStreamBlockSize = sampleSize,
                PerBinAggregateParameters = aggregateParameters,
                SortPerBinAggregateParameter = null,
                GlobalAggregateParameters = globalAggregates,
                AttributeCalculatedParameters = attributeCodeParameters.OfType<AttributeCaclculatedParameters>().ToList(),
                DegreeOfParallism = 4,
                IsCachable = false
            };
            return parameters;
        }

        private static List<AggregateParameters> getAggregateParameters(BaseVisualizationOperationModel model, List<AttributeTransformationModel> aggregates,
            List<AttributeTransformationModel> groupBy = null)
        {
            var psm = model.SchemaModel as IDEASchemaModel;
            var aggregateParameters = new List<AggregateParameters>();
            foreach (var agg in aggregates)
            {
                if (groupBy != null)
                {
                    if (groupBy.Contains(agg))
                        continue;
                    if (groupBy.Count == 0)
                        agg.AggregateFunction = AggregateFunction.None;
                    else if (agg.AggregateFunction == AggregateFunction.None)
                        agg.AggregateFunction = agg.AggregateFunctions.Last();
                }

                AggregateParameters aggParam = null;
                if (agg.AggregateFunction == AggregateFunction.Avg)
                {
                    aggParam = new AverageAggregateParameters
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
                aggregateParameters.Add(aggParam);

                if (groupBy == null)
                    aggregateParameters.Add(new MarginAggregateParameters
                    {
                        AttributeParameters = GetAttributeParameters(agg.AttributeModel),
                        DistinctAttributeParameters = psm.RootOriginModel.DatasetConfiguration.Schema.DistinctAttributeParameters,
                        AggregateFunction = agg.AggregateFunction
                    });
            }
            
            return aggregateParameters;
        }

        public static OperationParameters GetRawDataOperationParameters(RawDataOperationModel model, int sampleSize)
        {
            List<string> brushes;
            List<AttributeCaclculatedParameters> attributeCodeParameters;
            var aggregates = model.AttributeTransformationModelParameters.ToList();
            var filter       = GetBaseOperationParameters(model, out attributeCodeParameters, out brushes, model.BrushOperationModels.Select((m) => (object)m).ToList(), aggregates);
            attributeCodeParameters = IDEAAttributeModel.GetAllCalculatedAttributeModels(model.SchemaModel.OriginModels.First()).Select(a => GetAttributeParameters(a)).OfType<AttributeCaclculatedParameters>().ToList();
            var binnings = new List<BinningParameters>();
            foreach (var a in model.AttributeTransformationModelParameters)
            {
                binnings.Add( (BinningParameters)new SingleBinBinningParameters { AttributeParameters = GetAttributeParameters(a.AttributeModel)} );
            }
            var aggregateParameters = getAggregateParameters(model, aggregates, model.AttributeTransformationModelParameters.Where((atm) => atm.GroupBy).ToList());
            var parameters = new RawDataOperationParameters
            {
                AdapterName = (model.SchemaModel as IDEASchemaModel).RootOriginModel.DatasetConfiguration.Schema.RawName,
                Filter = filter,
                Brushes = new List<string>(),
                BinningParameters = binnings,
                SampleStreamBlockSize = sampleSize,
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

        public static AttributeModel GetAttributeModelFromAttribute(Attribute attribute, OriginModel originModel)
        {
            var attributeModel = IDEAAttributeModel.AddColumnField(
                attribute.RawName,
                attribute.DisplayName, 
                attribute.DataType,
                attribute.DataType == DataType.String ? "enum" : "numeric",
                attribute.VisualizationHints,
                originModel,
                attribute.IsTarget);
            return attributeModel;
        }

        public static List<FilterModel> GetFilterModelsFromSelections(List<Selection> selections, OriginModel originModel)
        {
            var ret = new List<FilterModel>();
            foreach (var selection in selections)
            {
                var fm = new FilterModel();
                foreach (var statement in selection.Statements)
                {
                    fm.ValueComparisons.Add(new ValueComparison()
                    {
                        AttributeTransformationModel = new AttributeTransformationModel(GetAttributeModelFromAttribute(statement.Attribute, originModel)),
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