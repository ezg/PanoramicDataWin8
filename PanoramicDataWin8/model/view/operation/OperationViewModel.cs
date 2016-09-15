using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view.operation
{
    public class OperationViewModel : ExtendedBindableBase
    {
        public static double WIDTH = 300;
        public static double HEIGHT = 300;

        public static double MIN_WIDTH = 100;
        public static double MIN_HEIGHT = 100;

        private static int _nextColorId = 0;
        public static Color[] COLORS = new Color[] {
            Color.FromArgb(255, 26, 188, 156),
            Color.FromArgb(255, 52, 152, 219),
            Color.FromArgb(255, 52, 73, 94),
            Color.FromArgb(255, 142, 68, 173),
            Color.FromArgb(255, 241, 196, 15),
            Color.FromArgb(255, 231, 76, 60),
            Color.FromArgb(255, 149, 165, 166),
            Color.FromArgb(255, 211, 84, 0),
            Color.FromArgb(255, 189, 195, 199),
            Color.FromArgb(255, 46, 204, 113),
            Color.FromArgb(255, 155, 89, 182),
            Color.FromArgb(255, 22, 160, 133),
            Color.FromArgb(255, 41, 128, 185),
            Color.FromArgb(255, 44, 62, 80),
            Color.FromArgb(255, 230, 126, 34),
            Color.FromArgb(255, 39, 174, 96),
            Color.FromArgb(255, 243, 156, 18),
            Color.FromArgb(255, 192, 57, 43),
            Color.FromArgb(255, 127, 140, 141)
        };
        
        public OperationViewModel(OperationModel operationModel)
        {
            _operationModel = operationModel;
            selectColor();
        }

        private Stopwatch _activeStopwatch = new Stopwatch();
        public Stopwatch ActiveStopwatch
        {
            get
            {
                return _activeStopwatch;
            }
            set
            {
                this.SetProperty(ref _activeStopwatch, value);
            }
        }

        private OperationModel _operationModel = null;
        public OperationModel OperationModel
        {
            get
            {
                return _operationModel;
            }
            set
            {
                this.SetProperty(ref _operationModel, value);
            }
        }

        private void selectColor()
        {
            if (_nextColorId >= COLORS.Count() - 1)
            {
                _nextColorId = 0;
            }
            Color = COLORS[_nextColorId++];
        }

        private ObservableCollection<AttachmentViewModel> _attachementViewModels = new ObservableCollection<AttachmentViewModel>();
        public ObservableCollection<AttachmentViewModel> AttachementViewModels
        {
            get
            {
                return _attachementViewModels;
            }
            set
            {
                this.SetProperty(ref _attachementViewModels, value);
            }
        }

        private SolidColorBrush _brush = null;
        public SolidColorBrush Brush
        {
            get
            {
                return _brush;
            }
            set
            {
                this.SetProperty(ref _brush, value);
            }
        }

        private SolidColorBrush _faintBrush = null;
        public SolidColorBrush FaintBrush
        {
            get
            {
                return _faintBrush;
            }
            set
            {
                this.SetProperty(ref _faintBrush, value);
            }
        }

        private Color _color = Color.FromArgb(0xff, 0x00, 0x00, 0x00);
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                this.SetProperty(ref _color, value);
                Brush = new SolidColorBrush(_color);
                FaintBrush = new SolidColorBrush(Color.FromArgb(70, _color.R, _color.G, _color.B));
            }
        }

        private Vec _size = new Vec(180, 100);
        public Vec Size
        {
            get
            {
                return _size;
            }
            set
            {
                this.SetProperty(ref _size, value);
            }
        }

        private Pt _postion;
        public Pt Position
        {
            get
            {
                return _postion;
            }
            set
            {
                this.SetProperty(ref _postion, value);
            }
        }

        public Rct Bounds
        {
            get
            {
                return new Rct(Position, Size);
            }
        }
    }
}
