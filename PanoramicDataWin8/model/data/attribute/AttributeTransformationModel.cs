﻿using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;

namespace PanoramicDataWin8.model.data
{
    [JsonObject(MemberSerialization.OptOut)]
    public class AttributeTransformationModel : BindableBase
    {
        public AttributeTransformationModel(AttributeModel attributeModel)
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

        private OperationModel _operationModel = null;
        [JsonIgnore]
        public OperationModel OperationModel
        {
            get
            {
                return _operationModel;
            }
            set
            {
                this.SetProperty(ref _operationModel, value);
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

        private TransformationFunction _transformationFunction = TransformationFunction.None;
        public TransformationFunction TransformationFunction
        {
            get
            {
                return _transformationFunction;
            }
            set
            {
                this.SetProperty(ref _transformationFunction, value);
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

        public string GetLabel()
        {
            var mainLabel = addDetailToLabel(AttributeModel.RawName);
            mainLabel = mainLabel.Replace("_", " ");
            return mainLabel;
        }

        private string addDetailToLabel(string name)
        {
            if (AggregateFunction == AggregateFunction.Avg)
            {
                name = "avg(" + name + ")";
            }
            else if (AggregateFunction == AggregateFunction.Count)
            {
                name = "count";
            }
            else if (AggregateFunction == AggregateFunction.Max)
            {
                name = "max(" + name + ")";
            }
            else if (AggregateFunction == AggregateFunction.Min)
            {
                name = "min(" + name + ")";
            }
            else if (AggregateFunction == AggregateFunction.Sum)
            {
                name = "sum(" + name + ")";
            }
            /*else if (AttributeTransformationViewModel.AggregateFunction == AggregateFunction.Bin)
            {
                name = "Bin Range(" + name + ")";
            }*/

            if (ScaleFunction != ScaleFunction.None)
            {
                if (ScaleFunction == ScaleFunction.Log)
                {
                    name += " [Log]";
                }
                else if (ScaleFunction == ScaleFunction.Normalize)
                {
                    name += " [Normalize]";
                }
                else if (ScaleFunction == ScaleFunction.RunningTotal)
                {
                    name += " [RT]";
                }
                else if (ScaleFunction == ScaleFunction.RunningTotalNormalized)
                {
                    name += " [RT Norm]";
                }
            }
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeTransformationModel)
            {
                var aom = obj as AttributeTransformationModel;
                return
                    aom._aggregateFunction.Equals(this.AggregateFunction) &&
                    aom._attributeModel.Equals(this._attributeModel) &&
                    aom._transformationFunction.Equals(this._transformationFunction) &&
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
            code ^= this._transformationFunction.GetHashCode();
            code ^= this._binSize.GetHashCode();
            code ^= this._scaleFunction.GetHashCode();
            //code ^= this._sortMode.GetHashCode();
            return code;
        }
    }

    public enum AggregateFunction { None, Sum, Count, Min, Max, Avg };

    public enum SortMode { Asc, Desc, None }

    public enum TransformationFunction { None, Year, MonthOfTheYear, DayOfTheMonth, DayOfTheWeek, HourOfTheDay}

    public enum ScaleFunction { None, Log, Normalize, RunningTotal, RunningTotalNormalized };
}
