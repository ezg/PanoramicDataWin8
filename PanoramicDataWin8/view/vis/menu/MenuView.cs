using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using GeoAPI.Geometries;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.view.inq;

namespace PanoramicDataWin8.view.vis.menu
{
    public class MenuView : UserControl, IScribbable
    {
        public static double GAP = 4;

        private DispatcherTimer _activeTimer = new DispatcherTimer();
        private Canvas _contentCanvas = new Canvas();

        private Dictionary<MenuItemViewModel, MenuItemView> _menuViewItems = new Dictionary<MenuItemViewModel, MenuItemView>();

        public MenuView()
        {
            this.DataContextChanged += MenuView_DataContextChanged;
            this.Content = _contentCanvas;
            this.Opacity = 1;


            _activeTimer.Interval = TimeSpan.FromMilliseconds(10);
            _activeTimer.Tick += _activeTimer_Tick;
            _activeTimer.Start();
        }

        void _activeTimer_Tick(object sender, object e)
        {
            // animate all elements to target size, position
            var model = (DataContext as MenuViewModel);
            if (model != null)
            {
                foreach (var item in model.MenuItemViewModels)
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
            }
        }


        private Dictionary<MenuItemViewModel, Storyboard> _storyboards = new Dictionary<MenuItemViewModel, Storyboard>();
        void toggleDisplayed()
        {
            var model = (DataContext as MenuViewModel);

            // fade out
            if (!model.IsDisplayed)
            {
                foreach (var kvp in _menuViewItems)
                {
                    if (!kvp.Key.IsAlwaysDisplayed)
                    {
                        if (_storyboards.ContainsKey(kvp.Key))
                        {
                            //_storyboards[kvp.Key].Stop();
                        }
                        ExponentialEase easingFunction = new ExponentialEase();
                        easingFunction.EasingMode = EasingMode.EaseInOut;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.From = kvp.Value.Opacity;
                        animation.To = 0;
                        animation.EasingFunction = easingFunction;
                        Storyboard storyboard = new Storyboard();
                        storyboard.Children.Add(animation);
                        Storyboard.SetTarget(animation, kvp.Value);
                        Storyboard.SetTargetProperty(animation, "Opacity");
                        storyboard.Begin();
                        storyboard.Completed += (sender, o) =>
                        {
                            kvp.Value.IsHitTestVisible = false;
                        };
                        _storyboards[kvp.Key] = storyboard;
                    }
                }
            }
            // fade in
            else
            {
                foreach (var kvp in _menuViewItems)
                {
                    if (!kvp.Key.IsAlwaysDisplayed)
                    {
                        if (_storyboards.ContainsKey(kvp.Key))
                        {
                           // _storyboards[kvp.Key].Stop();
                        }
                        kvp.Value.IsHitTestVisible = true;

                        ExponentialEase easingFunction = new ExponentialEase();
                        easingFunction.EasingMode = EasingMode.EaseInOut;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.From = kvp.Value.Opacity;
                        animation.To = 1;
                        animation.EasingFunction = easingFunction;
                        Storyboard storyboard = new Storyboard();
                        storyboard.Children.Add(animation);
                        Storyboard.SetTarget(animation, kvp.Value);
                        Storyboard.SetTargetProperty(animation, "Opacity");
                        storyboard.Begin();
                        storyboard.Completed += (sender, o) =>
                        {
                            kvp.Value.IsHitTestVisible = true;
                        };
                        _storyboards[kvp.Key] = storyboard;
                    }
                }
            }
        }

        private Storyboard fadeStoryboard(double from, double to, MenuItemView view)
        {
            ExponentialEase easingFunction = new ExponentialEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.EasingFunction = easingFunction;
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, view);
            Storyboard.SetTargetProperty(animation, "Opacity");

            return storyboard;
        }

