﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.controller.data.progressive;
using IDEA_common.operations;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class RawDataOperationModel : BaseVisualizationOperationModel
    {
        public RawDataOperationModel(SchemaModel schemaModel) : base(schemaModel) { }
    }
}