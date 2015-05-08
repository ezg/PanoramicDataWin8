using PanoramicData.model.data;
using PanoramicDataWin8.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.controller.data.sim
{
    public class DataAggregator
    {
        public void AggregateStep(DataBinStructure dataBinStructure, QueryModel queryModel)
        {
            double maxCount = double.MinValue;
            dataBinStructure.MaxValues.Clear();
            dataBinStructure.MinValues.Clear();

            var groupers = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Group);
            var aggregates = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Value).Concat(
                             queryModel.GetFunctionAttributeOperationModel(AttributeFunction.DefaultValue)).Concat(
                             queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                             queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).ToList();

            // update aggregations and counts
            foreach (var bin in dataBinStructure.Bins.SelectMany(b => b))
            {
                bin.Count += bin.Samples.Count;
                maxCount = Math.Max(bin.Count, maxCount);

                if (aggregates.Count() > 0)
                {
                    foreach (var aggregator in aggregates)
                    {
                        foreach (var sample in bin.Samples)
                        {
                            GroupingObject sampleGroupingObject = new GroupingObject();
                            groupers.Each((grouper, i) =>
                            {
                                sampleGroupingObject.Add(grouper, sample.Entries[grouper.AttributeModel]);
                            });
                            update(bin, sample, sampleGroupingObject, aggregator, queryModel);
                        }
                    }
                }
            }

            // update max / min 
            foreach (var bin in dataBinStructure.Bins.SelectMany(b => b))
            {
                foreach (var groupingObject in bin.Values.Keys)
                {
                    foreach (var aggregator in aggregates)
                    {
                        if (bin.Values[groupingObject][aggregator].HasValue)
                        {
                            if (!dataBinStructure.MaxValues.ContainsKey(aggregator))
                            {
                                dataBinStructure.MaxValues.Add(aggregator, double.MinValue);
                                dataBinStructure.MinValues.Add(aggregator, double.MaxValue);
                            }
                            dataBinStructure.MaxValues[aggregator] = Math.Max(dataBinStructure.MaxValues[aggregator], bin.Values[groupingObject][aggregator].Value);
                            dataBinStructure.MinValues[aggregator] = Math.Min(dataBinStructure.MinValues[aggregator], bin.Values[groupingObject][aggregator].Value);
                        }
                    }
                }
            }

            // normalized values
            foreach (var bin in dataBinStructure.Bins.SelectMany(b => b))
            {
                bin.NormalizedCount = bin.Count / maxCount;
                if (aggregates.Count() > 0)
                {
                    foreach (var aggregator in aggregates)
                    {
                        foreach (var groupingObject in bin.Values.Keys)
                        {
                            if ((dataBinStructure.MaxValues[aggregator] - dataBinStructure.MinValues[aggregator]) != 0.0)
                            {
                                bin.NormalizedValues[groupingObject][aggregator] =
                                    (bin.Values[groupingObject][aggregator] - dataBinStructure.MinValues[aggregator]) / (dataBinStructure.MaxValues[aggregator] - dataBinStructure.MinValues[aggregator]);
                            }
                            else
                            {
                                bin.NormalizedValues[groupingObject][aggregator] = 1.0;
                            }
                        }
                    }
                }
            }
        }

        private void update(Bin bin, DataRow sample, GroupingObject sampleGroupingObject, AttributeOperationModel aggregator, QueryModel queryModel)
        {
            double? currentValue = null;
            double? sampleValue = null;

            double d = 0;
            if (aggregator.AggregateFunction == AggregateFunction.Count) {
                sampleValue = 0;
            }
            else if (double.TryParse(sample.Entries[aggregator.AttributeModel].ToString(), out d))
            {
                sampleValue = d;
            }

            if (bin.Values.ContainsKey(sampleGroupingObject) && bin.Values[sampleGroupingObject].ContainsKey(aggregator))
            {
                currentValue = bin.Values[sampleGroupingObject][aggregator];
            }
            else
            {
                currentValue = sampleValue;
            }

            if (!bin.Counts.ContainsKey(sampleGroupingObject)) 
            {
                bin.Counts.Add(sampleGroupingObject, new Dictionary<AttributeOperationModel,double>());
            }
            if (!bin.Counts[sampleGroupingObject].ContainsKey(aggregator)) 
            {
                bin.Counts[sampleGroupingObject].Add(aggregator, 0);
            }

            if (sampleValue.HasValue)
            {
                if (aggregator.AggregateFunction == AggregateFunction.Max)
                {
                    currentValue = new double?[] { currentValue, sampleValue }.Max();
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Min)
                {
                    currentValue = new double?[] { currentValue, sampleValue }.Min();
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Avg)
                {
                    currentValue = ((currentValue * bin.Counts[sampleGroupingObject][aggregator]) + sampleValue) / (bin.Counts[sampleGroupingObject][aggregator] + 1);
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Sum)
                {
                    currentValue = currentValue + sampleValue;
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Count)
                {
                    currentValue = currentValue + 1;
                }
                else
                {
                    currentValue = ((currentValue * bin.Counts[sampleGroupingObject][aggregator]) + sampleValue) / (bin.Counts[sampleGroupingObject][aggregator] + 1);
                    //currentValue = currentValue + 1;
                }
            }


            if (!bin.Values.ContainsKey(sampleGroupingObject))
            {
                bin.Values.Add(sampleGroupingObject, new Dictionary<AttributeOperationModel, double?>());
                bin.NormalizedValues.Add(sampleGroupingObject, new Dictionary<AttributeOperationModel, double?>());
            }
            if (!bin.Values[sampleGroupingObject].ContainsKey(aggregator))
            {
                bin.Values[sampleGroupingObject].Add(aggregator, 0);
                bin.NormalizedValues[sampleGroupingObject].Add(aggregator, 0);
            } 
            bin.Values[sampleGroupingObject][aggregator] = currentValue;
            bin.Counts[sampleGroupingObject][aggregator] += 1;
        }
    }
}
