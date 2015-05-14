using PanoramicData.model.data.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data.result
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

        private List<AttributeOperationModel> _dimensions = new List<AttributeOperationModel>();
        public List<AttributeOperationModel> Dimensions
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


        private Dictionary<AttributeOperationModel, double> _maxValues = new Dictionary<AttributeOperationModel, double>();
        public Dictionary<AttributeOperationModel, double> MaxValues
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

        private Dictionary<AttributeOperationModel, double> _minValues = new Dictionary<AttributeOperationModel, double>();
        public Dictionary<AttributeOperationModel, double> MinValues
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
