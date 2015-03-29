using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.controller.data;
using PanoramicData.utils;
using PanoramicDataWin8.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class QueryResultItemModel : ExtendedBindableBase
    {
        public QueryResultItemModel()
        {
        }

        private Dictionary<JobResult, QueryResultItemValueModel> _jobResultValues = new Dictionary<JobResult, QueryResultItemValueModel>();
        public Dictionary<JobResult, QueryResultItemValueModel> JobResultValues
        {
            get
            {
                return _jobResultValues;
            }
            set
            {
                this.SetProperty(ref _jobResultValues, value);
            }
        }

        private Dictionary<VisualizationResult, QueryResultItemValueModel> _visualizationResultValues = new Dictionary<VisualizationResult, QueryResultItemValueModel>();
        public Dictionary<VisualizationResult, QueryResultItemValueModel> VisualizationResultValues
        {
            get
            {
                return _visualizationResultValues;
            }
            set
            {
                this.SetProperty(ref _visualizationResultValues, value);
            }
        }

        public double? XValue
        {
            get
            {
                if (VisualizationResultValues[VisualizationResult.X].Value != null)
                {
                    return double.Parse(VisualizationResultValues[VisualizationResult.X].Value.ToString());
                }
                return null;
            }
        }
        public double? YValue
        {
            get
            {
                if (VisualizationResultValues[VisualizationResult.Y].Value != null)
                {
                    return double.Parse(VisualizationResultValues[VisualizationResult.Y].Value.ToString());
                }
                return null;
            }
        }

        private Dictionary<AttributeOperationModel, QueryResultItemValueModel> _attributeValues = new Dictionary<AttributeOperationModel, QueryResultItemValueModel>();
        public Dictionary<AttributeOperationModel, QueryResultItemValueModel> AttributeValues
        {
            get
            {
                return _attributeValues;
            }
            set
            {
                this.SetProperty(ref _attributeValues, value);
            }
        }

        private Bin _bin = null;
        public Bin Bin
        {
            get
            {
                return _bin;
            }
            set
            {
                this.SetProperty(ref _bin, value);
            }
        }

        private Dictionary<AttributeOperationModel, double> _partitions = null;
        public Dictionary<AttributeOperationModel, double> Partitions
        {
            get
            {
                return _partitions;
            }
            set
            {
                this.SetProperty(ref _partitions, value);
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                this.SetProperty(ref _isSelected, value);
            }
        }

        private int _rowNumber = -1;
        public int RowNumber
        {
            get
            {
                return _rowNumber;
            }
            set
            {
                this.SetProperty(ref _rowNumber, value);
            }
        }

        private GroupingObject _groupingObject = null;
        public GroupingObject GroupingObject
        {
            get
            {
                return _groupingObject;
            }
            set
            {
                this.SetProperty(ref _groupingObject, value);
            }
        }

        public void Update(QueryResultItemModel updateTo)
        {
            this.JobResultValues = updateTo.JobResultValues;
            this.IsSelected = updateTo.IsSelected;
            this.GroupingObject = updateTo.GroupingObject;
            this.AttributeValues = updateTo.AttributeValues;
            this.RowNumber = updateTo.RowNumber;
            this.VisualizationResultValues = updateTo.VisualizationResultValues;
            this.Bin = updateTo.Bin;
        }
    }

    public class GroupingObject
    {
        private Dictionary<int, object> _dictionary = new Dictionary<int, object>();
        private bool _isAnyGrouped = false;
        private bool _isAnyAggregated = false;
        private object _idValue = null;

        public GroupingObject(bool isAnyGrouped, bool isAnyAggregated, object idValue)
        {
            _isAnyGrouped = isAnyGrouped;
            _isAnyAggregated = isAnyAggregated;
            _idValue = idValue;
        }

        public void Add(int index, object value)
        {
            _dictionary.Add(index, value);
        }

        public override bool Equals(object obj)
        {
            if (obj is GroupingObject)
            {
                var go = obj as GroupingObject;
                if (_isAnyGrouped)
                {
                    return go._dictionary.SequenceEqual(this._dictionary);
                }
                else
                {
                    if (_isAnyAggregated)
                    {
                        return true;
                    }
                    return go._idValue.Equals(_idValue);
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            if (_isAnyGrouped)
            {
                int code = 0;
                foreach (var v in _dictionary.Values)
                    if (v == null)
                    {
                        code ^= "null".GetHashCode();
                    }
                    else
                    {
                        code ^= v.GetHashCode();
                    }
                return code;
            }
            else
            {
                if (_isAnyAggregated)
                {
                    return 0;
                }
                return _idValue.GetHashCode();
            }
        }
    }
}
