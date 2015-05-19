﻿using System.Collections.Generic;
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


        private Dictionary<InputOperationModel, double> _maxValues = new Dictionary<InputOperationModel, double>();
        public Dictionary<InputOperationModel, double> MaxValues
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

        private Dictionary<InputOperationModel, double> _minValues = new Dictionary<InputOperationModel, double>();
        public Dictionary<InputOperationModel, double> MinValues
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
