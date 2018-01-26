using IDEA_common.catalog;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    public interface FunctionSubtypeModel {
        DataType DataType { get; }
        string   Name { get; }
        Dictionary<string, ObservableCollection<AttributeTransformationModel>> AttributeParameterGroups { get; }
        Dictionary<string, object> ValueParameters { get; }
        string   InputVisualizationType { get; }
        string   Code();
    }

    public class FunctionOperationModel : ComputationalOperationModel
    {
        public FunctionSubtypeModel FunctionSubtypeModel;

        public FunctionOperationModel(SchemaModel schemaModel, string rawName, FunctionSubtypeModel functionSubtypeModel, string displayName = null) : 
            base(schemaModel, "0", 
                functionSubtypeModel.DataType, 
                functionSubtypeModel.InputVisualizationType,
                rawName, 
                displayName == null ? rawName : displayName)
        {
            FunctionSubtypeModel = functionSubtypeModel;
            foreach (var p in functionSubtypeModel.AttributeParameterGroups)
                p.Value.CollectionChanged += _attributeUsageTransformationModels_CollectionChanged;
        }

        private void _attributeUsageTransformationModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            GetAttributeModel().SetCode(FunctionSubtypeModel.Code(), FunctionSubtypeModel.DataType);
            FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
        }
    }
}