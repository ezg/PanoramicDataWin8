using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class StatisticalComparisonViewModel : ExtendedBindableBase
    {
        public Pt DwellStartPosition { get; set; }
        public long TicksSinceDwellStart { get; set; }
        public bool IsActive { get; set; }

        private Pt _position = new Pt(0, 0);
        public Pt Position
        {
            get
            {
                return _position;
            }
            set
            {
                var p = new Pt(Math.Round(value.X, MidpointRounding.ToEven), Math.Round(value.Y, MidpointRounding.ToEven));
                this.SetProperty(ref _position, p);
            }
        }

        private Vec _size = new Vec(60, 60);
        public Vec Size
        {
            get
            {
                return _size;
            }
            set
            {
                var s = new Vec(Math.Round(value.X, MidpointRounding.ToEven), Math.Round(value.Y, MidpointRounding.ToEven));
                this.SetProperty(ref _size, s);
            }
        }

        public Rct Bounds
        {
            get { return new Rct(Position, Size); }
        }

        private List<HistogramOperationViewModel> _visualizationViewModels = new List<HistogramOperationViewModel>();
        public List<HistogramOperationViewModel> VisualizationViewModels
        {
            get
            {
                return _visualizationViewModels;
            }
            set
            {
                this.SetProperty(ref _visualizationViewModels, value);
            }
        }


        private ComparisonViewModelState _comparisonViewModelState = ComparisonViewModelState.Opening;
        public ComparisonViewModelState ComparisonViewModelState
        {
            get
            {
                return _comparisonViewModelState;
            }
            set
            {
                this.SetProperty(ref _comparisonViewModelState, value);
            }
        }
    }

    public enum ComparisonViewModelState { Opening, Opened, Closing }
}
