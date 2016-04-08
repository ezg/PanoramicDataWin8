using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class InputVisualizationViewModel : ExtendedBindableBase
    {
        public static List<Color> ColorScheme1 = new List<Color>()
        {
            //Helpers.GetColorFromString("#956D99"), 
            Helpers.GetColorFromString("#B24D94"),
            Helpers.GetColorFromString("#EC7545"),
            Helpers.GetColorFromString("#38AE97"),
            Helpers.GetColorFromString("#4497C1"),
        };

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

        private Color _color = ColorScheme1[0];
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                this.SetProperty(ref _color, value);
            }
        }

        private int _colorIndex = 0;
        public int ColorIndex
        {
            get
            {
                return _colorIndex;
            }
            set
            {
                this.SetProperty(ref _colorIndex, value);
            }
        }


        public Rct Bounds
        {
            get { return new Rct(Position, Size); }
        }

        private VisualizationViewModel _from = null;
        public VisualizationViewModel From
        {
            get
            {
                return _from;
            }
            set
            {
                if (value == VisualizationViewModels[1])
                {
                    _to = VisualizationViewModels[0];
                }
                else
                {
                    _to = VisualizationViewModels[1];
                }
                this.SetProperty(ref _from, value);
            }
        }

        private VisualizationViewModel _to = null;
        public VisualizationViewModel To
        {
            get
            {
                return _to;
            }
            set { this.SetProperty(ref _to, value); }
        }

        private List<VisualizationViewModel> _visualizationViewModels = new List<VisualizationViewModel>();
        public List<VisualizationViewModel> VisualizationViewModels
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


        private InputVisualizationViewModelState _inputVisualizationViewModelState = InputVisualizationViewModelState.Opening;
        public InputVisualizationViewModelState InputVisualizationViewModelState
        {
            get
            {
                return _inputVisualizationViewModelState;
            }
            set
            {
                this.SetProperty(ref _inputVisualizationViewModelState, value);
            }
        }
    }

    public enum InputVisualizationViewModelState { Opening, Opened, Closing }
}
