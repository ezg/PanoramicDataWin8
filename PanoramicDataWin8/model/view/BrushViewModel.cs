using System;
using System.Collections.Generic;
using Windows.UI;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class BrushViewModel : ExtendedBindableBase
    {
        public static List<Color> ColorScheme1 = new List<Color>
        {
            //Helpers.GetColorFromString("#956D99"), 
            Helpers.GetColorFromString("#B24D94"),
            Helpers.GetColorFromString("#EC7545"),
            Helpers.GetColorFromString("#38AE97"),
            Helpers.GetColorFromString("#4497C1")
        };


        private BrushableOperationViewModelState _brushableOperationViewModelState = BrushableOperationViewModelState.Opening;

        private Color _color = ColorScheme1[0];

        private int _colorIndex;

        private OperationViewModel _from;

        private List<OperationViewModel> _operationViewModels = new List<OperationViewModel>();

        private Pt _position = new Pt(0, 0);

        private Vec _size = new Vec(60, 60);

        private OperationViewModel _to;

        public Pt DwellStartPosition { get; set; }
        public long TicksSinceDwellStart { get; set; }
        public bool IsActive { get; set; }

        public Pt Position
        {
            get { return _position; }
            set
            {
                var p = new Pt(Math.Round(value.X, MidpointRounding.ToEven), Math.Round(value.Y, MidpointRounding.ToEven));
                SetProperty(ref _position, p);
            }
        }

        public Vec Size
        {
            get { return _size; }
            set
            {
                var s = new Vec(Math.Round(value.X, MidpointRounding.ToEven), Math.Round(value.Y, MidpointRounding.ToEven));
                SetProperty(ref _size, s);
            }
        }

        public Color Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        public int ColorIndex
        {
            get { return _colorIndex; }
            set { SetProperty(ref _colorIndex, value); }
        }


        public Rct Bounds
        {
            get { return new Rct(Position, Size); }
        }

        public OperationViewModel From
        {
            get { return _from; }
            set { SetProperty(ref _from, value); }
        }

        public OperationViewModel To
        {
            get { return _to; }
            set { SetProperty(ref _to, value); }
        }

        public List<OperationViewModel> OperationViewModels
        {
            get { return new List<OperationViewModel>(new[] {_to, _from}); }
        }

        public BrushableOperationViewModelState BrushableOperationViewModelState
        {
            get { return _brushableOperationViewModelState; }
            set { SetProperty(ref _brushableOperationViewModelState, value); }
        }
    }
}