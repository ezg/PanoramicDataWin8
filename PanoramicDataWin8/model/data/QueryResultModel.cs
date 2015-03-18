﻿using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.controller.data;
using PanoramicData.utils;
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
    }
}
