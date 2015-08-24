using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.data.common;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.result
{
    public class ClassfierResultDescriptionModel : ResultDescriptionModel
    {
        private List<InputFieldModel> _labels = new List<InputFieldModel>();
        public List<InputFieldModel> Labels
        {
            get { return _labels; }
            set { this.SetProperty(ref _labels, value); }
        }

        private Dictionary<InputFieldModel, List<List<double>>> _confusionMatrices = new Dictionary<InputFieldModel, List<List<double>>>();
        public Dictionary<InputFieldModel, List<List<double>>> ConfusionMatrices
        {
            get { return _confusionMatrices; }
            set { this.SetProperty(ref _confusionMatrices, value); }
        }

        private Dictionary<InputFieldModel, List<Pt>> _rocCurves = new Dictionary<InputFieldModel, List<Pt>>();
        public Dictionary<InputFieldModel, List<Pt>> RocCurves
        {
            get { return _rocCurves; }
            set { this.SetProperty(ref _rocCurves, value); }
        }

        private Dictionary<InputFieldModel, double> _f1s = new Dictionary<InputFieldModel, double>();
        public Dictionary<InputFieldModel, double> F1s
        {
            get { return _f1s; }
            set { this.SetProperty(ref _f1s, value); }
        }

        private Dictionary<InputFieldModel, double> _precisions = new Dictionary<InputFieldModel, double>();
        public Dictionary<InputFieldModel, double> Precisions
        {
            get { return _precisions; }
            set { this.SetProperty(ref _precisions, value); }
        }

        private Dictionary<InputFieldModel, double> _recalls = new Dictionary<InputFieldModel, double>();
        public Dictionary<InputFieldModel, double> Recalls
        {
            get { return _recalls; }
            set { this.SetProperty(ref _recalls, value); }
        }

        private Dictionary<InputFieldModel, double> _aucs = new Dictionary<InputFieldModel, double>();
        public Dictionary<InputFieldModel, double> AUCs
        {
            get { return _aucs; }
            set { this.SetProperty(ref _aucs, value); }
        }
    }
}
