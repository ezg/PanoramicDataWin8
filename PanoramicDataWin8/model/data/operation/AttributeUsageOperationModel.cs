using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.model.data.operation
{
    public class AttributeUsageOperationModel : OperationModel
    {
        public AttributeUsageOperationModel(SchemaModel schemaModel) :base(schemaModel)
        {
            foreach (var inputUsage in Enum.GetValues(typeof(InputUsage)).Cast<InputUsage>())
            {
                _usageAttributeTransformationModels.Add(inputUsage, new ObservableCollection<AttributeTransformationModel>());
                _usageAttributeTransformationModels[inputUsage].CollectionChanged += InputOperationModel_CollectionChanged;
            }
        }

        void InputOperationModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((AttributeTransformationModel)item).PropertyChanged -= AttributeTransformationModel_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    ((AttributeTransformationModel)item).OperationModel = this;
                    ((AttributeTransformationModel)item).PropertyChanged += AttributeTransformationModel_PropertyChanged;
                }
            }
            FireOperationModelUpdated(new AttributeUsageOperationModelUpdatedEventArgs());
        }

        void AttributeTransformationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FireOperationModelUpdated(new AttributeUsageOperationModelUpdatedEventArgs());
        }
        

        private Dictionary<InputUsage, ObservableCollection<AttributeTransformationModel>> _usageAttributeTransformationModels = new Dictionary<InputUsage, ObservableCollection<AttributeTransformationModel>>();

        public Dictionary<InputUsage, ObservableCollection<AttributeTransformationModel>> UsageAttributeTransformationModels
        {
            get
            {
                return _usageAttributeTransformationModels;
            }
            set
            {
                this.SetProperty(ref _usageAttributeTransformationModels, value);
            }
        }

        [JsonIgnore]
        public List<AttributeTransformationModel> AttributeTransformationModels
        {
            get
            {
                List<AttributeTransformationModel> retList = new List<AttributeTransformationModel>();
                foreach (var key in _usageAttributeTransformationModels.Keys)
                {
                    retList.AddRange(_usageAttributeTransformationModels[key]);
                }
                return retList;
            }
        }

        public void AddUsageAttributeTransformationModel(InputUsage inputUsage, AttributeTransformationModel attributeTransformationModel)
        {
            _usageAttributeTransformationModels[inputUsage].Add(attributeTransformationModel);
        }

        public void RemoveUsageAttributeTransformationModel(InputUsage inputUsage, AttributeTransformationModel attributeTransformationModel)
        {
            _usageAttributeTransformationModels[inputUsage].Remove(attributeTransformationModel);
        }

        public void RemoveAttributeTransformationModel(AttributeTransformationModel attributeTransformationModel)
        {
            foreach (var key in _usageAttributeTransformationModels.Keys)
            {
                if (_usageAttributeTransformationModels[key].Any(aom => aom == attributeTransformationModel))
                {
                    RemoveUsageAttributeTransformationModel(key, attributeTransformationModel);
                }
            }
        }

        public ObservableCollection<AttributeTransformationModel> GetUsageAttributeTransformationModel(InputUsage inputUsage)
        {
            return _usageAttributeTransformationModels[inputUsage];
        }
    }

    public class AttributeUsageOperationModelUpdatedEventArgs : OperationModelUpdatedEventArgs
    {
    }
}
