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

        public abstract Dictionary<CalculatedInputModel, string> CalculatedInputFieldModels
        {
            get;
        }

        public abstract Dictionary<NamedInputModel, string> NamedInputFieldModels
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
