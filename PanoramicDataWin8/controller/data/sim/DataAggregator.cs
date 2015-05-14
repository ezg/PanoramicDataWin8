using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.data.common;

namespace PanoramicDataWin8.controller.data.sim
{
    public class DataAggregator
    {
        public void AggregateStep(BinStructure binStructure, QueryModel queryModel, double progress)
        {
            double maxCount = double.MinValue;
            binStructure.AggregatedMaxValues.Clear();
            binStructure.AggregatedMinValues.Clear();

            var aggregates = queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Value).Concat(
                             queryModel.GetFunctionAttributeOperationModel(AttributeFunction.DefaultValue)).Concat(
                             queryModel.GetFunctionAttributeOperationModel(AttributeFunction.X).Where(aom => aom.AggregateFunction != AggregateFunction.None)).Concat(
                             queryModel.GetFunctionAttributeOperationModel(AttributeFunction.Y).Where(aom => aom.AggregateFunction != AggregateFunction.None)).ToList();

            // update aggregations and counts
            foreach (var bin in binStructure.Bins.Values)
            {
                bin.Count += bin.Samples.Count;
                maxCount = Math.Max(bin.Count, maxCount);

                if (aggregates.Count() > 0)
                {
                    foreach (var aggregator in aggregates)
                    {
                        foreach (var sample in bin.Samples)
                        {
                            update(bin, sample, aggregator, queryModel, progress);
                        }
                    }
                }
            }

            // update max / min 
            foreach (var bin in binStructure.Bins.Values)
            {
                foreach (var aggregator in aggregates)
                {
                    if (bin.Values.ContainsKey(aggregator) && bin.Values[aggregator].HasValue)
                    {
                        if (!binStructure.AggregatedMaxValues.ContainsKey(aggregator))
                        {
                            binStructure.AggregatedMaxValues.Add(aggregator, double.MinValue);
                            binStructure.AggregatedMinValues.Add(aggregator, double.MaxValue);
                        }
                        binStructure.AggregatedMaxValues[aggregator] = Math.Max(binStructure.AggregatedMaxValues[aggregator], bin.Values[aggregator].Value);
                        binStructure.AggregatedMinValues[aggregator] = Math.Min(binStructure.AggregatedMinValues[aggregator], bin.Values[aggregator].Value);
                    }
                }
            }

            // normalized values
            foreach (var bin in binStructure.Bins.Values)
            {
                bin.NormalizedCount = bin.Count / maxCount;
                if (aggregates.Count() > 0)
                {
                    foreach (var aggregator in bin.Values.Keys)
                    {
                        if (binStructure.AggregatedMaxValues.ContainsKey(aggregator) && binStructure.AggregatedMinValues.ContainsKey(aggregator) &&
                            (binStructure.AggregatedMaxValues[aggregator] - binStructure.AggregatedMinValues[aggregator]) != 0.0)
                        {
                            bin.NormalizedValues[aggregator] =
                                (bin.Values[aggregator] - binStructure.AggregatedMinValues[aggregator]) / (binStructure.AggregatedMaxValues[aggregator] - binStructure.AggregatedMinValues[aggregator]);
                        }
                        else
                        {
                            bin.NormalizedValues[aggregator] = 1.0;
                        }
                    }
                }
            }
        }

        private void update(Bin bin, DataRow sample, AttributeOperationModel aggregator, QueryModel queryModel, double progress)
        {
            double? currentValue = null;
            double? sampleValue = null;
            object currentTempValue = null;

            double d = 0;
            if (aggregator.AggregateFunction == AggregateFunction.Count) 
            {
                sampleValue = 0;
                currentTempValue = 0d;
            }
            else if (double.TryParse(sample.Entries[aggregator.AttributeModel].ToString(), out d))
            {
                sampleValue = d;
            }

            if (bin.Values.ContainsKey(aggregator))
            {
                currentValue = bin.Values[aggregator];
            }
            else
            {
                currentValue = sampleValue;
            }

            if (bin.TemporaryValues.ContainsKey(aggregator))
            {
                currentTempValue = bin.TemporaryValues[aggregator];
            }

            if (!bin.Counts.ContainsKey(aggregator)) 
            {
                bin.Counts.Add(aggregator, 0);
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
                    currentValue = ((currentValue * bin.Counts[aggregator]) + sampleValue) / (bin.Counts[aggregator] + 1);
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Sum)
                {
                    currentValue = currentValue + sampleValue;
                }
                else if (aggregator.AggregateFunction == AggregateFunction.Count)
                {
                    currentTempValue = (double)currentTempValue + 1;
                    currentValue = progress < 1.0 ? (double)currentTempValue / progress : (double)currentTempValue;
                }
                else
                {
                    currentValue = ((currentValue * bin.Counts[aggregator]) + sampleValue) / (bin.Counts[aggregator] + 1);
                    //currentValue = currentValue + 1;
                }
            }

            if (!bin.Values.ContainsKey(aggregator))
            {
                bin.Values.Add(aggregator, 0);
                bin.TemporaryValues.Add(aggregator, null);
                bin.NormalizedValues.Add(aggregator, 0);
            } 

            bin.Values[aggregator] = currentValue;
            bin.TemporaryValues[aggregator] = currentTempValue;
            bin.Counts[aggregator] += 1;
        }
    }
}
