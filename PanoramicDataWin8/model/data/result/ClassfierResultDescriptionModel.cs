using System.Collections.Generic;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.result
{
    public class ClassfierResultDescriptionModel : ResultDescriptionModel
    {
        private double _auc;
        private List<List<double>> _confusionMatrices = new List<List<double>>();

        private List<double> _f1s = new List<double>();

        private double _precision;

        private List<double> _progresses = new List<double>();

        private double _recall;

        private List<Pt> _rocCurve = new List<Pt>();

        private string _uuid;

        private List<ResultModel> _visualizationResultModel = new List<ResultModel>();

        public List<List<double>> ConfusionMatrices
        {
            get { return _confusionMatrices; }
            set { SetProperty(ref _confusionMatrices, value); }
        }

        public List<ResultModel> VisualizationResultModel
        {
            get { return _visualizationResultModel; }
            set { SetProperty(ref _visualizationResultModel, value); }
        }

        public List<Pt> RocCurve
        {
            get { return _rocCurve; }
            set { SetProperty(ref _rocCurve, value); }
        }

        public List<double> F1s
        {
            get { return _f1s; }
            set { SetProperty(ref _f1s, value); }
        }

        public List<double> Progresses
        {
            get { return _progresses; }
            set { SetProperty(ref _progresses, value); }
        }

        public double Precision
        {
            get { return _precision; }
            set { SetProperty(ref _precision, value); }
        }

        public double Recall
        {
            get { return _recall; }
            set { SetProperty(ref _recall, value); }
        }

        public double AUC
        {
            get { return _auc; }
            set { SetProperty(ref _auc, value); }
        }

        public string Uuid
        {
            get { return _uuid; }
            set { SetProperty(ref _uuid, value); }
        }
    }
}