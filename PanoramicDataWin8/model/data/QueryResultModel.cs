﻿using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.controller.data;
using PanoramicData.utils;
using PanoramicDataWin8.controller.data.sim;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class QueryResultModel : ExtendedBindableBase
    {
        public delegate void QueryResultModelUpdatedHandler(object sender, EventArgs e);
        public event QueryResultModelUpdatedHandler QueryResultModelUpdated;

        private ObservableCollection<QueryResultItemModel> _queryResultItemModels = null;
        public ObservableCollection<QueryResultItemModel> QueryResultItemModels
        {
            get
            {
                return _queryResultItemModels;
            }
            set
            {
                this.SetProperty(ref _queryResultItemModels, value);
            }
        }
        public void FireQueryResultModelUpdated()
        {
            if (QueryResultModelUpdated != null)
            {
                QueryResultModelUpdated(this, new EventArgs());
            }
        }


        private double _progress = 0;
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                this.SetProperty(ref _progress, value);
            }
        }

        private double _xNullCount = 0;
        public double XNullCount
        {
            get
            {
                return _xNullCount;
            }
            set
            {
                this.SetProperty(ref _xNullCount, value);
            }
        }

        private double _yNullCount = 0;
        public double YNullCount
        {
            get
            {
                return _yNullCount;
            }
            set
            {
                this.SetProperty(ref _yNullCount, value);
            }
        }

        private double _xAndYNullCount = 0;
        public double XAndYNullCount
        {
            get
            {
                return _xAndYNullCount;
            }
            set
            {
                this.SetProperty(ref _xAndYNullCount, value);
            }
        }

        private AxisType _xAxisType = AxisType.Nominal;
        public AxisType XAxisType
        {
            get
            {
                return _xAxisType;
            }
            set
            {
                this.SetProperty(ref _xAxisType, value);
            }
        }

        private AxisType _yAxisType = AxisType.Nominal;
        public AxisType YAxisType
        {
            get
            {
                return _yAxisType;
            }
            set
            {
                this.SetProperty(ref _yAxisType, value);
            }
        }

        private Scale _xScale = null;
        public Scale XScale
        {
            get
            {
                return _xScale;
            }
            set
            {
                this.SetProperty(ref _xScale, value);
            }
        }

        private Scale _yScale = null;
        public Scale YScale
        {
            get
            {
                return _yScale;
            }
            set
            {
                this.SetProperty(ref _yScale, value);
            }
        }
    }

    public enum AxisType { Ordinal, Quantitative, Nominal, Time, Date }
}
