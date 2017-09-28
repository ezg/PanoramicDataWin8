﻿using IDEA_common.catalog;
using IDEA_common.operations;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class OperationModel : ExtendedBindableBase, IOperationModel
    {
        public delegate void OperationModelUpdatedHandler(object sender, OperationModelUpdatedEventArgs e);

        private static long _nextId;

        private long _id;

        private ExecutionState _executionState = ExecutionState.Stopped;

        private IResult _result;

        private SchemaModel _schemaModel;

        public OperationModel(SchemaModel schemaModel)
        {
            _schemaModel = schemaModel;

            _id = _nextId++;
        }

        public int ExecutionId { get; set; } = 0;

        public ExecutionState ExecutionState
        {
            get { return _executionState; }
            set { SetProperty(ref _executionState, value); }
        }

        [JsonIgnore]
        public int ResultExecutionId { get; set; } = 0;

        public long Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
        public virtual bool ResetFilterModelWhenInputLinksChange {  get { return true;  } }

        public SchemaModel SchemaModel
        {
            get { return _schemaModel; }
            set { SetProperty(ref _schemaModel, value); }
        }

        public virtual void Cleanup()
        {

        }

        public OperationModel Clone()
        {
            var serializedQueryModel = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.All
            });

            OperationModel deserializeObject = null;
            deserializeObject = JsonConvert.DeserializeObject<OperationModel>(serializedQueryModel, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Auto
            });
            return deserializeObject;
        }

        public virtual void FireOperationModelUpdated(OperationModelUpdatedEventArgs args)
        {
            OperationModelUpdated?.Invoke(this, args);
        }

        [JsonIgnore]
        public IResult Result
        {
            get { return _result; }
            set { SetProperty(ref _result, value); }
        }

        [JsonIgnore]
        public virtual ResultParameters ResultParameters
        {
            get
            {
                return new ResultParameters();
            }
        }

        [JsonIgnore]
        public IOperationModel ResultCauserClone { get; set; }

        public event OperationModelUpdatedHandler OperationModelUpdated;

        public override bool Equals(object obj)
        {
            if (obj is OperationModel)
            {
                var am = obj as OperationModel;
                return
                    am.Id.Equals(Id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= Id.GetHashCode();
            return code;
        }
    }

    public enum ExecutionState
    {
        Running,
        Stopped
    }

    public class AttributeUsageOperationModel : OperationModel {

        public AttributeUsageOperationModel(SchemaModel schemaModel) : base(schemaModel) { }

        public ObservableCollection<AttributeTransformationModel> AttributeUsageTransformationModels { get; } = new ObservableCollection<AttributeTransformationModel>();
    }

}