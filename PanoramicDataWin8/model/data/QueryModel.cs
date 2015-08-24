using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class QueryModel : ExtendedBindableBase
    {
        private static long _nextId = 0;
        private bool _isClone = false;

        public delegate void QueryModelUpdatedHandler(object sender, QueryModelUpdatedEventArgs e);
        public event QueryModelUpdatedHandler QueryModelUpdated;
        
        public QueryModel(SchemaModel schemaModel, ResultModel resultModel)
        {
            _id = _nextId++;
            _schemaModel = schemaModel;
            _resultModel = resultModel;

            foreach (var inputUsage in Enum.GetValues(typeof(InputUsage)).Cast<InputUsage>())
            {
                _usageInputOperationModels.Add(inputUsage, new ObservableCollection<InputOperationModel>());
                _usageInputOperationModels[inputUsage].CollectionChanged += InputOperationModel_CollectionChanged;
            }

            _linkModels.CollectionChanged += LinkModels_CollectionChanged;
        }

        public QueryModel()
        {
            _isClone = true; // disable event firing. 
        }


        public QueryModel Clone()
        {
            ITraceWriter traceWriter = new MemoryTraceWriter();
            
            string serializedQueryModel = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
            });

            //Debug.WriteLine("");
            //Debug.WriteLine(traceWriter);

            traceWriter = new MemoryTraceWriter();
            QueryModel deserializedQueryModel = null;
            deserializedQueryModel = JsonConvert.DeserializeObject<QueryModel>(serializedQueryModel, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto
            });
            return deserializedQueryModel;
        }


        void LinkModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool fire = false;
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (Equals(((LinkModel) item).ToQueryModel, this))
                    {
                        ((LinkModel) item).FromQueryModel.QueryModelUpdated -= FromQueryModel_QueryModelUpdated;
                        fire = true;
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (Equals(((LinkModel) item).ToQueryModel, this))
                    {
                        ((LinkModel) item).FromQueryModel.QueryModelUpdated += FromQueryModel_QueryModelUpdated;
                        fire = true;
                    }
                }
            }
            if (fire)
            {
                FireQueryModelUpdated(QueryModelUpdatedEventType.Structure);
            }
        }

        void FromQueryModel_QueryModelUpdated(object sender, QueryModelUpdatedEventArgs e)
        {
            FireQueryModelUpdated(QueryModelUpdatedEventType.Links);
        }

        void InputOperationModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    (item as InputOperationModel).PropertyChanged -= InputOperationModel_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as InputOperationModel).QueryModel = this;
                    (item as InputOperationModel).PropertyChanged += InputOperationModel_PropertyChanged;
                }
            }
            FireQueryModelUpdated(QueryModelUpdatedEventType.Structure);
        }

        void InputOperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FireQueryModelUpdated(QueryModelUpdatedEventType.Structure);   
        }

        private long _id = 0;
        public long Id
        {
            get
            {
                return _id;
            }
            set
            {
                this.SetProperty(ref _id, value);
            }
        }

        
        private ResultModel _resultModel = null;
        [JsonIgnore]
        public ResultModel ResultModel
        {
            get
            {
                return _resultModel;
            }
            set
            {
                this.SetProperty(ref _resultModel, value);
            }
        }

        
        private SchemaModel _schemaModel = null;
        public SchemaModel SchemaModel
        {
            get
            {
                return _schemaModel;
            }
            set
            {
                this.SetProperty(ref _schemaModel, value);
            }
        }

        private FilteringOperation _filteringOperation = FilteringOperation.AND;
        public FilteringOperation FilteringOperation
        {
            get
            {
                return _filteringOperation;
            }
            set
            {
                this.SetProperty(ref _filteringOperation, value);
            }
        }

        private VisualizationType _visualizationType;
        public VisualizationType VisualizationType
        {
            get
            {
                return _visualizationType;
            }
            set
            {
                this.SetProperty(ref _visualizationType, value);
            }
        }

        private Dictionary<InputUsage, ObservableCollection<InputOperationModel>> _usageInputOperationModels = new Dictionary<InputUsage, ObservableCollection<InputOperationModel>>();
       
        public Dictionary<InputUsage, ObservableCollection<InputOperationModel>> UsageInputOperationModels
        {
            get
            {
                return _usageInputOperationModels;
            }
            set
            {
                this.SetProperty(ref _usageInputOperationModels, value);
            }
        }

        [JsonIgnore]
        public List<InputOperationModel> InputOperationModels
        {
            get
            {
                List<InputOperationModel> retList = new List<InputOperationModel>();
                foreach (var key in _usageInputOperationModels.Keys)
                {
                    retList.AddRange(_usageInputOperationModels[key]);
                }
                return retList;
            }
        }

        public void AddUsageInputOperationModel(InputUsage inputUsage, InputOperationModel inputOperationModel)
        {
            _usageInputOperationModels[inputUsage].Add(inputOperationModel);
        }

        public void RemoveUsageInputOperationModel(InputUsage inputUsage, InputOperationModel inputOperationModel)
        {
            _usageInputOperationModels[inputUsage].Remove(inputOperationModel);
        }

        public void RemoveInputOperationModel(InputOperationModel inputOperationModel)
        {
            foreach (var key in _usageInputOperationModels.Keys)
            {
                if (_usageInputOperationModels[key].Any(aom => aom == inputOperationModel))
                {
                    RemoveUsageInputOperationModel(key, inputOperationModel);
                }
            }
        }

        public ObservableCollection<InputOperationModel> GetUsageInputOperationModel(InputUsage inputUsage)
        {
            return _usageInputOperationModels[inputUsage];
        }

        private ObservableCollection<LinkModel> _linkModels = new ObservableCollection<LinkModel>();
        public ObservableCollection<LinkModel> LinkModels
        {
            get
            {
                return _linkModels;
            }
            set
            {
                this.SetProperty(ref _linkModels, value);
            }
        }

        private ObservableCollection<FilterModel> _filterModels = new ObservableCollection<FilterModel>();
        public ObservableCollection<FilterModel> FilterModels
        {
            get
            {
                return _filterModels;
            }
        }

        public void ClearFilterModels()
        {
            foreach (var filterModel in _filterModels.ToArray())
            {
                _filterModels.Remove(filterModel);
            }
            FireQueryModelUpdated(QueryModelUpdatedEventType.FilterModels);
        }

        public void AddFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterModel in filterModels)
            {
                _filterModels.Add(filterModel);
            }
            FireQueryModelUpdated(QueryModelUpdatedEventType.FilterModels);
        }

        public void AddFilterModel(FilterModel filterModel)
        {
            _filterModels.Add(filterModel);
            FireQueryModelUpdated(QueryModelUpdatedEventType.FilterModels);
        }

        public void RemoveFilterModel(FilterModel filterModel)
        {
            _filterModels.Remove(filterModel);
            FireQueryModelUpdated(QueryModelUpdatedEventType.FilterModels);
        }

        public void RemoveFilterModels(List<FilterModel> filterModels)
        {
            foreach (var filterItem in filterModels)
            {
                _filterModels.Remove(filterItem);
            }
            if (filterModels.Count > 0)
            {
                FireQueryModelUpdated(QueryModelUpdatedEventType.FilterModels);
            }
        }

        public void FireQueryModelUpdated(QueryModelUpdatedEventType type)
        {
            if (!_isClone)
            {
                if (type == QueryModelUpdatedEventType.Structure || type == QueryModelUpdatedEventType.Links)
                {
                    ClearFilterModels();
                }
                if (QueryModelUpdated != null)
                {
                    QueryModelUpdated(this, new QueryModelUpdatedEventArgs(type));
                }

                if (type != QueryModelUpdatedEventType.FilterModels && SchemaModel.QueryExecuter != null)
                {
                    SchemaModel.QueryExecuter.ExecuteQuery(this);
                }
            }
        }

        public AxisType GetAxisType(InputOperationModel inputOperationModel)
        {
            // determine axis type
            // some aggregation
            if (inputOperationModel.AggregateFunction != AggregateFunction.None)
            {
                if (inputOperationModel.AggregateFunction == AggregateFunction.Avg ||
                    inputOperationModel.AggregateFunction == AggregateFunction.Sum ||
                    inputOperationModel.AggregateFunction == AggregateFunction.Count)
                {
                    return AxisType.Quantitative;
                }
                else if (inputOperationModel.AggregateFunction == AggregateFunction.Max ||
                         inputOperationModel.AggregateFunction == AggregateFunction.Min) 
                {
                    if (((InputFieldModel) inputOperationModel.InputModel).InputDataType == InputDataTypeConstants.FLOAT ||
                        ((InputFieldModel)inputOperationModel.InputModel).InputDataType == InputDataTypeConstants.INT)
                    {
                        return AxisType.Quantitative;
                    }
                }
                else 
                {
                    return AxisType.Ordinal;
                }
            }
            // no aggrgation
            else
            {
                if (((InputFieldModel) inputOperationModel.InputModel).InputDataType == InputDataTypeConstants.FLOAT ||
                    ((InputFieldModel) inputOperationModel.InputModel).InputDataType == InputDataTypeConstants.INT)
                {
                    if (((InputFieldModel) inputOperationModel.InputModel).InputVisualizationType == InputVisualizationTypeConstants.ENUM)
                    {
                        return AxisType.Nominal;
                    }
                    return AxisType.Quantitative;
                }
                else if (((InputFieldModel) inputOperationModel.InputModel).InputDataType == InputDataTypeConstants.TIME)
                {
                    return AxisType.Time;
                }
                else if (((InputFieldModel) inputOperationModel.InputModel).InputDataType == InputDataTypeConstants.DATE)
                {
                    return AxisType.Date;
                }
            }
            return AxisType.Nominal;
        }

        private string _taskType;
        public string TaskType
        {
            get
            {
                return _taskType;
            }
            set
            {
                this.SetProperty(ref _taskType, value);
                FireQueryModelUpdated(QueryModelUpdatedEventType.Structure);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is QueryModel)
            {
                var am = obj as QueryModel;
                return
                    am.Id.Equals(this.Id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.Id.GetHashCode();
            return code;
        }
    }

    public class QueryModelUpdatedEventArgs : EventArgs
    {
        public QueryModelUpdatedEventType QueryModelUpdatedEventType { get; set; }

        public QueryModelUpdatedEventArgs(QueryModelUpdatedEventType type)
            : base()
        {
            QueryModelUpdatedEventType = type;
        }
    }

    public enum QueryModelUpdatedEventType { Structure, Links, FilterModels }
    
    public enum VisualizationType { table, bar, map, plot, line }
}
