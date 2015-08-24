using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis.menu;
using Windows.UI.Core;
using Windows.System;
using Windows.Devices.Input;

namespace PanoramicDataWin8.view.vis
{
    public class AttachmentView : UserControl, InputFieldViewModelEventHandler, InputGroupViewModelEventHandler, IScribbable
    {
        public static double GAP = 4;

        private DispatcherTimer _activeTimer = new DispatcherTimer();
        private bool _isActive = false;
        private Canvas _contentCanvas = new Canvas();

        private Dictionary<AttachmentHeaderViewModel, List<AttachmentItemView>> _attachmentItemViews = new Dictionary<AttachmentHeaderViewModel, List<AttachmentItemView>>();
        private Dictionary<AttachmentHeaderViewModel, AddAttachmentItemView> _addAttachmentViews = new Dictionary<AttachmentHeaderViewModel, AddAttachmentItemView>();

        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;
        private double _maxX = 0;
        private double _maxY = 0;

        public AttachmentView()
        {
            this.DataContextChanged += AttachmentView_DataContextChanged;
            this.Content = _contentCanvas;
            this.Opacity = 0;


            _activeTimer.Interval = TimeSpan.FromMilliseconds(10);
            _activeTimer.Tick += _activeTimer_Tick;
            _activeTimer.Start();
        }

        void _activeTimer_Tick(object sender, object e)
        {
            var model = (DataContext as AttachmentViewModel);
            if (model.ActiveStopwatch.Elapsed > TimeSpan.FromSeconds(2.5))
            {
                toggleActive();
                model.ActiveStopwatch.Reset();
            }

            // animate all elements to target size, position
            if (model != null)
            {
                foreach (var header in model.AttachmentHeaderViewModels)
                {
                    foreach (var item in header.AttachmentItemViewModels)
                    {
                        // position
                        if (item.Position.X == 0 && item.Position.Y == 0)
                        {
                            item.Position = item.TargetPosition;
                        }
                        else
                        {
                            var delta = item.TargetPosition - item.Position;
                            var deltaNorm = delta.Normalized();
                            var t = delta.Length;
                            item.Position = t <= 1 ? item.TargetPosition : item.Position + deltaNorm * (t / item.DampingFactor);
                        }

                        // size
                        if (item.Size.X == 0 && item.Size.Y == 0)
                        {
                            item.Size = item.TargetSize;
                        }
                        else
                        {
                            var delta = item.TargetSize - item.Size;
                            var deltaNorm = delta.Normalized();
                            var t = delta.Length;
                            item.Size = t <= 1 ? item.TargetSize : item.Size + deltaNorm * (t / item.DampingFactor);
                        }
                    }
                    if (header.AddAttachmentItemViewModel != null)
                    {
                        var item = header.AddAttachmentItemViewModel;
                        // position
                        if (item.Position.X == 0 && item.Position.Y == 0)
                        {
                            item.Position = item.TargetPosition;
                        }
                        else
                        {
                            var delta = item.TargetPosition - item.Position;
                            var deltaNorm = delta.Normalized();
                            var t = delta.Length;
                            item.Position = t <= 1 ? item.TargetPosition : item.Position + deltaNorm * (t / item.DampingFactor);
                        }

                        // size
                        if (item.Size.X == 0 && item.Size.Y == 0)
                        {
                            item.Size = item.TargetSize;
                        }
                        else
                        {
                            var delta = item.TargetSize - item.Size;
                            var deltaNorm = delta.Normalized();
                            var t = delta.Length;
                            item.Size = t <= 1 ? item.TargetSize : item.Size + deltaNorm * (t / item.DampingFactor);
                        }
                    }
                }
            }
        }

        void toggleActive()
        {
            // fade out
            if (_isActive)
            {
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = this.Opacity;
                animation.To = 0;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, this);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                if (_menuViewModel != null)
                {
                    _menuViewModel.IsToBeRemoved = true;
                    _menuViewModel.IsDisplayed = false;
                }
            }
            // fade in
            else
            {
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = this.Opacity;
                animation.To = 1;
                animation.EasingFunction = easingFunction;
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, this);
                Storyboard.SetTargetProperty(animation, "Opacity");
                storyboard.Begin();