        void MenuView_DataContextChanged(FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                _menuViewItems.Clear();
                _contentCanvas.Children.Clear();

                var model = (e.NewValue as MenuViewModel);
                model.PropertyChanged -= MenuViewModel_PropertyChanged;
                model.PropertyChanged += MenuViewModel_PropertyChanged;
                model.MenuItemViewModels.CollectionChanged -= MenuItemViewModels_CollectionChanged;
                model.MenuItemViewModels.CollectionChanged += MenuItemViewModels_CollectionChanged;
                model.Updated -= Model_Updated;
                model.Updated += Model_Updated;

                foreach (var item in model.MenuItemViewModels)
                {
                    var menuItemView = new MenuItemView()
                    {
                        DataContext = item
                    };
                    if (model.IsDisplayed || item.IsAlwaysDisplayed)
                    {
                        menuItemView.Opacity = 1;
                    }
                    else
                    {
                        menuItemView.Opacity = 0;
                    }
                    _menuViewItems.Add(item, menuItemView);
                    _contentCanvas.Children.Add(menuItemView);
                }

                updateRendering();
            }
        }

        private void Model_Updated(object sender, EventArgs e)
        {
            updateRendering();
        }

        void MenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = (DataContext as MenuViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.IsDisplayed))
            {
                if (model.MoveOnHide)
                {
                    if (model.IsDisplayed)
                    {
                        updateRendering();
                    }
                    else
                    {
                        foreach (var m in model.MenuItemViewModels)
                        {
                            m.TargetPosition = model.HidePosition;
                        }

                    }
                }
                toggleDisplayed();
            }
            else if (e.PropertyName == model.GetPropertyName(() => model.AnkerPosition))
            {
                updateRendering();
            }
        }
        void MenuItemViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var model = (item as MenuItemViewModel);
                    model.PropertyChanged -= MenuItemViewModel_PropertyChanged;
                    var view = _menuViewItems.FirstOrDefault(v => v.Key == model);
                    _contentCanvas.Children.Remove(view.Value);
                    _menuViewItems.Remove(view.Key);
                    updateRendering();
                }
            }
            else if (e.NewItems != null)
            {
                var menuModel = (DataContext as MenuViewModel);
                foreach (var item in e.NewItems)
                {
                    var menuItemView = new MenuItemView()
                    {
                        DataContext = (item as MenuItemViewModel)
                    };
                    var model = item as MenuItemViewModel;
                    model.PropertyChanged += MenuItemViewModel_PropertyChanged;
                    _menuViewItems.Add(model, menuItemView);
                    _contentCanvas.Children.Insert(0, menuItemView);
                    updateRendering();
                    menuItemView.Opacity = 0;
                    if (Opacity == 1 && menuModel.SynchronizeItemsDisplay)
                        fadeStoryboard(menuItemView.Opacity, 1, menuItemView).Begin();
                }
            }
        }

        private void MenuItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as MenuItemViewModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Row) ||
                e.PropertyName == model.GetPropertyName(() => model.Column))
            {
                updateRendering();
            }
        }

        private void updateRendering()
        {
            MenuViewModel model = (DataContext as MenuViewModel);
            {
                if (model.AttachmentOrientation == AttachmentOrientation.Left)
                {
                    for (int col = model.NrColumns - 1; col >= 0; col--)
                    {
                        for (int row = 0; row < model.NrRows; row++)
                        {
                            var itemsInSameCol = model.MenuItemViewModels.Where(mi => mi.Row < row && mi.Column == col).ToList();
                            var itemsInSameRow = model.MenuItemViewModels.Where(mi => mi.Column > col && (mi.Row == row || (mi.Row < row && mi.Row + mi.RowSpan - 1 >= row))).ToList();

                            double currentY = model.AnkerPosition.Y + itemsInSameCol.Sum(mi => mi.Size.Y) + itemsInSameCol.Count * GAP;
                            double currentX = model.AnkerPosition.X - itemsInSameRow.Sum(mi => mi.Size.X) - itemsInSameRow.Count() * GAP - GAP;

                            var rowItem = model.MenuItemViewModels.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                rowItem.TargetPosition = new Pt(currentX - rowItem.Size.X, currentY);
                            }
                        }
                    }
                }
                if (model.AttachmentOrientation == AttachmentOrientation.Right)
                {
                    if (model.IsRigid)
                    {
                        foreach (var mi in model.MenuItemViewModels)
                        {
                            double currentY = model.AnkerPosition.Y + mi.Row * GAP + mi.Row * model.RigidSize;
                            double currentX = model.AnkerPosition.X + mi.Column * GAP + mi.Column * model.RigidSize + GAP;
                            if (mi.MenuXAlign.HasFlag(MenuXAlign.Right))
                            {
                                currentX += model.RigidSize - mi.Size.X;
                            }
                            mi.TargetPosition = new Pt(currentX, currentY);
                        }
                    }
                    else
                    {
                        for (int col = 0; col < model.NrColumns; col++)
                        {
                            for (int row = 0; row < model.NrRows; row++)
                            {
                                var itemsInSameCol = model.MenuItemViewModels
                                    .Where(mi => mi.Row < row &&
                                                 (mi.Column == col ||
                                                  (mi.Column < col && mi.Column + mi.ColumnSpan - 1 >= col))).ToList();
                                var itemsInSameRow = model.MenuItemViewModels
                                    .Where(mi => mi.Column < col &&
                                                 (mi.Row == row || (mi.Row < row && mi.Row + mi.RowSpan - 1 >= row)))
                                    .ToList();
                                double currentY = model.AnkerPosition.Y + itemsInSameCol.Sum(mi => mi.Size.Y) +
                                                  itemsInSameCol.Count * GAP;
                                double currentX = model.AnkerPosition.X + itemsInSameRow.Sum(mi => mi.Size.X) +
                                                  itemsInSameRow.Count() * GAP + GAP;

                                var rowItem =
                                    model.MenuItemViewModels.FirstOrDefault(mi => mi.Row == row && mi.Column == col);

                                if (rowItem != null)
                                {
                                    rowItem.TargetPosition = new Pt(currentX, currentY);
                                }
                            }
                        }
                        foreach (var rowItem in model.MenuItemViewModels.Where(
                            ri => ri.MenuXAlign.HasFlag(MenuXAlign.WithColumn)))
                        {
                            var allInCol =
                                model.MenuItemViewModels.Where(mi => rowItem != mi && mi.Column == rowItem.Column);
                            if (allInCol.Any())
                            {
                                var maxCol = allInCol.Max(mi => mi.TargetPosition.X);
                                rowItem.TargetPosition = new Pt(maxCol, rowItem.TargetPosition.Y);
                            }
                        }
                        foreach (var rowItem in model.MenuItemViewModels.Where(
                            ri => ri.MenuYAlign.HasFlag(MenuYAlign.WithRow)))
                        {
                            var allInRow = model.MenuItemViewModels.Where(mi => rowItem != mi && mi.Row < rowItem.Row);
                            if (allInRow.Any())
                            {
                                var maxRow = allInRow.Max(mi => mi.TargetPosition.Y + mi.TargetSize.Y + GAP);
                                rowItem.TargetPosition = new Pt(rowItem.TargetPosition.X, maxRow);
                            }
                        }
                        foreach (var rowItem in model.MenuItemViewModels.Where(
                            ri => ri.MenuXAlign.HasFlag(MenuXAlign.Right)))
                        {
                            var allInCol =
                                model.MenuItemViewModels.Where(mi => rowItem != mi && mi.Column == rowItem.Column);
                            if (allInCol.Any())
                            {
                                var maxCol = allInCol.Max(mi => mi.TargetPosition.X + mi.TargetSize.X);
                                rowItem.TargetPosition =
                                    new Pt(maxCol - rowItem.TargetSize.X, rowItem.TargetPosition.Y);
                            }
                        }
                    }
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.Bottom)
                {
                    for (int col = 0; col < model.NrColumns; col++)
                    {
                        for (int row = 0; row < model.NrRows; row++)
                        {
                            var itemsInSameCol = model.MenuItemViewModels.Where(mi => mi.Row < row && (mi.Column == col || (mi.Column < col && mi.Column + mi.ColumnSpan - 1 >= col))).ToList();
                            var itemsInSameRow = model.MenuItemViewModels.Where(mi => mi.Column < col && mi.Row == row).ToList();

                            double currentY = model.AnkerPosition.Y + itemsInSameCol.Sum(mi => mi.Size.Y) + itemsInSameCol.Count * GAP + GAP;
                            double currentX = model.AnkerPosition.X + itemsInSameRow.Sum(mi => mi.Size.X) + itemsInSameRow.Count() * GAP;

                            var rowItem = model.MenuItemViewModels.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                rowItem.TargetPosition = new Pt(currentX, currentY);
                            }
                        }
                    }
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.TopStacked)
                {
                    double rowHeight = 0;
                    for (int row = model.NrRows - 1; row >= 0; row--)
                    {
                        double maxCellHeight = 0;
                        for (int col = 0; col < model.NrColumns; col++)
                            {
                            var itemsInSameCol = model.MenuItemViewModels.Where(mi => mi.Row > row && mi.Column == col).ToList();
                            var itemsInSameRow = model.MenuItemViewModels.Where(mi => mi.Column < col && (mi.Row == row || (mi.Row < row && mi.Row + mi.RowSpan - 1 >= row))).ToList();
                            var iColCnt = col;
                            var iRowCnt = model.NrRows - row -1;

                            var rowItem = model.MenuItemViewModels.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                maxCellHeight = Math.Max(maxCellHeight, rowItem.TargetSize.Y);
                                double currentY = model.AnkerPosition.Y - rowHeight - GAP;
                                double currentX = model.AnkerPosition.X + iColCnt * rowItem.TargetSize.X + iColCnt * GAP;
                                rowItem.TargetPosition = new Pt(currentX, currentY - rowItem.Size.Y);
                            }
                        }
                        rowHeight += maxCellHeight + GAP;
                    }
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.Top)
                {
                    for (int col = 0; col < model.NrColumns; col++)
                    {
                        for (int row = model.NrRows - 1; row >= 0; row--)
                        {
                            var itemsInSameCol = model.MenuItemViewModels.Where(mi => mi.Row > row && mi.Column == col).ToList();
                            var itemsInSameRow = model.MenuItemViewModels.Where(mi => mi.Column < col && (mi.Row == row || (mi.Row < row && mi.Row + mi.RowSpan - 1 >= row))).ToList();

                            double currentY = model.AnkerPosition.Y - itemsInSameCol.Sum(mi => mi.Size.Y) - itemsInSameCol.Count * GAP - GAP;
                            double currentX = model.AnkerPosition.X + itemsInSameRow.Sum(mi => mi.Size.X) + itemsInSameRow.Count() * GAP;

                            var rowItem = model.MenuItemViewModels.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                rowItem.TargetPosition = new Pt(currentX, currentY - rowItem.Size.Y);
                            }
                        }
                    }
                }
                else if (model.AttachmentOrientation == AttachmentOrientation.TopRight)
                {

                    if (model.IsRigid)
                    {
                        foreach (var mi in model.MenuItemViewModels)
                        {
                            double currentY = model.AnkerPosition.Y + mi.Row * GAP + mi.Row * model.RigidSize;
                            double currentX = model.AnkerPosition.X + mi.Column * GAP + mi.Column * model.RigidSize + GAP;
                            if (mi.MenuXAlign.HasFlag(MenuXAlign.Right))
                            {
                                currentX += model.RigidSize - mi.Size.X;
                            }
                            mi.TargetPosition = new Pt(currentX -30, currentY -30);
                        }
                    }
                    else
                    {
                        for (int col = 0; col < model.NrColumns; col++)
                        {
                            for (int row = 0; row < model.NrRows; row++)
                            {
                                var itemsInSameCol = model.MenuItemViewModels
                                    .Where(mi => mi.Row < row &&
                                                 (mi.Column == col ||
                                                  (mi.Column < col && mi.Column + mi.ColumnSpan - 1 >= col))).ToList();
                                var itemsInSameRow = model.MenuItemViewModels
                                    .Where(mi => mi.Column < col &&
                                                 (mi.Row == row || (mi.Row < row && mi.Row + mi.RowSpan - 1 >= row)))
                                    .ToList();
                                double currentY = model.AnkerPosition.Y + itemsInSameCol.Sum(mi => mi.Size.Y) +
                                                  itemsInSameCol.Count * GAP;
                                double currentX = model.AnkerPosition.X - itemsInSameRow.Sum(mi => mi.Size.X) -
                                                  itemsInSameRow.Count() * GAP;

                                var rowItem =
                                    model.MenuItemViewModels.FirstOrDefault(mi => mi.Row == row && mi.Column == col);

                                if (rowItem != null)
                                {
                                    rowItem.TargetPosition = new Pt(currentX - rowItem.Size.X, currentY-30);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public bool IsDeletable { get { return false; } }
        public IGeometry Geometry { get; }

        public List<IScribbable> Children
        {
            get { return _menuViewItems.Values.Select(a => a as IScribbable).ToList(); }
        }

        public bool Consume(InkStroke inkStroke)
        {
            throw new NotImplementedException();
        }
    }

}
