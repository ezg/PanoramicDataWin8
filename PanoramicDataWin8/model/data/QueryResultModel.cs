﻿using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.controller.data;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class QueryResultModel : ExtendedBindableBase
    {
        private AsyncVirtualizedCollection<QueryResultItemModel> _queryResultItemModels = null;
        public AsyncVirtualizedCollection<QueryResultItemModel> QueryResultItemModels
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
    }
}
