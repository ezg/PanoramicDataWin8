using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.result
{
    public class ClassfierResultDescriptionModel : ResultDescriptionModel
    {
        private List<List<double>> _confusionMatrices = new List<List<double>>();
        public List<List<double>> ConfusionMatrices
        {
            get { return _confusionMatrices; }
            set { this.SetProperty(ref _confusionMatrices, value); }
        }
        
        private List<ResultModel> _visualizationResultModel = new List<ResultModel>();
        public List<ResultModel> VisualizationResultModel
        {
            get { return _visualizationResultModel; }
            set { this.SetProperty(ref _visualizationResultModel, value); }
        }

        private List<Pt> _rocCurve = new List<Pt>();
        public List<Pt> RocCurve
        {
            get { return _rocCurve; }
            set { this.SetProperty(ref _rocCurve, value); }
        }

        private List<double> _f1s = new List<double>();
        public List<double> F1s
        {
            get { return _f1s; }
            set { this.SetProperty(ref _f1s, value); }
        }

        private List<double> _progresses = new List<double>();
        public List<double> Progresses
        {
            get { return _progresses; }
            set { this.SetProperty(ref _progresses, value); }
        }

        private double _precision = 0;
        public double Precision
        {
            get { return _precision; }
            set { this.SetProperty(ref _precision, value); }
        }

        private double _recall = 0;
        public double Recall
        {
            get { return _recall; }
            set { this.SetProperty(ref _recall, value); }
        }

        private double _auc = 0;
        public double AUC
        {
            get { return _auc; }
            set { this.SetProperty(ref _auc, value); }
        }

        private string _uuid = null;
        public string Uuid
        {
            get { return _uuid; }
            set { this.SetProperty(ref _uuid, value); }
        }
    }
}
