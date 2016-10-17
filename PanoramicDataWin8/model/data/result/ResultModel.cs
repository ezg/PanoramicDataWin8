using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.result
{
    public class ResultModel : ExtendedBindableBase
    {
        public delegate void ResultModelUpdatedHandler(object sender, EventArgs e);


        private double _progress;

        private ResultDescriptionModel _resultDescriptionModel;

        private ObservableCollection<ResultItemModel> _resultItemModels;

        private ResultType _resultType = ResultType.Clear;

        public ObservableCollection<ResultItemModel> ResultItemModels
        {
            get { return _resultItemModels; }
            set { SetProperty(ref _resultItemModels, value); }
        }

        public ResultDescriptionModel ResultDescriptionModel
        {
            get { return _resultDescriptionModel; }
            set { SetProperty(ref _resultDescriptionModel, value); }
        }

        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResultType ResultType
        {
            get { return _resultType; }
            set { SetProperty(ref _resultType, value); }
        }

        public event ResultModelUpdatedHandler ResultModelUpdated;

        public void FireResultModelUpdated(ResultType resultType)
        {
            _resultType = resultType;
            if (ResultModelUpdated != null)
                ResultModelUpdated(this, new EventArgs());
        }
    }
}