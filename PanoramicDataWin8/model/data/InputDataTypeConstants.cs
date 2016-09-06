using System;
using IDEA_common.catalog;

namespace PanoramicDataWin8.model.data
{
    public class InputDataTypeConstants
    {
        public static string NVARCHAR = "nvarchar";
        public static string BIT = "bit";
        public static string DATE = "date";
        public static string FLOAT = "float";
        public static string GEOGRAPHY = "geography";
        public static string INT = "int";
        public static string TIME = "time";
        public static string GUID = "uniqueidentifier";

        public static string FromType(Type type)
        {
            if (type == typeof (float) ||
                type == typeof(double))
            {
                return FLOAT;
            }
            if (type == typeof(DateTime))
            {
                return DATE;
            }
            if (type == typeof(int))
            {
                return INT;
            }
            return NVARCHAR;
        }

        public static string FromDataType(DataType type)
        {
            if (type == DataType.Float ||
                type == DataType.Double)
            {
                return FLOAT;
            }
            if (type == DataType.DateTime)
            {
                return DATE;
            }
            if (type == DataType.Int)
            {
                return INT;
            }
            return NVARCHAR;
        }
    }
}