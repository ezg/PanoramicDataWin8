using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;
using System.Linq;
using IDEA_common.aggregates;
using System.Collections.Generic;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.view;
using IDEA_common.catalog;

namespace PanoramicDataWin8.model.data.attribute
{
    [JsonObject(MemberSerialization.OptOut)]
    public class AttributeTransformationModel : BindableBase
    {
        private AggregateFunction _aggregateFunction = AggregateFunction.None;

        private AttributeModel _attributeModel;

        private double _binSize = 1.0;

        private double _maxBinSize = 100.0;

        private double _minBinSize = 1.0;

        private bool   _groupBy = false;

        private OperationModel _operationModel;

        private ScaleFunction _scaleFunction = ScaleFunction.None;

        private SortMode _sortMode = SortMode.None;

        private TransformationFunction _transformationFunction = TransformationFunction.None;

        public AttributeTransformationModel(AttributeModel attributeModel)
        {
            _attributeModel = attributeModel;
        }

        public AttributeModel AttributeModel
        {
            get { return _attributeModel; }
            set { SetProperty(ref _attributeModel, value); }
        }

        [JsonIgnore]
        public OperationModel OperationModel
        {
            get { return _operationModel; }
            set { SetProperty(ref _operationModel, value); }
        }

        public bool GroupBy
        {
            get { return _groupBy; }
            set { SetProperty(ref _groupBy, value); }
        }


        public AggregateFunction AggregateFunction
        {
            get { return _aggregateFunction; }
            set { SetProperty(ref _aggregateFunction, value); }
        }

        public double BinSize
        {
            get { return _binSize; }
            set { SetProperty(ref _binSize, value); }
        }

        public double MinBinSize
        {
            get { return _minBinSize; }
            set { SetProperty(ref _minBinSize, value); }
        }

        public double MaxBinSize
        {
            get { return _maxBinSize; }
            set { SetProperty(ref _maxBinSize, value); }
        }

        public TransformationFunction TransformationFunction
        {
            get { return _transformationFunction; }
            set { SetProperty(ref _transformationFunction, value); }
        }

        public SortMode SortMode
        {
            get { return _sortMode; }
            set { SetProperty(ref _sortMode, value); }
        }

        public ScaleFunction ScaleFunction
        {
            get { return _scaleFunction; }
            set { SetProperty(ref _scaleFunction, value); }
        }

        public List<AggregateFunction> AggregateFunctions {
            get { 
                var aggregateFunctions = new[] { AggregateFunction.None, AggregateFunction.Count }.ToList();
                if (AttributeModel.DataType == DataType.Float ||
                    AttributeModel.DataType == DataType.Double ||
                    AttributeModel.DataType == DataType.Int)
                {
                    aggregateFunctions.Add(AggregateFunction.Avg);
                    aggregateFunctions.Add(AggregateFunction.Sum);
                    if (MainViewController.Instance.MainModel.IsUnknownUnknownEnabled)
                    {
                        aggregateFunctions.Add(AggregateFunction.SumE);
                    }
                }
                return aggregateFunctions;
            }
        }


        public string GetLabel
        {
            get
            {
                var mainLabel = addDetailToLabel(AttributeModel.DisplayName);
                mainLabel = mainLabel.Replace("_", " ");
                return mainLabel;
            }
        }
        private string addDetailToLabel(string name)
        {
            if (AggregateFunction == AggregateFunction.Avg)
                name = "avg(" + name + ")";
            else if (AggregateFunction == AggregateFunction.Count)
                name = "count";
            else if (AggregateFunction == AggregateFunction.Max)
                name = "max(" + name + ")";
            else if (AggregateFunction == AggregateFunction.Min)
                name = "min(" + name + ")";
            else if (AggregateFunction == AggregateFunction.Sum)
                name = "sum(" + name + ")";
             else if (AggregateFunction == AggregateFunction.SumE)
                name = "sumE(" + name + ")";
            /*else if (AttributeTransformationViewModel.AggregateFunction == AggregateFunction.Bin)
            {
                name = "Bin Range(" + name + ")";
            }*/

            if (ScaleFunction != ScaleFunction.None)
                if (ScaleFunction == ScaleFunction.Log)
                    name += " [Log]";
                else if (ScaleFunction == ScaleFunction.Normalize)
                    name += " [Normalize]";
                else if (ScaleFunction == ScaleFunction.RunningTotal)
                    name += " [RT]";
                else if (ScaleFunction == ScaleFunction.RunningTotalNormalized)
                    name += " [RT Norm]";
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj is AttributeTransformationModel)
            {
                var aom = obj as AttributeTransformationModel;
                return
                    aom._aggregateFunction.Equals(AggregateFunction) &&
                    ((aom._attributeModel == null && _attributeModel == null) || (aom._attributeModel != null && aom._attributeModel.Equals(_attributeModel))) &&
                    aom._transformationFunction.Equals(_transformationFunction) &&
                    aom._binSize.Equals(_binSize) &&
                    aom._scaleFunction.Equals(_scaleFunction) &&
                    aom._sortMode.Equals(_sortMode);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= _aggregateFunction.GetHashCode();
            code ^= _attributeModel.GetHashCode();
            code ^= _transformationFunction.GetHashCode();
            code ^= _binSize.GetHashCode();
            code ^= _scaleFunction.GetHashCode();
            //code ^= this._sortMode.GetHashCode();
            return code;
        }
        public static AttributeTransformationModel MatchesExistingField(string str, bool exact=false)
        {
            var originModel = (MainViewController.Instance.MainPage.DataContext as MainModel).SchemaModel.OriginModels.First();
            var inputModels = originModel.InputModels.Where(am => am.IsDisplayed).ToList();
            inputModels.AddRange(IDEAAttributeModel.GetAllCalculatedAttributeModels(originModel));
            AttributeTransformationModel attributeTransformationModel = null;
            foreach (var im in inputModels)
                if (im.RawName.ToLower().StartsWith(str.ToLower()) && !exact)
                    attributeTransformationModel = new AttributeTransformationModel(im as AttributeModel);
            foreach (var im in inputModels)
                if (im.RawName.ToLower() == str.ToLower())
                    attributeTransformationModel = new AttributeTransformationModel(im as AttributeModel);
            return attributeTransformationModel;
        }
        public static IEnumerable<string> ExistingFieldList()
        {
            var originModel = (MainViewController.Instance.MainPage.DataContext as MainModel).SchemaModel.OriginModels.First();
            var inputModels = originModel.InputModels.Where(am => am.IsDisplayed).ToList();
            inputModels.AddRange(IDEAAttributeModel.GetAllCalculatedAttributeModels(originModel));
            var existingFields = new List<string>();
            foreach (var im in inputModels)
                existingFields.Add(im.RawName);
            return existingFields;
        }
    }
}