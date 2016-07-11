using System.Collections.Generic;
using PanoramicDataWin8.model.data.common;

namespace PanoramicDataWin8.model.data.result
{
    public class VisualizationResultDescriptionModel : ResultDescriptionModel
    {
        private double _nullCount = 0;
        public double NullCount
        {
            get
            {
                return _nullCount;
            }
            set
            {
                this.SetProperty(ref _nullCount, value);
            }
        }

        public Dictionary<string, double> OverallMeans { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> OverallCount { get; set; } = new Dictionary<string, double>();

        public Dictionary<string, double> OverallPowerSumAverage { get; set; } = new Dictionary<string, double>();
        
        public Dictionary<string, double> OverallSampleStandardDeviations { get; set; } = new Dictionary<string, double>();

        private List<AxisType> _axisTypes = new List<AxisType>();
        public List<AxisType> AxisTypes
        {
            get
            {
                return _axisTypes;
            }
            set
            {
                this.SetProperty(ref _axisTypes, value);
            }
        }

        private List<BinRange> _binRanges = null;
        public List<BinRange> BinRanges
        {
            get
            {
                return _binRanges;
            }
            set
            {
                this.SetProperty(ref _binRanges, value);
            }
        }


        private List<BrushIndex> _brushIndices = null;
        public List<BrushIndex> BrushIndices
        {
            get
            {
                return _brushIndices;
            }
            set
            {
                this.SetProperty(ref _brushIndices, value);
            }
        }

        private List<InputOperationModel> _dimensions = new List<InputOperationModel>();
        public List<InputOperationModel> Dimensions
        {
            get
            {
                return _dimensions;
            }
            set
            {
                this.SetProperty(ref _dimensions, value);
            }
        }


        private Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> _maxValues = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
        public Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> MaxValues
        {
            get
            {
                return _maxValues;
            }
            set
            {
                this.SetProperty(ref _maxValues, value);
            }
        }

        private Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> _minValues = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
        public Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> MinValues
        {
            get
            {
                return _minValues;
            }
            set
            {
                this.SetProperty(ref _minValues, value);
            }
        }
    }
}
