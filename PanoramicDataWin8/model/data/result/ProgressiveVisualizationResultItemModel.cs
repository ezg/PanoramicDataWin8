using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Operation.Valid;

namespace PanoramicDataWin8.model.data.result
{

    public class ProgressiveVisualizationResultItemModel : ResultItemModel
    {
        public ProgressiveVisualizationResultItemModel()
        {
        }

        private Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> _values = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
        public Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> Values
        {
            get
            {
                return _values;
            }
            set
            {
                this.SetProperty(ref _values, value);
            }
        }

        private Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> _margins = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
        public Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> Margins
        {
            get
            {
                return _margins;
            }
            set
            {
                this.SetProperty(ref _margins, value);
            }
        }

        private Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> _counts = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
        public Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> Counts
        {
            get
            {
                return _counts;
            }
            set
            {
                this.SetProperty(ref _counts, value);
            }
        }


        private Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> _countsInterpolated = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
        public Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> CountsInterpolated
        {
            get
            {
                return _countsInterpolated;
            }
            set
            {
                this.SetProperty(ref _countsInterpolated, value);
            }
        }

        private Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> _marginsAbsolute = new Dictionary<InputOperationModel, Dictionary<BrushIndex, double>>();
        public Dictionary<InputOperationModel, Dictionary<BrushIndex, double>> MarginsAbsolute
        {
            get
            {
                return _marginsAbsolute;
            }
            set
            {
                this.SetProperty(ref _marginsAbsolute, value);
            }
        }


        public void AddValue(InputOperationModel aom, BrushIndex bi, double value)
        {
            if (!_values.ContainsKey(aom))
            {
                _values.Add(aom, new Dictionary<BrushIndex, double>());
            }
            else
            {
                
            }
            _values[aom][bi] = value;
        }

        public void AddMargin(InputOperationModel aom, BrushIndex bi, double margin)
        {
            if (!_margins.ContainsKey(aom))
            {
                _margins.Add(aom, new Dictionary<BrushIndex, double>());
            }
            _margins[aom][bi] = margin;
        }

        public void AddMarginAbsolute(InputOperationModel aom, BrushIndex bi, double marginAbsolute)
        {
            if (!_marginsAbsolute.ContainsKey(aom))
            {
                _marginsAbsolute.Add(aom, new Dictionary<BrushIndex, double>());
            }
            _marginsAbsolute[aom][bi] = marginAbsolute;
        }

        public void AddCount(InputOperationModel aom, BrushIndex bi, double marginAbsolute)
        {
            if (!_counts.ContainsKey(aom))
            {
                _counts.Add(aom, new Dictionary<BrushIndex, double>());
            }
            _counts[aom][bi] = marginAbsolute;
        }

        public void AddCountInterpolated(InputOperationModel aom, BrushIndex bi, double marginAbsolute)
        {
            if (!_countsInterpolated.ContainsKey(aom))
            {
                _countsInterpolated.Add(aom, new Dictionary<BrushIndex, double>());
            }
            _countsInterpolated[aom][bi] = marginAbsolute;
        }
    }

    public class BrushIndex
    {
        private string _id = "";

        public static BrushIndex OVERLAP = new BrushIndex("overlap");
        public static BrushIndex ALL = new BrushIndex("all");

        public BrushIndex(QueryModel qm)
        {
            _id = qm.Id.ToString();
        }

        public BrushIndex(string id)
        {
            _id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj is BrushIndex)
            {
                var am = obj as BrushIndex;
                return
                    am._id.Equals(this._id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this._id.GetHashCode();
            return code;
        }
    }
}
