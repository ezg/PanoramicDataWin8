using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

        public double? Value { get; set; }
        public List<ValueComparison> ValueComparisons { get; set; }
        public string GroupAggregateComparisons { get; set; }

        public override int GetHashCode()
        {
            return ValueComparisons.Aggregate(0, (current, k) => current ^ k.GetHashCode());
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
                foreach (var linkModel in filterConsumer.LinkModels)
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
                        ret = "(" + ret + " &&  " + childrenJoined + ")";
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
}