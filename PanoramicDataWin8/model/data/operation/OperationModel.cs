using System;
using System.ComponentModel;
using IDEA_common.operations;
using Newtonsoft.Json;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data
{
    public interface IOperationModel : INotifyPropertyChanged
    {
        event OperationModel.OperationModelUpdatedHandler OperationModelUpdated;
        void FireOperationModelUpdated(OperationModelUpdatedEventArgs args);

        IResult Result { get; set; }

        IOperationModel ResultCauserClone { get; set; }

        SchemaModel SchemaModel { get; set; }

        OperationModel Clone();
    }


    [JsonObject(MemberSerialization.OptOut)]
    public class OperationModel : ExtendedBindableBase, IOperationModel
    {
        public delegate void OperationModelUpdatedHandler(object sender, OperationModelUpdatedEventArgs e);

        private static long _nextId;

        private long _id;

        private IResult _result;
        private IOperationModel _resultCauserClone;

        private SchemaModel _schemaModel;

        public OperationModel(SchemaModel schemaModel)
        {
            _schemaModel = schemaModel;

            _id = _nextId++;
        }

        public bool IsClone { get; set; }



        public SchemaModel SchemaModel
        {
            get { return _schemaModel; }
            set { SetProperty(ref _schemaModel, value); }
        }

        public long Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public  OperationModel Clone()
        {
            string serializedQueryModel = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
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
            int code = 0;
            code ^= Id.GetHashCode();
            return code;
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
        public IOperationModel ResultCauserClone
        {
            get { return _resultCauserClone; }
            set { _resultCauserClone = value; }
        }

        public event OperationModelUpdatedHandler OperationModelUpdated;
    }

    public class OperationModelUpdatedEventArgs : EventArgs
    {
    }

    public class VisualOperationModelUpdatedEventArgs : OperationModelUpdatedEventArgs
    {
    }
}