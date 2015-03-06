using PanoramicData.model.data;
using PanoramicData.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.controller.data
{
    public abstract class QueryExecuter
    {
        public abstract void ExecuteQuery(QueryModel queryModel);
    }
}
