﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sanity;

// ReSharper disable All

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class FilterModel
    {
        public FilterModel()
        {
            ValueComparisons = new List<ValueComparison>();
            GroupAggregateComparisons = "";
        }
        
        public List<ValueComparison> ValueComparisons { get; set; }
        public string GroupAggregateComparisons { get; set; }

        public override int GetHashCode()
        {
            return ValueComparisons.Aggregate(0, (current, k) => current ^ k.GetHashCode());
        }
        public Range<double> ToRange()
        {
            if (ValueComparisons.Count == 2)
            {
                double? min = null, max = null;
                if (ValueComparisons[0].Predicate == IDEA_common.operations.recommender.Predicate.LESS_THAN ||
                    ValueComparisons[0].Predicate == IDEA_common.operations.recommender.Predicate.LESS_THAN_EQUAL)
                {
                    max = (double)ValueComparisons[0].Value;
                }
                if (ValueComparisons[0].Predicate == IDEA_common.operations.recommender.Predicate.GREATER_THAN ||
                    ValueComparisons[0].Predicate == IDEA_common.operations.recommender.Predicate.GREATER_THAN_EQUAL)
                {
                    min = (double)ValueComparisons[0].Value;
                }
                if (ValueComparisons[1].Predicate == IDEA_common.operations.recommender.Predicate.LESS_THAN ||
                    ValueComparisons[1].Predicate == IDEA_common.operations.recommender.Predicate.LESS_THAN_EQUAL)
                {
                    max = (double)ValueComparisons[1].Value;
                }
                if (ValueComparisons[1].Predicate == IDEA_common.operations.recommender.Predicate.GREATER_THAN ||
                    ValueComparisons[1].Predicate == IDEA_common.operations.recommender.Predicate.GREATER_THAN_EQUAL)
                {
                    min = (double)ValueComparisons[1].Value;
                }
                if (min != null && max != null)
                    return new Range<double>((double)min, (double)max);
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            FilterModel compareTo = null;
            if (obj is FilterModel)
            {
                compareTo = obj as FilterModel;
                var valueComp = Compare(this.ValueComparisons, compareTo.ValueComparisons);
                return valueComp;
            }
            return false;
        }

        public bool Compare(List<ValueComparison> a, List<ValueComparison> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }
            var count = 0;
            foreach (var valueComp in a)
            {
                if (!valueComp.Equals(b[count]))
                {
                    return false;
                }
                count++;
            }
            return true;
        }

        public string ToPythonString()
        {
            var ret = "(" + string.Join("&&", ValueComparisons.Select(vc => vc.ToPythonString())) + ")";
            return ret;
        }

        public static string And(params string[] filters)
        {
            var ret = string.Join(" && ", filters.Where(f => !string.IsNullOrEmpty(f)));
            return ret;
        }

        public static string GetFilterModelsRecursive(object filterGraphNode, List<IFilterProviderOperationModel> visitedFilterProviders, List<FilterModel> filterModels, bool isFirst)
        {
            var ret = "";
            if (filterGraphNode is IFilterProviderOperationModel)
            {
                var filterProvider = ((IFilterProviderOperationModel) filterGraphNode);
                visitedFilterProviders.Add(filterProvider);
                if (!isFirst && filterProvider.FilterModels.Count(fm => fm.ValueComparisons.Count > 0) > 0)
                {
                    filterModels.AddRange(filterProvider.FilterModels.Where(fm => fm.ValueComparisons.Count > 0));
                    if (filterProvider.FilterModels.Any(fm => fm.ValueComparisons.Count > 0))
                    {
                        if (filterProvider.FilterModels.Where(fm => fm.ValueComparisons.Count > 0).All(fm => fm.GroupAggregateComparisons == ""))
                        {
                            ret = "(" + string.Join(" || ", filterProvider.FilterModels.Select(fm => fm.ToPythonString())) + ")";
                        }
                        else
                        {
                            ret = "(" + filterProvider.FilterModels[0].ValueComparisons[0].AttributeTransformationModel.AttributeModel.RawName +
                                  " in (" + string.Join(",", filterProvider.FilterModels.Select(fm => fm.ValueComparisons[0]).Select(vc => "'" + vc.Value.ToString() + "'")) + "))";
                            //ret = "(" + string.Join(",", operationModel.FilterModels.Select(fm => fm.ValueComparisons[0].)) + ")";
                        }
                    }
                }
            }
            if (filterGraphNode is IFilterConsumerOperationModel)
            {
                var filterConsumer = ((IFilterConsumerOperationModel) filterGraphNode);
                var children = new List<string>();
                foreach (var linkModel in filterConsumer.ConsumerLinkModels)
                {
                    if (linkModel.FromOperationModel != null && !visitedFilterProviders.Contains(linkModel.FromOperationModel))
                    {
                        var child = GetFilterModelsRecursive(linkModel.FromOperationModel, visitedFilterProviders, filterModels, false);
                        if (child != "")
                        {
                            if (linkModel.IsInverted)
                            {
                                child = "! " + child;
                            }
                            children.Add(child);
                        }
                    }
                }

                var childrenJoined = string.Join(filterConsumer.FilteringOperation == FilteringOperation.AND ? " && " : " || ", children);
                if (children.Count > 0)
                {
                    if (ret != "")
                    {
                        ret = "(" + ret + " &&  (" + childrenJoined + "))";
                    }
                    else
                    {
                        ret = "(" + childrenJoined + ")";
                    }
                }
            }
            return ret;
        }
    }

    public class BczNormalization
    {
        public enum Scoping
        {
            MinToMax,
            ZeroToSum
        }
        public enum axis {
            None = 0,
            X = 1,
            Y = 2,
            XY = 3
        }
        public axis     Axis  { get; set; }
        public Scoping  Scope { get; set; }
        public BczNormalization() { Axis = BczNormalization.axis.None; }
    }


    [JsonObject(MemberSerialization.OptOut)]
    public class BczBinMapModel
    {
        public BczBinMapModel()
        {
        }
        public BczBinMapModel(double value, bool sortAxis)
        {
            Value = value;
            SortAxis = sortAxis;
        }

        public bool   SortAxis { get; set; }
        public bool   SortUp   { get; set; }
        public double Value    { get; set; }

        public override int GetHashCode()
        {
            return SortAxis.GetHashCode() ^ Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            BczBinMapModel compareTo = null;
            if (obj is BczBinMapModel)
            {
                compareTo = obj as BczBinMapModel;
                if (SortAxis == compareTo.SortAxis)// && Value == compareTo.Value) // bcz: if we can sort by row/col then include Value in the test
                {
                    return true;
                }
            }
            return false;
        }
    }
}