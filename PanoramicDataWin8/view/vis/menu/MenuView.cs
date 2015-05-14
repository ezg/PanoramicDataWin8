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

namespace PanoramicDataWin8.view.vis.menu
{
    public class MenuView : UserControl
    {
        public static double GAP = 4;

        private DispatcherTimer _activeTimer = new DispatcherTimer();
        private Canvas _contentCanvas = new Canvas();

        private List<MenuItemView> _menuViewItems = new List<MenuItemView>();

        public MenuView()
        {
            this.DataContextChanged += MenuView_DataContextChanged;
            this.Content = _contentCanvas;
            this.Opacity = 0;


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

        void toggleActive()
        {
            var model = (DataContext as MenuViewModel);

            // fade out
            if (!model.IsDisplayed)
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
                storyboard.Completed += (s, e) =>
                {
                    if (model.IsToBeRemoved) 
                    {
                        MainViewController.Instance.InkableScene.Remove(this);
                    }
                };
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
            }
        }

        void MenuView_DataContextChanged(FrameworkElement sender, Windows.UI.Xaml.DataContextChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                _menuViewItems.Clear();
                _contentCanvas.Children.Clear();

                var model = (e.NewValue as MenuViewModel);
                model.PropertyChanged += MenuViewModel_PropertyChanged;
                model.MenuItemViewModels.CollectionChanged += MenuItemViewModels_CollectionChanged;

                foreach (var item in model.MenuItemViewModels)
                {
                    var menuItemView = new MenuItemView()
                    {
                        DataContext = item
                    };
                    _menuViewItems.Add(menuItemView);
                    _contentCanvas.Children.Add(menuItemView);
                }

                updateRendering();
            }
        }

        void MenuViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = (DataContext as MenuViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.IsDisplayed))
            {
                toggleActive();
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
                    var view = _menuViewItems.FirstOrDefault(v => v.DataContext == model);
                    _contentCanvas.Children.Remove(view);
                    _menuViewItems.Remove(view);
                    updateRendering();
                }
            }
            else if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var menuItemView = new MenuItemView()
                    {
                        DataContext = (item as MenuItemViewModel)
                    };
                    var model = (item as MenuItemViewModel);
                    var views = new List<AttachmentItemView>();
                    _menuViewItems.Add(menuItemView);
                    _contentCanvas.Children.Insert(0, menuItemView);
                    updateRendering();
                }
            }
        }

        private void updateRendering()
        {
            MenuViewModel model = (DataContext as MenuViewModel);
            if (!model.IsToBeRemoved)
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
                else if (model.AttachmentOrientation == AttachmentOrientation.Bottom)
                {
                    for (int col = 0; col < model.NrColumns; col++)
                    {
                        for (int row = model.NrRows - 1; row >= 0; row--)
                        {
                            var itemsInSameCol = model.MenuItemViewModels.Where(mi => mi.Row > row && mi.Column == col).ToList();
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
                else if (model.AttachmentOrientation == AttachmentOrientation.Top)
                {
                    for (int col = 0; col < model.NrColumns; col++)
                    {
                        for (int row = model.NrRows - 1; row >= 0; row--)
                        {
                            var itemsInSameCol = model.MenuItemViewModels.Where(mi => mi.Row > row && mi.Column == col).ToList();
                            var itemsInSameRow = model.MenuItemViewModels.Where(mi => mi.Column < col && mi.Row == row).ToList();

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
    }

}