                if (_menuViewModel != null)
                {
                    _menuViewModel.IsDisplayed = true;
                }
            }
            _isActive = !_isActive;
        }

        void AttachmentView_DataContextChanged(FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                _attachmentItemViews.Clear();
                _addAttachmentViews.Clear();
                _contentCanvas.Children.Clear();

                var model = (e.NewValue as AttachmentViewModel);
                model.AttachmentHeaderViewModels.CollectionChanged += AttachmentHeaderViewModels_CollectionChanged;
                model.VisualizationViewModel.PropertyChanged += VisualizationViewModel_PropertyChanged;
                model.PropertyChanged += AttachmentViewModel_PropertyChanged;

                foreach (var header in model.AttachmentHeaderViewModels)
                {
                    header.AttachmentItemViewModels.CollectionChanged += AttachmentItemViewModels_CollectionChanged;

                    List<AttachmentItemView> views = new List<AttachmentItemView>();
                    foreach (var item in header.AttachmentItemViewModels)
                    {
                        var attachmentView = new AttachmentItemView()
                        {
                            DataContext = item
                        };
                        views.Add(attachmentView);
                        attachmentView.PointerPressed += attachmentView_PointerPressed;
                        attachmentView.PointerEntered += attachmentView_PointerEntered;
                        _contentCanvas.Children.Add(attachmentView);
                    }
                    _attachmentItemViews.Add(header, views);

                    if (header.AddAttachmentItemViewModel != null)
                    {
                        AddAttachmentItemView addView = new AddAttachmentItemView();
                        addView.DataContext = header.AddAttachmentItemViewModel;
                        _addAttachmentViews.Add(header, addView);
                        _contentCanvas.Children.Add(addView);
                    }
                }

                updateRendering();
            }
        }

        void attachmentView_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Debug.WriteLine("entere");
        }

        void attachmentView_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.LeftControl);
            if (e.Pointer.PointerDeviceType != PointerDeviceType.Pen && (state & CoreVirtualKeyStates.Down) != CoreVirtualKeyStates.Down)
            {
                Debug.WriteLine("press");
                (DataContext as AttachmentViewModel).ActiveStopwatch.Restart();
                AttachmentItemViewModel model = (sender as AttachmentItemView).DataContext as AttachmentItemViewModel;
                displayMenu(model);
                e.Handled = true;
            }
        }

        void AttachmentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = (DataContext as AttachmentViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.IsDisplayed))
            {
                if (model.IsDisplayed)
                {
                    model.ActiveStopwatch.Reset();
                }
                if (!_isActive && model.IsDisplayed)
                {
                    toggleActive();
                }
                if (_isActive && !model.IsDisplayed)
                {
                    model.ActiveStopwatch.Restart();
                }
            }
        }

        void AttachmentHeaderViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    (item as AttachmentHeaderViewModel).AttachmentItemViewModels.CollectionChanged -= AttachmentItemViewModels_CollectionChanged;
                }
            }
            else if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as AttachmentHeaderViewModel).AttachmentItemViewModels.CollectionChanged += AttachmentItemViewModels_CollectionChanged;
                }
            }
        }

        void AttachmentItemViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var model = (item as AttachmentItemViewModel);
                    var view = _attachmentItemViews[model.AttachmentHeaderViewModel].FirstOrDefault(v => v.DataContext == model);
                    if (view != null)
                    {
                        view.PointerPressed -= attachmentView_PointerPressed;
                        view.PointerEntered -= attachmentView_PointerEntered;
                        _contentCanvas.Children.Remove(view);
                        _attachmentItemViews[model.AttachmentHeaderViewModel].Remove(view);
                        updateRendering();
                    }
                }
            }
            else if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var attachmentView = new AttachmentItemView()
                    {
                        DataContext = (item as AttachmentItemViewModel)
                    };
                    attachmentView.PointerPressed += attachmentView_PointerPressed;
                    attachmentView.PointerEntered += attachmentView_PointerEntered;
                    var model = (item as AttachmentItemViewModel);
                    var views = new List<AttachmentItemView>();
                    if (_attachmentItemViews.ContainsKey(model.AttachmentHeaderViewModel))
                    {
                        views = _attachmentItemViews[model.AttachmentHeaderViewModel];
                    }
                    else
                    {
                        _attachmentItemViews.Add(model.AttachmentHeaderViewModel, views);
                    }

                    views.Add(attachmentView);
                    model.Position = (DataContext as AttachmentViewModel).VisualizationViewModel.Position;
                    _contentCanvas.Children.Insert(0, attachmentView);
                    updateRendering();
                }
            }
        }

        void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var visModel = (DataContext as AttachmentViewModel).VisualizationViewModel;
            if (e.PropertyName == visModel.GetPropertyName(() => visModel.Size) ||
                e.PropertyName == visModel.GetPropertyName(() => visModel.Position))
            {
                updateRendering();
            }
        }

        private void updateRendering()
        {
            AttachmentViewModel model = (DataContext as AttachmentViewModel);
            _maxX = 0;
            _maxY = 0;

            if (model.AttachmentOrientation == AttachmentOrientation.Left)
            {
                var availableHeight = model.VisualizationViewModel.Size.Y;
                var currentY = model.VisualizationViewModel.Position.Y;
                if (availableHeight > calculateMinPreferedSizeY(model.AttachmentHeaderViewModels))
                {
                    var remainingHeaders = model.AttachmentHeaderViewModels.ToList();
                    foreach (var header in model.AttachmentHeaderViewModels)
                    {
                        remainingHeaders.Remove(header);
                        double headerHeight = header.PreferedItemSize.Y;
                        int nrRows = (availableHeight - calculateMinPreferedSizeY(remainingHeaders)) + GAP > 2 * header.PreferedItemSize.Y + GAP ? 2 : 1;
                        int upperNrElemPerRow = (int) Math.Ceiling((double)header.AttachmentItemViewModels.Count / (double)nrRows);
                        int lowerNrElemPerRow = (int) Math.Floor((double)header.AttachmentItemViewModels.Count / (double)nrRows);
                        double currentX = model.VisualizationViewModel.Position.X - (upperNrElemPerRow * header.PreferedItemSize.Y + (upperNrElemPerRow) * GAP);

                        if (header.AddAttachmentItemViewModel != null)
                        {
                            header.AddAttachmentItemViewModel.TargetPosition = new Pt(currentX - (header.AddAttachmentItemViewModel.Size.X + GAP), currentY);
                            _maxX = Math.Max(_maxX, header.AddAttachmentItemViewModel.TargetPosition.X);
                           // header.AddAttachmentItemViewModel.Size = new Vec(300, 300);
                        }


                        int currentRow = 0;
                        int count = 0;
                        foreach (var item in header.AttachmentItemViewModels)
                        {
                            var itemView = _attachmentItemViews[header].FirstOrDefault(v => v.DataContext == item);
                            item.TargetPosition = new Pt(currentX, currentY);
                            currentX += header.PreferedItemSize.X + GAP;
                            count++;
                            if (count == upperNrElemPerRow)
                            {
                                currentRow++;
                                currentY += header.PreferedItemSize.X + GAP;
                                currentX = model.VisualizationViewModel.Position.X - (lowerNrElemPerRow * header.PreferedItemSize.X + (lowerNrElemPerRow) * GAP);
                            }
                        }
                        availableHeight -= headerHeight;
                    }
                }
            }
            else if (model.AttachmentOrientation == AttachmentOrientation.Bottom)
            {
                var availableWidth = model.VisualizationViewModel.Size.X;
                var tt = this.Opacity;
                if (availableWidth > calculateMinPreferedSizeX(model.AttachmentHeaderViewModels))
                {
                    var remainingHeaders = model.AttachmentHeaderViewModels.ToList();
                    var maxX = 0d;
                    foreach (var header in model.AttachmentHeaderViewModels)
                    {
                        var startX = model.VisualizationViewModel.Position.X;
                        var offsetX =  (model.VisualizationViewModel.QueryModel.VisualizationType != VisualizationType.table ? 50 + GAP : 0);
                        var currentX = startX;
                        remainingHeaders.Remove(header);
                        double headerWidth = header.PreferedItemSize.X;
                        int nrCols = (availableWidth - calculateMinPreferedSizeX(remainingHeaders)) + GAP > 2 * header.PreferedItemSize.X + GAP ? 2 : 1;
                        int leftNrElemPerCol = (int)Math.Ceiling((double)header.AttachmentItemViewModels.Count / (double)nrCols);
                        int rightNrElemPercol = (int)Math.Floor((double)header.AttachmentItemViewModels.Count / (double)nrCols);
                        double currentY = (model.VisualizationViewModel.Position.Y + model.VisualizationViewModel.Size.Y) + GAP;// -(upperNrElemPerCol * header.PreferedItemSize.Y + (upperNrElemPerCol) * GAP);

                        int currentCol = 0;
                        int count = 0;
                        foreach (var item in header.AttachmentItemViewModels)
                        {
                            var itemView = _attachmentItemViews[header].FirstOrDefault(v => v.DataContext == item);
                            item.TargetPosition = new Pt(maxX + currentX + offsetX, currentY);
                            currentY += header.PreferedItemSize.Y + GAP;
                            count++;
                            if (count == leftNrElemPerCol)
                            {
                                currentCol++;
                                currentY = (model.VisualizationViewModel.Position.Y + model.VisualizationViewModel.Size.Y) + GAP;
                                currentX = startX + header.PreferedItemSize.Y + GAP;
                            }
                        }
                        if (header.AddAttachmentItemViewModel != null)
                        {
                            header.AddAttachmentItemViewModel.TargetPosition = new Pt(
                                maxX + startX + offsetX, 
                                (model.VisualizationViewModel.Position.Y + model.VisualizationViewModel.Size.Y) + GAP + 
                                leftNrElemPerCol * header.PreferedItemSize.Y + leftNrElemPerCol * GAP);
                            // header.AddAttachmentItemViewModel.Size = new Vec(300, 300);

                            _maxY = Math.Max(_maxY, header.AddAttachmentItemViewModel.TargetPosition.Y + header.AddAttachmentItemViewModel.Size.Y);
                        }

                        if (header.AttachmentItemViewModels.Count > 0)
                        {
                            maxX = header.AttachmentItemViewModels.Max(item => item.TargetPosition.X) + header.PreferedItemSize.X - model.VisualizationViewModel.Position.X + GAP - offsetX;
                        }
                        else
                        {
                            maxX += header.PreferedItemSize.X + GAP;
                        }
                        availableWidth -= headerWidth; 
                        
                    }
                }
            }
            else if (model.AttachmentOrientation == AttachmentOrientation.Top)
            {
                var availableWidth = model.VisualizationViewModel.Size.X;
                var tt = this.Opacity;
                if (availableWidth > calculateMinPreferedSizeX(model.AttachmentHeaderViewModels))
                {
                    var remainingHeaders = model.AttachmentHeaderViewModels.ToList();
                    var maxX = 0d;
                    foreach (var header in model.AttachmentHeaderViewModels)
                    {
                        var startX = model.VisualizationViewModel.Position.X;
                        var offsetX = (model.VisualizationViewModel.QueryModel.VisualizationType != VisualizationType.table ? 50 + GAP : 0);
                        var currentX = startX;
                        remainingHeaders.Remove(header);
                        double headerWidth = header.PreferedItemSize.X;
                        int nrCols = (availableWidth - calculateMinPreferedSizeX(remainingHeaders)) + GAP > 2 * header.PreferedItemSize.X + GAP ? 2 : 1;
                        int leftNrElemPerCol = (int)Math.Ceiling((double)header.AttachmentItemViewModels.Count / (double)nrCols);
                        int rightNrElemPercol = (int)Math.Floor((double)header.AttachmentItemViewModels.Count / (double)nrCols);
                        double currentY = (model.VisualizationViewModel.Position.Y ) - GAP;// -(upperNrElemPerCol * header.PreferedItemSize.Y + (upperNrElemPerCol) * GAP);

                        int currentCol = 0;
                        int count = 0;
                        foreach (var item in header.AttachmentItemViewModels)
                        {
                            var itemView = _attachmentItemViews[header].FirstOrDefault(v => v.DataContext == item);
                            item.TargetPosition = new Pt(maxX + currentX + offsetX, currentY - header.PreferedItemSize.Y);
                            currentY -= header.PreferedItemSize.Y + GAP;
                            count++;
                            if (count == leftNrElemPerCol)
                            {
                                currentCol++;
                                currentY = (model.VisualizationViewModel.Position.Y + model.VisualizationViewModel.Size.Y) + GAP;
                                currentX = startX + header.PreferedItemSize.Y + GAP;
                            }
                        }
                        if (header.AddAttachmentItemViewModel != null)
                        {
                            header.AddAttachmentItemViewModel.TargetPosition = new Pt(
                                maxX + startX + offsetX,
                                (model.VisualizationViewModel.Position.Y) - GAP - header.PreferedItemSize.Y -
                                leftNrElemPerCol * header.PreferedItemSize.Y - leftNrElemPerCol * GAP);
                            // header.AddAttachmentItemViewModel.Size = new Vec(300, 300);

                            _maxY = Math.Max(_maxY, header.AddAttachmentItemViewModel.TargetPosition.Y + header.AddAttachmentItemViewModel.Size.Y);
                        }

                        if (header.AttachmentItemViewModels.Count > 0)
                        {
                            maxX = header.AttachmentItemViewModels.Max(item => item.TargetPosition.X) + header.PreferedItemSize.X - model.VisualizationViewModel.Position.X + GAP - offsetX;
                        }
                        else
                        {
                            maxX += header.PreferedItemSize.X + GAP;
                        }
                        availableWidth -= headerWidth;

                    }
                }
            }
            // update menu anker
            setMenuViewModelAnkerPosition();
        }

        private void displayMenu(AttachmentItemViewModel itemModel)
        {
            AttachmentViewModel model = (DataContext as AttachmentViewModel);
            bool createNew = true;

            if (_menuViewModel != null && !_menuViewModel.IsToBeRemoved)
            {
                createNew = _menuViewModel.AttachmentItemViewModel != itemModel;
                foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                {
                    menuItem.TargetPosition = _menuViewModel.AttachmentItemViewModel.Position;
                }
                _menuViewModel.IsToBeRemoved = true;
                _menuViewModel.IsDisplayed = false;
            }

            if (createNew)
            {
                var menuViewModel = model.CreateMenuViewModel(itemModel);
                if (menuViewModel.MenuItemViewModels.Count > 0)
                {
                    _menuViewModel = menuViewModel;
                    _menuView = new MenuView()
                    {
                        DataContext = _menuViewModel
                    };
                    setMenuViewModelAnkerPosition();
                    MainViewController.Instance.InkableScene.Add(_menuView);
                    _menuViewModel.IsDisplayed = true;
                }
            }
        }

        private void setMenuViewModelAnkerPosition()
        {
            if (_menuViewModel != null)
            {
                if (_menuViewModel.IsToBeRemoved)
                {
                    foreach (var menuItem in _menuViewModel.MenuItemViewModels)
                    {
                        menuItem.TargetPosition = _menuViewModel.AttachmentItemViewModel.TargetPosition;
                    }
                }
                else
                {
                    AttachmentViewModel model = (DataContext as AttachmentViewModel);

                    if (model.AttachmentOrientation == AttachmentOrientation.Left)
                    {
                        _menuViewModel.AnkerPosition = new Pt(_maxX, _menuViewModel.AttachmentItemViewModel.TargetPosition.Y);
                    }
                    else if (model.AttachmentOrientation == AttachmentOrientation.Bottom)
                    {
                        _menuViewModel.AnkerPosition = new Pt(_menuViewModel.AttachmentItemViewModel.TargetPosition.X, _maxY);
                    }
                }
            }
        }

        private double calculateMinPreferedSizeX(IEnumerable<AttachmentHeaderViewModel> headers)
        {
            return headers.Sum(h => h.PreferedItemSize.X) + (headers.Count() - 1) * GAP;
        }

        private double calculateMinPreferedSizeY(IEnumerable<AttachmentHeaderViewModel> headers)
        {
            return headers.Sum(h => h.PreferedItemSize.Y) + (headers.Count() - 1) * GAP;
        }

        public GeoAPI.Geometries.IGeometry BoundsGeometry
        {
            get
            {
                GeoAPI.Geometries.IGeometry geom = null;
                foreach (var h in _attachmentItemViews.Keys)
                {
                    foreach (var v in _attachmentItemViews[h])
                    {
                        var bounds = v.GetBounds(MainViewController.Instance.InkableScene);
                        if (geom == null)
                        {
                            geom = bounds.GetPolygon();
                        }
                        else
                        {
                            geom = geom.Union(bounds.GetPolygon().Buffer(5));
                        }
                    }
                    if (_addAttachmentViews.ContainsKey(h))
                    {
                        var bounds = _addAttachmentViews[h].GetBounds(MainViewController.Instance.InkableScene);
                        if (geom == null)
                        {
                            geom = bounds.GetPolygon();
                        }
                        else
                        {
                            geom = geom.Union(bounds.GetPolygon().Buffer(5));
                        }
                    }
                }
                
                return geom;
            }
        }

        public void InputGroupViewModelMoved(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement)
        {
            AttachmentViewModel model = (DataContext as AttachmentViewModel);
            model.IsDisplayed = true;

            var closestModel = overElement ? getClosestModel(e.Bounds) : null;
            if (closestModel != null && closestModel is AddAttachmentItemViewModel &&
                (closestModel as AddAttachmentItemViewModel).AttachmentHeaderViewModel.AcceptsInputGroupModels)
            {
                (closestModel as AddAttachmentItemViewModel).IsActive = true;
            }

            foreach (var header in model.AttachmentHeaderViewModels)
            {
                if (header.AddAttachmentItemViewModel != null && header.AddAttachmentItemViewModel != closestModel)
                {
                    header.AddAttachmentItemViewModel.IsActive = false;
                }
            }
        }

        public void InputGroupViewModelDropped(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement)
        {
            AttachmentViewModel model = (DataContext as AttachmentViewModel);
            model.IsDisplayed = false;
            foreach (var header in model.AttachmentHeaderViewModels)
            {
                if (header.AddAttachmentItemViewModel != null)
                {
                    header.AddAttachmentItemViewModel.IsActive = false;
                }
            }

            var closestModel = overElement ? getClosestModel(e.Bounds) : null;
            if (closestModel != null && closestModel is AddAttachmentItemViewModel && (closestModel as AddAttachmentItemViewModel).AttachmentHeaderViewModel.AcceptsInputGroupModels &&
                (closestModel as AddAttachmentItemViewModel).AttachmentHeaderViewModel.AddedTriggered != null)
            {
                (closestModel as AddAttachmentItemViewModel).AttachmentHeaderViewModel.AddedTriggered(new InputOperationModel(e.InputGroupModel));
            }
        }

        public void InputFieldViewModelMoved(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            AttachmentViewModel model = (DataContext as AttachmentViewModel);
            model.IsDisplayed = true;

            var closestModel = overElement ? getClosestModel(e.Bounds) : null;
            if (closestModel != null && closestModel is AddAttachmentItemViewModel &&
                (closestModel as AddAttachmentItemViewModel).AttachmentHeaderViewModel.AcceptsInputModels)
            {
                (closestModel as AddAttachmentItemViewModel).IsActive = true;
            }

            foreach (var header in model.AttachmentHeaderViewModels)
            {
                if (header.AddAttachmentItemViewModel != null && header.AddAttachmentItemViewModel != closestModel)
                {
                    header.AddAttachmentItemViewModel.IsActive = false;
                }
            }
        }
        
        public void InputFieldViewModelDropped(InputFieldViewModel sender, InputFieldViewModelEventArgs e, bool overElement)
        {
            AttachmentViewModel model = (DataContext as AttachmentViewModel);
            model.IsDisplayed = false;
            foreach (var header in model.AttachmentHeaderViewModels)
            {
                if (header.AddAttachmentItemViewModel != null)
                {
                    header.AddAttachmentItemViewModel.IsActive = false;
                }
            }

            var closestModel = overElement ? getClosestModel(e.Bounds) : null;
            if (closestModel != null && closestModel is AddAttachmentItemViewModel && (closestModel as AddAttachmentItemViewModel).AttachmentHeaderViewModel.AddedTriggered != null)
            {
                (closestModel as AddAttachmentItemViewModel).AttachmentHeaderViewModel.AddedTriggered(e.InputOperationModel);
            }
        }

        private object getClosestModel(Rct bounds)
        {
            object closest = null;
            double closestDist = double.MaxValue;
            AttachmentViewModel model = (DataContext as AttachmentViewModel);

            foreach (var header in model.AttachmentHeaderViewModels)
            {
                foreach (var item in header.AttachmentItemViewModels.Where(i => i.IsDropTarget))
                {
                    double dist = (new Rct(item.Position, item.Size).Center - bounds.Center).LengthSquared;
                    if (dist < closestDist)
                    {
                        closest = item;
                        closestDist = dist;
                    }
                }
                if (header.AddAttachmentItemViewModel != null)
                {
                    double dist = (new Rct(header.AddAttachmentItemViewModel.Position, header.AddAttachmentItemViewModel.Size).Center - bounds.Center).LengthSquared;
                    if (dist < closestDist)
                    {
                        closest = header.AddAttachmentItemViewModel;
                        closestDist = dist;
                    }
                }
            }
            return closest;
        }

        public GeoAPI.Geometries.IGeometry Geometry
        {
            get { return null; }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }

        public List<IScribbable> Children
        {
            get
            {
                List<IScribbable> ret = new List<IScribbable>();
                foreach (var v in _attachmentItemViews.Values)
                {
                    ret.AddRange(v);
                }
                return ret;
            }
        }
    }

}
