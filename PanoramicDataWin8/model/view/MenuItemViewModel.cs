using PanoramicData.model.data;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicDataWin8.model.view
{
    public class MenuItemViewModel : ExtendedBindableBase
    {
        private static Random random = new Random();

        public MenuItemViewModel()
        {
            _dampingFactor = random.NextDouble() * 3.0 + 3;
        }

        private MenuViewModel _menuViewModel = null;
        public MenuViewModel MenuViewModel
        {
            get
            {
                return _menuViewModel;
            }
            set
            {
                this.SetProperty(ref _menuViewModel, value);
            }
        }

        private double _dampingFactor = 0.0;
        public double DampingFactor
        {
            get
            {
                return _dampingFactor;
            }
            set
            {
                this.SetProperty(ref _dampingFactor, value);
            }
        }

        private bool _isDropTarget = false;
        public bool IsDropTarget
        {
            get
            {
                return _isDropTarget;
            }
            set
            {
                this.SetProperty(ref _isDropTarget, value);
            }
        }

        private int _row = 0;
        public int Row
        {
            get
            {
                return _row;
            }
            set
            {
                this.SetProperty(ref _row, value);
            }
        }

        private Vec _preferedSize = new Vec(50, 50);
        public Vec PreferedSize
        {
            get
            {
                return _preferedSize;
            }
            set
            {
                this.SetProperty(ref _preferedSize, value);
            }
        }

        private Vec _targetSize = new Vec(50, 50);
        public Vec TargetSize
        {
            get
            {
                return _targetSize;
            }
            set
            {
                this.SetProperty(ref _targetSize, value);
            }
        }

        private Pt _targetPosition = new Pt(0, 0);
        public Pt TargetPosition
        {
            get
            {
                return _targetPosition;
            }
            set
            {
                this.SetProperty(ref _targetPosition, value);
            }
        }

        private Vec _size = new Vec(50, 50);
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

        private Pt _position = new Pt(0, 0);
        public Pt Position
        {
            get
            {
                return _position;
            }
            set
            {
                this.SetProperty(ref _position, value);
            }
        }

        private MenuItemComponentViewModel _menuItemComponentViewModel = null;
        public MenuItemComponentViewModel MenuItemComponentViewModel
        {
            get
            {
                return _menuItemComponentViewModel;
            }
            set
            {
                this.SetProperty(ref _menuItemComponentViewModel, value);
            }
        }
    }

    public class MenuItemComponentViewModel : ExtendedBindableBase
    {
        private bool _isEnabled = false;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                this.SetProperty(ref _isEnabled, value);
            }
        }
    }

    public class ToggleMenuItemComponentViewModel : MenuItemComponentViewModel
    {
        private bool _isChecked = false;
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                this.SetProperty(ref _isChecked, value);
            }
        }

        private string _label = "";
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                this.SetProperty(ref _label, value);
            }
        }

        private List<ToggleMenuItemComponentViewModel> _otherToggles = new List<ToggleMenuItemComponentViewModel>();
        public List<ToggleMenuItemComponentViewModel> OtherToggles
        {
            get
            {
                return _otherToggles;
            }
            set
            {
                this.SetProperty(ref _otherToggles, value);
            }
        }
    }

    public class SliderMenuItemComponentViewModel : MenuItemComponentViewModel
    {
        private double _value = 0;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                this.SetProperty(ref _value, value);
            }
        }

        private double _maxValue = 0;
        public double MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                this.SetProperty(ref _maxValue, value);
            }
        }

        private double _minValue = 0;
        public double MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                this.SetProperty(ref _minValue, value);
            }
        }

        private string _label = "";
        public string Label
        {
            get
            {
                return _label;
            }
            set
            {
                this.SetProperty(ref _label, value);
            }
        }

    }
}
