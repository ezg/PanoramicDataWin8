using System.Collections.Generic;
using System.Linq;
using PanoramicDataWin8.model.data.result;
// ReSharper disable All

namespace PanoramicDataWin8.model.data
{
    public class FilterModel
    {
        public List<ValueComparison> ValueComparisons { get; set; }
        
        public FilterModel()
        {
            ValueComparisons = new List<ValueComparison>();
        }

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
                
                bool valueComp = Compare(this.ValueComparisons, compareTo.ValueComparisons);
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
            int count = 0;
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
    }

    public enum Predicate { EQUALS, LIKE, GREATER_THAN, LESS_THAN, GREATER_THAN_EQUAL, LESS_THAN_EQUAL }

    public class ValueComparison
    {
        public InputOperationModel InputOperationModel { get; set; }
        public object Value { get; set; }
        public Predicate Predicate { get; set; }

        public ValueComparison()
        {
        }

        public ValueComparison(InputOperationModel aom, Predicate predicate, object value)
        {
            this.InputOperationModel = aom;
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


        public bool Compare(object value)
        {
            if (this.Predicate == Predicate.EQUALS)
            {
                double d1 = 0.0;
                double d2 = 0.0;
                if (double.TryParse(this.Value.ToString(), out d1) &&
                    double.TryParse(value.ToString(), out d2))
                {
                    return d1 > d2 - 0.0001 && d1 < d2 + 0.0001;
                }
                else
                {
                    int cmp = value.ToString().CompareTo(this.Value.ToString());
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
                if (double.TryParse(this.Value.ToString(), out d1) &&
                    double.TryParse(value.ToString(), out d2))
                {
                    return d2 >= d1;
                }
                else
                {
                    int cmp = value.ToString().CompareTo(this.Value.ToString());
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
                if (double.TryParse(this.Value.ToString(), out d1) &&
                    double.TryParse(value.ToString(), out d2))
                {
                    return d2 <= d1;
                }

                else
                {
                    int cmp = value.ToString().CompareTo(this.Value.ToString());
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
