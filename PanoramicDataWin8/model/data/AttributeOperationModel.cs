using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class AttributeOperationModel : BindableBase
    {
        public AttributeOperationModel(AttributeModel attributeModel)
        {
            _attributeModel = attributeModel;
        }

        private AttributeModel _attributeModel = null;
        
        public AttributeModel AttributeModel
        {
            get
            {
                return _attributeModel;
            }
            set
            {
                this.SetProperty(ref _attributeModel, value);
            }
        }

        private QueryModel _queryModel = null;
        [JsonIgnore]
        public QueryModel QueryModel
        {
            get
            {
                return _queryModel;
            }
            set
            {
                this.SetProperty(ref _queryModel, value);
            }
        }

        private AggregateFunction _aggregateFunction = AggregateFunction.None;
        public AggregateFunction AggregateFunction
        {
            get
            {
                return _aggregateFunction;
            }
            set
            {
                this.SetProperty(ref _aggregateFunction, value);
            }
        }

        private double _binSize = 1.0;
        public double BinSize
        {
            get
            {
                return _binSize;
            }
            set
            {
                this.SetProperty(ref _binSize, value);
            }
        }

        private double _minBinSize = 1.0;
        public double MinBinSize
        {
            get
            {
                return _minBinSize;
            }
            set
            {
                this.SetProperty(ref _minBinSize, value);
            }
        }

        private double _maxBinSize = 100.0;
        public double MaxBinSize
        {
            get
            {
                return _maxBinSize;
            }
            set
            {
                this.SetProperty(ref _maxBinSize, value);
            }
        }

        private GroupMode _groupMode = GroupMode.None;
        public GroupMode GroupMode
        {
            get
            {
                return _groupMode;
            }
            set
            {
                this.SetProperty(ref _groupMode, value);
            }
        }

        private SortMode _sortMode = SortMode.None;
        public SortMode SortMode
        {
            get
            {
                return _sortMode;
            }
            set
            {
                this.SetProperty(ref _sortMode, value);
            }
        }

        private ScaleFunction _scaleFunction = ScaleFunction.None;
        public ScaleFunction ScaleFunction
        {
            get
            {
                return _scaleFunction;
            }
            set
            {
                this.SetProperty(ref _scaleFunction, value);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeOperationModel)
            {
                var aom = obj as AttributeOperationModel;
                return
                    aom._aggregateFunction.Equals(this.AggregateFunction) &&
                    aom._attributeModel.Equals(this._attributeModel) &&
                    aom._groupMode.Equals(this._groupMode) &&
                    aom._binSize.Equals(this._binSize) &&
                    aom._scaleFunction.Equals(this._scaleFunction) &&
                    aom._sortMode.Equals(this._sortMode);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this._aggregateFunction.GetHashCode();
            code ^= this._attributeModel.GetHashCode();
            code ^= this._groupMode.GetHashCode();
            code ^= this._binSize.GetHashCode();
            code ^= this._scaleFunction.GetHashCode();
            //code ^= this._sortMode.GetHashCode();
            return code;
        }
    }

    public enum AggregateFunction { None, Sum, Count, Min, Max, Avg };

    public enum SortMode { Asc, Desc, None }

    public enum GroupMode { None, Distinct, Binned, Year, MonthOfTheYear, DayOfTheMonth, DayOfTheWeek, HourOfTheDay}

    public enum ScaleFunction { None, Log, Normalize, RunningTotal, RunningTotalNormalized };
}
