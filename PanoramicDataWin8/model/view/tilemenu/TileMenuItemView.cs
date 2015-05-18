using System;
using System.Collections.ObjectModel;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view.tilemenu
{
    public class TileMenuItemViewModel : ExtendedBindableBase
    {
        private static Random _random = new Random();

        public TileMenuItemViewModel(TileMenuItemViewModel parent)
        {
            this.Parent = parent;
            _dampingFactor = _random.NextDouble() * 3.0 + 3;
        }

        private TileMenuItemViewModel _parent = null;
        public TileMenuItemViewModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                this.SetProperty(ref _parent, value);
            }
        }

        private TileMenuContentViewModel _tileMenuContentViewModel = null;
        public TileMenuContentViewModel TileMenuContentViewModel
        {
            get
            {
                return _tileMenuContentViewModel;
            }
            set
            {
                this.SetProperty(ref _tileMenuContentViewModel, value);
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                this.SetProperty(ref _isEnabled, value);
                foreach (var menuItemViewModel in Children)
                {
                    menuItemViewModel.IsEnabled = value;
                }
            }
        }

        private bool _isDisplayed = false;
        public bool IsDisplayed
        {
            get
            {
                return _isDisplayed;
            }
            set
            {
                this.SetProperty(ref _isDisplayed, value);
            }
        }

        private bool _isBeingRemoved = false;
        public bool IsBeingRemoved
        {
            get
            {
                return _isBeingRemoved;
            }
            set
            {
                this.SetProperty(ref _isBeingRemoved, value);
                if (value)
                {
                    foreach (var menuItemViewModel in Children)
                    {
                        menuItemViewModel.IsBeingRemoved = value;
                    }
                }
            }
        }

        private bool _areChildrenExpanded = false;
        public bool AreChildrenExpanded
        {
            get
            {
                return _areChildrenExpanded;
            }
            set
            {
                this.SetProperty(ref _areChildrenExpanded, value);
                if (!value)
                {
                    foreach (var menuItemViewModel in Children)
                    {
                        menuItemViewModel.AreChildrenExpanded = value;
                    }
                }
                else
                {
                    if (Parent != null)
                    {
                        foreach (var menuItemViewModel in Parent.Children)
                        {
                            if (menuItemViewModel != this)
                            {
                                menuItemViewModel.AreChildrenExpanded = false;
                            }
                        }
                    }
                }
            }
        }

        private ObservableCollection<TileMenuItemViewModel> _children = new ObservableCollection<TileMenuItemViewModel>();
        public ObservableCollection<TileMenuItemViewModel> Children
        {
            get
            {
                return _children;
            }
            set
            {
                this.SetProperty(ref _children, value);
            }
        }

        private int _childrenNrColumns = 0;
        public int ChildrenNrColumns
        {
            get
            {
                return _childrenNrColumns;
            }
            set
            {
                this.SetProperty(ref _childrenNrColumns, value);
            }
        }

        private int _childrenNrRows = 0;
        public int ChildrenNrRows
        {
            get
            {
                return _childrenNrRows;
            }
            set
            {
                this.SetProperty(ref _childrenNrRows, value);
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

        private int _column = 0;
        public int Column
        {
            get
            {
                return _column;
            }
            set
            {
                this.SetProperty(ref _column, value);
            }
        }

        private int _rowSpan = 1;
        public int RowSpan
        {
            get
            {
                return _rowSpan;
            }
            set
            {
                this.SetProperty(ref _rowSpan, value);
            }
        }

        private int _columnSpan = 1;
        public int ColumnSpan
        {
            get
            {
                return _columnSpan;
            }
            set
            {
                this.SetProperty(ref _columnSpan, value);
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

        private double _childrenGap = 4;
        public double Gap
        {
            get
            {
                return _childrenGap;
            }
            set
            {
                this.SetProperty(ref _childrenGap, value);
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

        private Pt _currentPosition = new Pt(0, 0);
        public Pt CurrentPosition
        {
            get
            {
                return _currentPosition;
            }
            set
            {
                this.SetProperty(ref _currentPosition, value);
            }
        }

        private AttachPosition _attachPosition = AttachPosition.Bottom;
        public AttachPosition AttachPosition
        {
            get
            {
                return _attachPosition;
            }
            set
            {
                this.SetProperty(ref _attachPosition, value);
            }
        }

        private Alignment _alignment = Alignment.LeftOrTop;
        public Alignment Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                this.SetProperty(ref _alignment, value);
            }
        }
    }

    public enum AttachPosition { Bottom, Top, Left, Right }
    public enum Alignment { LeftOrTop, RightOrBottom, Center }
}
