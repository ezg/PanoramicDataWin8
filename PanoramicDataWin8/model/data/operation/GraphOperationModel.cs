using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.Blueprints.Util.IO.GML;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class GraphOperationModel : BaseVisualizationOperationModel
    {
        TinkerGrapĥ _g;
        public string InputGraphFile;
        public GraphOperationModel(SchemaModel schemaModel, string inputGraphFile) : base(schemaModel)
        {
            InputGraphFile = inputGraphFile;

            IDEAAttributeModel.CodeDefinitionChangedEvent += TestForRefresh;

            _g = new TinkerGrapĥ();
            var gr = new GmlReader(_g);
            gr.InputGraph(inputGraphFile);
        }

        public TinkerGrapĥ Graph => _g;
        private void TestForRefresh(object sender)
        {
        }
        public override void Dispose()
        {
            IDEAAttributeModel.CodeDefinitionChangedEvent -= TestForRefresh;
            ResultCauserClone?.Dispose();
        }
        private void AttributeTransformationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ClearFilterModels();
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}