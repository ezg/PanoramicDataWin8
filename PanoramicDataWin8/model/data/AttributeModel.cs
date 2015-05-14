using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public abstract class AttributeModel : BindableBase
    {
        public AttributeModel()
        {
        }
        
        private OriginModel _originModel = null;
        public OriginModel OriginModel
        {
            get
            {
                return _originModel;
            }
            set
            {
                this.SetProperty(ref _originModel, value);
            }
        }

        private bool _isDisplayed = true;
        public bool IsDisplayed
        {
            get
            {
                return _isDisplayed;
            }
            set
            {
                this.SetProperty(ref _isDisplayed, value);
            }
        }

        public abstract string Name
        {
            get;
        }

        public abstract string AttributeVisualizationType
        {
            get;
        }

        public abstract string AttributeDataType
        {
            get;
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeModel)
            {
                var am = obj as AttributeModel;
                return
                    am.OriginModel.Equals(this.OriginModel) &&
                    am.Name.Equals(this.Name) &&
                    am.AttributeVisualizationType.Equals(this.AttributeVisualizationType) &&
                    am.AttributeDataType.Equals(this.AttributeDataType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.OriginModel.GetHashCode();
            code ^= this.Name.GetHashCode();
            code ^= this.AttributeVisualizationType.GetHashCode();
            code ^= this.AttributeDataType.GetHashCode();
            return code;
        }
    }

    public class AttributeDataTypeConstants
    {
        public static string NVARCHAR = "nvarchar";
        public static string BIT = "bit";
        public static string DATE = "date";
        public static string FLOAT = "float";
        public static string GEOGRAPHY = "geography";
        public static string INT = "int";
        public static string TIME = "time";
        public static string GUID = "uniqueidentifier";
    }

    public class AttributeVisualizationTypeConstants
    {
        public static string NUMERIC = "numeric";
        public static string DATE = "date";
        public static string TIME = "time";
        public static string GEOGRAPHY = "geography";
        public static string CATEGORY = "category";
        public static string ENUM = "enum";
        public static string ENUM_LONG = "enum_long";
        public static string BOOLEAN = "boolean";
    }
}
