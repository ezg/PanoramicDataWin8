using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.operation
{
    public class ValueComparison
    {
        public ValueComparison()
        {
        }

        public ValueComparison(AttributeTransformationModel aom, Predicate predicate, object value)
        {
            AttributeTransformationModel = aom;
            Value = value;
            Predicate = predicate;
        }

        public AttributeTransformationModel AttributeTransformationModel { get; set; }
        public object Value { get; set; }
        public Predicate Predicate { get; set; }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= Value.GetHashCode();
            code ^= Predicate.GetHashCode();
            return code;
        }

        public override bool Equals(object obj)
        {
            if (obj is ValueComparison)
            {
                var compareTo = obj as ValueComparison;
                return compareTo.Predicate.Equals(Predicate) && compareTo.Value.Equals(Value);
            }
            return false;
        }

        public string ToPythonString()
        {
            var op = "";
            switch (Predicate)
            {
                case Predicate.EQUALS:
                    op = "==";
                    break;
                case Predicate.GREATER_THAN:
                    op = ">";
                    break;
                case Predicate.GREATER_THAN_EQUAL:
                    op = ">=";
                    break;
                case Predicate.LESS_THAN:
                    op = "<";
                    break;
                case Predicate.LESS_THAN_EQUAL:
                    op = "<=";
                    break;
                default:
                    op = "==";
                    break;
            }
            var val = Value.ToString();
            if (Value is string)
                val = "\"" + val + "\"";
            if (Predicate != Predicate.STARTS_WITH)
            {
                var ret = " " + AttributeTransformationModel.AttributeModel.RawName + " " + op + " " + val + " ";
                return ret;
            }
            else
            {
                var ret = " " + AttributeTransformationModel.AttributeModel.RawName + ".StartsWith(" + val + ") ";
                 return ret;
            }
        }

        /*public bool Compare(object value)
        {
            if (Predicate == Predicate.EQUALS)
            {
                var d1 = 0.0;
                var d2 = 0.0;
                if (double.TryParse(Value.ToString(), out d1) &&
                    double.TryParse(value.ToString(), out d2))
                    return (d1 > d2 - 0.0001) && (d1 < d2 + 0.0001);
                var cmp = value.ToString().CompareTo(Value.ToString());
                if (cmp == 0)
                    return true;
            }
            else if (Predicate == Predicate.GREATER_THAN_EQUAL)
            {
                var d1 = 0.0;
                var d2 = 0.0;
                if (double.TryParse(Value.ToString(), out d1) &&
                    double.TryParse(value.ToString(), out d2))
                    return d2 >= d1;
                var cmp = value.ToString().CompareTo(Value.ToString());
                if ((cmp == 1) || (cmp == 0))
                    return true;
            }
            else if (Predicate == Predicate.LESS_THAN_EQUAL)
            {
                var d1 = 0.0;
                var d2 = 0.0;
                if (double.TryParse(Value.ToString(), out d1) &&
                    double.TryParse(value.ToString(), out d2))
                    return d2 <= d1;

                var cmp = value.ToString().CompareTo(Value.ToString());
                if ((cmp == -1) || (cmp == 0))
                    return true;
            }

            return false;
        }*/
    }
}