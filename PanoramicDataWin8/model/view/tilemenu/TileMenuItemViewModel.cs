using System;
using System.Collections.ObjectModel;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view.tilemenu
{
    public class TileMenuItemViewModel : ExtendedBindableBase
    {
        private static readonly Random _random = new Random();

        private Alignment _alignment = Alignment.LeftOrTop;

        private bool _areChildrenExpanded;

        private AttachPosition _attachPosition = AttachPosition.Bottom;

        private ObservableCollection<TileMenuItemViewModel> _children = new ObservableCollection<TileMenuItemViewModel>();

        private double _childrenGap = 4;

        private int _childrenNrColumns;

        private int _childrenNrRows;

        private int _column;

        private int _columnSpan = 1;

        private Pt _currentPosition = new Pt(0, 0);

        private double _dampingFactor;

        private bool _isBeingRemoved;

        private bool _isDisplayed;

        private bool _isEnabled = true;

        private TileMenuItemViewModel _parent;

        private int _row;

        private int _rowSpan = 1;

        private Vec _size = new Vec(50, 50);

        private Pt _targetPosition = new Pt(0, 0);

        private TileMenuContentViewModel _tileMenuContentViewModel;

        public TileMenuItemViewModel(TileMenuItemViewModel parent)
        {
            Parent = parent;
            _dampingFactor = _random.NextDouble()*1.0 + 3;
        }

        public TileMenuItemViewModel Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        public TileMenuContentViewModel TileMenuContentViewModel
        {
            get { return _tileMenuContentViewModel; }
            set { SetProperty(ref _tileMenuContentViewModel, value); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                SetProperty(ref _isEnabled, value);
                foreach (var menuItemViewModel in Children)
                    menuItemViewModel.IsEnabled = value;
            }
        }

        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set { SetProperty(ref _isDisplayed, value); }
        }

        public bool IsBeingRemoved
        {
            get { return _isBeingRemoved; }
            set
            {
                SetProperty(ref _isBeingRemoved, value);
                if (value)
                    foreach (var menuItemViewModel in Children)
                        menuItemViewModel.IsBeingRemoved = value;
            }
        }

        public bool AreChildrenExpanded
        {
            get { return _areChildrenExpanded; }
            set
            {
                SetProperty(ref _areChildrenExpanded, value);
                if (!value)
                {
                    foreach (var menuItemViewModel in Children)
                        menuItemViewModel.AreChildrenExpanded = value;
                }
                else
                {
                    if (Parent != null)
                        foreach (var menuItemViewModel in Parent.Children)
                            if (menuItemViewModel != this)
                                menuItemViewModel.AreChildrenExpanded = false;
                }
            }
        }

        public ObservableCollection<TileMenuItemViewModel> Children
        {
            get { return _children; }
            set { SetProperty(ref _children, value); }
        }

        public int ChildrenNrColumns
        {
            get { return _childrenNrColumns; }
            set { SetProperty(ref _childrenNrColumns, value); }
        }

        public int ChildrenNrRows
        {
            get { return _childrenNrRows; }
            set { SetProperty(ref _childrenNrRows, value); }
        }

        public int Row
        {
            get { return _row; }
            set { SetProperty(ref _row, value); }
        }

        public int Column
        {
            get { return _column; }
            set { SetProperty(ref _column, value); }
        }

        public int RowSpan
        {
            get { return _rowSpan; }
            set { SetProperty(ref _rowSpan, value); }
        }

        public int ColumnSpan
        {
            get { return _columnSpan; }
            set { SetProperty(ref _columnSpan, value); }
        }

        public double DampingFactor
        {
            get { return _dampingFactor; }
            set { SetProperty(ref _dampingFactor, value); }
        }

        public double Gap
        {
            get { return _childrenGap; }
            set { SetProperty(ref _childrenGap, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public Pt TargetPosition
        {
            get { return _targetPosition; }
            set { SetProperty(ref _targetPosition, value); }
        }

        public Pt CurrentPosition
        {
            get { return _currentPosition; }
            set { SetProperty(ref _currentPosition, value); }
        }

        public AttachPosition AttachPosition
        {
            get { return _attachPosition; }
            set { SetProperty(ref _attachPosition, value); }
        }

        public Alignment Alignment
        {
            get { return _alignment; }
            set { SetProperty(ref _alignment, value); }
        }
    }
}