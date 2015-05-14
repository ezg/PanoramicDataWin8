using System.Collections.Generic;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.controller.data;

namespace PanoramicDataWin8.model.data
{
    public abstract class SchemaModel : BindableBase
    {
        public abstract List<OriginModel> OriginModels
        {
            get;
        }

        public abstract Dictionary<CalculatedAttributeModel, string> CalculatedAttributeModels
        {
            get;
        }

        public abstract Dictionary<NamedAttributeModel, string> NamedAttributeModels
        {
            get;
        }

        public abstract QueryExecuter QueryExecuter
        {
            get;
            set;
        }
    }
}
