using System;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class MenuItemViewModel : ExtendedBindableBase
    {
        private static readonly Random random = new Random();

        private int _column;

        private int _columnSpan = 1;

        private double _dampingFactor;

        private bool _isAlwaysDisplayed;

        private bool _isDropTarget;

        private bool _isHeightBoundToParent;

        private bool _isWidthBoundToParent;

        private MenuItemComponentViewModel _menuItemComponentViewModel;

        private MenuViewModel _menuViewModel;

        private Pt _position = new Pt(0, 0);

        private int _row;

        private int _rowSpan = 1;

        private Vec _size = new Vec(50, 50);

        private Pt _targetPosition = new Pt(0, 0);

        private Vec _targetSize = new Vec(50, 50);

        public MenuItemViewModel()
        {
            _dampingFactor = random.NextDouble()*3.0 + 3;
        }

        public MenuViewModel MenuViewModel
        {
            get { return _menuViewModel; }
            set { SetProperty(ref _menuViewModel, value); }
        }

        public double DampingFactor
        {
            get { return _dampingFactor; }
            set { SetProperty(ref _dampingFactor, value); }
        }

        public bool IsDropTarget
        {
            get { return _isDropTarget; }
            set { SetProperty(ref _isDropTarget, value); }
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

        public Vec TargetSize
        {
            get { return _targetSize; }
            set { SetProperty(ref _targetSize, value); }
        }

        public Pt TargetPosition
        {
            get { return _targetPosition; }
            set { SetProperty(ref _targetPosition, value); }
        }

        public bool IsWidthBoundToParent
        {
            get { return _isWidthBoundToParent; }
            set { SetProperty(ref _isWidthBoundToParent, value); }
        }

        public bool IsHeightBoundToParent
        {
            get { return _isHeightBoundToParent; }
            set { SetProperty(ref _isHeightBoundToParent, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public Pt Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        public bool IsAlwaysDisplayed
        {
            get { return _isAlwaysDisplayed; }
            set { SetProperty(ref _isAlwaysDisplayed, value); }
        }

        public MenuItemComponentViewModel MenuItemComponentViewModel
        {
            get { return _menuItemComponentViewModel; }
            set { SetProperty(ref _menuItemComponentViewModel, value); }
        }

        public event EventHandler Deleted;

        public void FireDeleted()
        {
            Deleted?.Invoke(this, new EventArgs());
        }
    }
}