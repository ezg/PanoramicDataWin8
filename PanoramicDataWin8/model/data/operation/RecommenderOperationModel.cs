using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using IDEA_common.operations;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class RecommenderOperationModel : OperationModel
    {

        public RecommenderOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public override ResultParameters ResultParameters
        {
            get
            {
                return new RecommenderResultParameters()
                {
                    From = -1,
                    To = -1
                };
            }
        }
    }
}