using System;
using System.Collections.Generic;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view.operation
{
    public class StatisticalComparisonOperationViewModel : ExtendedBindableBase
    {
        private ComparisonViewModelState _comparisonViewModelState = ComparisonViewModelState.Opening;

        private List<OperationViewModel> _operationViewModels = new List<OperationViewModel>();

        private Pt _position = new Pt(0, 0);

        private Vec _size = new Vec(75, 75);

        private StatisticalComparisonOperationModel _statisticalComparisonOperationModel;

        public StatisticalComparisonOperationViewModel(
            StatisticalComparisonOperationModel statisticalComparisonOperationModel)
        {
            StatisticalComparisonOperationModel = statisticalComparisonOperationModel;
        }

        public Pt DwellStartPosition { get; set; }
        public long TicksSinceDwellStart { get; set; }
        public bool IsActive { get; set; }

        public Pt Position
        {
            get { return _position; }
            set
            {
                var p = new Pt(Math.Round(value.X, MidpointRounding.ToEven),
                    Math.Round(value.Y, MidpointRounding.ToEven));
                SetProperty(ref _position, p);
            }
        }

        public Vec Size
        {
            get { return _size; }
            set
            {
                var s = new Vec(Math.Round(value.X, MidpointRounding.ToEven),
                    Math.Round(value.Y, MidpointRounding.ToEven));
                SetProperty(ref _size, s);
            }
        }

        public Rct Bounds
        {
            get { return new Rct(Position, Size); }
        }

        public StatisticalComparisonOperationModel StatisticalComparisonOperationModel
        {
            get { return _statisticalComparisonOperationModel; }
            set { SetProperty(ref _statisticalComparisonOperationModel, value); }
        }

        public List<OperationViewModel> OperationViewModels
        {
            get { return _operationViewModels; }
            set { SetProperty(ref _operationViewModels, value); }
        }

        public ComparisonViewModelState ComparisonViewModelState
        {
            get { return _comparisonViewModelState; }
            set { SetProperty(ref _comparisonViewModelState, value); }
        }
    }
}