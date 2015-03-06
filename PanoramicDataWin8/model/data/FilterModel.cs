using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class FilterModel
    {
        public Dictionary<AttributeOperationModel, ValueComparison> ValueComparisons { get; set; }
        
        public FilterModel()
        {
            ValueComparisons = new Dictionary<AttributeOperationModel, ValueComparison>();
        }

        public FilterModel(QueryResultItemModel queryResultItemModel)
        {
            ValueComparisons = new Dictionary<AttributeOperationModel, ValueComparison>();
            foreach (var k in queryResultItemModel.Values.Keys.Where(aom => aom.IsGrouped || aom.IsBinned))
            {
                ValueComparisons.Add(k, new ValueComparison(queryResultItemModel.Values[k], Predicate.EQUALS));
            }
            if (ValueComparisons.Count == 0)
            {
                foreach (var k in queryResultItemModel.Values.Keys.Where(aom => !(aom.IsGrouped || aom.IsBinned)))
                {
                    ValueComparisons.Add(k, new ValueComparison(queryResultItemModel.Values[k], Predicate.EQUALS));
                }
            }
        }

        public override int GetHashCode()
        {
            int code = 0;
            foreach (var k in ValueComparisons.Keys)
                code ^= k.GetHashCode() + ValueComparisons[k].GetHashCode();
            return code;
        }

        public override bool Equals(object obj)
        {
            FilterModel compareTo = null;
            if (obj is FilterModel)
            {
                compareTo = obj as FilterModel;
                bool groupComp = compare(
                    this.ValueComparisons.Where(kvp => kvp.Key.IsBinned || kvp.Key.IsBinned).ToDictionary(t => t.Key, t => t.Value),
                    compareTo.ValueComparisons.Where(kvp => kvp.Key.IsBinned || kvp.Key.IsBinned).ToDictionary(t => t.Key, t => t.Value));
                if (!groupComp)
                    return false;

                bool valueComp = compare(
                    this.ValueComparisons.Where(kvp => !(kvp.Key.IsBinned || kvp.Key.IsBinned)).ToDictionary(t => t.Key, t => t.Value),
                    compareTo.ValueComparisons.Where(kvp => !(kvp.Key.IsBinned || kvp.Key.IsBinned)).ToDictionary(t => t.Key, t => t.Value));
                if (!valueComp)
                    return false;

                return true;
            }
            return false;
        }

        public bool compare(Dictionary<AttributeOperationModel, ValueComparison> a, Dictionary<AttributeOperationModel, ValueComparison> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }
            foreach (AttributeOperationModel aom in a.Keys)
            {
                if (!b.ContainsKey(aom))
                {
                    return false;
                }
                if (!a[aom].Equals(b[aom]))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public enum Predicate { EQUALS, BETWEEN, LIKE, GREATER_THAN, LESS_THAN, GREATER_THAN_EQUAL, LESS_THAN_EQUAL }

    public class ValueComparison
    {
        public QueryResultItemValueModel Value { get; set; }
        public Predicate Predicate { get; set; }

        public ValueComparison()
        {
        }

        public ValueComparison(QueryResultItemValueModel value, Predicate predicate)
        {
            this.Value = value;
            this.Predicate = predicate;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= Value.GetHashCode();
            code ^= Predicate.GetHashCode();
            return code;
        }

        public override bool Equals(object obj)
        {
            if (obj is ValueComparison)
            {
                var compareTo = obj as ValueComparison;
                return compareTo.Predicate.Equals(this.Predicate) && compareTo.Value.Equals(this.Value);
            }
            return false;
        }


        public bool Compare(QueryResultItemValueModel value)
        {
            if (this.Predicate == Predicate.EQUALS)
            {
                double d1 = 0.0;
                double d2 = 0.0;
                if (double.TryParse(this.Value.StringValue, out d1) &&
                    double.TryParse(value.StringValue, out d2))
                {
                    return d1 > d2 - 0.0001 && d1 < d2 + 0.0001;
                }
                else
                {
                    int cmp = value.StringValue.CompareTo(this.Value.StringValue);
                    if (cmp == 0)
                    {
                        return true;
                    }
                }
            }
            else if (this.Predicate == Predicate.GREATER_THAN_EQUAL)
            {
                double d1 = 0.0;
                double d2 = 0.0;
                if (double.TryParse(this.Value.StringValue, out d1) &&
                    double.TryParse(value.StringValue, out d2))
                {
                    return d2 >= d1;
                }
                else
                {
                    int cmp = value.StringValue.CompareTo(this.Value.StringValue);
                    if (cmp == 1 || cmp == 0)
                    {
                        return true;
                    }
                }
            }
            else if (this.Predicate == Predicate.LESS_THAN_EQUAL)
            {
                double d1 = 0.0;
                double d2 = 0.0;
                if (double.TryParse(this.Value.StringValue, out d1) &&
                    double.TryParse(value.StringValue, out d2))
                {
                    return d2 <= d1;
                }

                else
                {
                    int cmp = value.StringValue.CompareTo(this.Value.StringValue);
                    if (cmp == -1 || cmp == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
