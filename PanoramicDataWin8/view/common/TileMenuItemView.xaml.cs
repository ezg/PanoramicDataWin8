using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.tilemenu;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis.menu;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.common
{
    public sealed partial class TileMenuItemView : UserControl
    {
        private DispatcherTimer _animationTimer = new DispatcherTimer();
        private List<TileMenuItemView> _childernMenuItemViews = new List<TileMenuItemView>();
        public Canvas MenuCanvas { get; set; }

        public TileMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += MenuItemView_DataContextChanged;
            this.Tapped += MenuItemView_Tapped;

            _animationTimer.Interval = TimeSpan.FromMilliseconds(10);
            _animationTimer.Tick += animationTimer_Tick;
        }
        public void Dispose()
        {
            _animationTimer.Stop();
            var model = (DataContext as TileMenuItemViewModel);
            if (model != null)
            {
                model.PropertyChanged -= model_PropertyChanged;
                model.Children.CollectionChanged -= Children_CollectionChanged;
                this.DataContextChanged -= MenuItemView_DataContextChanged;
            }
        }

        void MenuItemView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var model = (DataContext as TileMenuItemViewModel);
            if (model.Children.Count > 0 && model.IsEnabled)
            {
                model.AreChildrenExpanded = !model.AreChildrenExpanded;
                e.Handled = true;
            }
        }

        void animationTimer_Tick(object sender, object e)
        {
            // animate all elements to target size, position
            var model = (DataContext as TileMenuItemViewModel);
            if (model != null && model.IsDisplayed)
            {
                var delta = model.TargetPosition - model.CurrentPosition;
                var deltaNorm = delta.Normalized();
                var t = delta.Length;
                model.CurrentPosition = t <= 1 ? model.TargetPosition : model.CurrentPosition + deltaNorm * (t / model.DampingFactor);

                if (t <= 1 && model.IsBeingRemoved)
                {
                    model.IsDisplayed = false;
                    model.IsBeingRemoved = false;
                    if (MenuCanvas != null && MenuCanvas.Children.Contains(this))
                    {
                        MenuCanvas.Children.Remove(this);
                    }
                    else
                    {

                    }
                    _animationTimer.Stop();
                }
                updateChildrenTargetPositions();
            }
        }

        private void MenuItemView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            TileMenuItemViewModel model = (TileMenuItemViewModel)DataContext;
            model.PropertyChanged -= model_PropertyChanged;
            model.Children.CollectionChanged -= Children_CollectionChanged;
            model.PropertyChanged += model_PropertyChanged;
            model.Children.CollectionChanged += Children_CollectionChanged;

            // initialize children
            foreach (var oldChildrenViews in _childernMenuItemViews)
            {
                oldChildrenViews.Dispose();
                if (MenuCanvas != null && MenuCanvas.Children.Contains(oldChildrenViews))
                {
                    MenuCanvas.Children.Remove(oldChildrenViews);
                }
            }
            _childernMenuItemViews.Clear();
            foreach (var menuItemViewModel in model.Children)
            {
                TileMenuItemView menutItemView = new TileMenuItemView() { DataContext = menuItemViewModel };
                _childernMenuItemViews.Add(menutItemView);
            }
            updateChildrenTargetPositions();


            if (model.TileMenuContentViewModel == null)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (model.TileMenuContentViewModel is InputFieldViewTileMenuContentViewModel)
                {
                    mainGrid.Children.Clear();
                    mainGrid.Children.Add(new InputFieldView()
                    {
                        DataContext =
                            ((InputFieldViewTileMenuContentViewModel) model.TileMenuContentViewModel).AttributeTransformationViewModel
                    });
                }
                else if (model.TileMenuContentViewModel is OperationTypeTileMenuContentViewModel)
                {
                    mainGrid.Children.Clear();
                    mainGrid.Children.Add(new OperationTypeView()
                    {
                        DataContext =
                            ((OperationTypeTileMenuContentViewModel)model.TileMenuContentViewModel).OperationTypeModel
                    });
                }
                else if (model.TileMenuContentViewModel is OperationTypeGroupTileMenuContentViewModel)
                {
                    mainGrid.Children.Clear();
                    mainGrid.Children.Add(new OperationTypeView()
                    {
                        DataContext =
                            ((OperationTypeGroupTileMenuContentViewModel)model.TileMenuContentViewModel).OperationTypeGroupModel
                    });
                }
                else if (model.TileMenuContentViewModel is InputGroupViewTileMenuContentViewModel)
                {
                    mainGrid.Children.Clear();
                    mainGrid.Children.Add(new InputGroupView()
                    {
                        DataContext =
                            ((InputGroupViewTileMenuContentViewModel)model.TileMenuContentViewModel).InputGroupViewModel
                    });
                }
            }
        }

        void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var model = (item as TileMenuItemViewModel);
                    var view = _childernMenuItemViews.FirstOrDefault(v => v.DataContext == model);
                    if (MenuCanvas.Children.Contains(view))
                    {
                        view.Dispose();
                        MenuCanvas.Children.Remove(view);
                    }
                }
            }
            else if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var model = (item as TileMenuItemViewModel);
                    TileMenuItemView menutItemView = new TileMenuItemView() { DataContext = model };
                    _childernMenuItemViews.Add(menutItemView);
                }
            }
            updateChildrenTargetPositions();
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CurrentPosition")
            {
                TileMenuItemViewModel model = (TileMenuItemViewModel)DataContext;

                if (e.PropertyName == model.GetPropertyName(() => model.AreChildrenExpanded))
                {
                    if (model.AreChildrenExpanded)
                    {
                        if (MenuCanvas != null)
                        {
                            foreach (var menuItemViewModel in model.Children)
                            {
                                var view = _childernMenuItemViews.FirstOrDefault(v => v.DataContext == menuItemViewModel);
                                view.MenuCanvas = MenuCanvas;
                                if (!MenuCanvas.Children.Contains(view))
                                {
                                    MenuCanvas.Children.Add(view);
                                    //view.SendToBack();
                                }
                                menuItemViewModel.IsEnabled = true;
                                menuItemViewModel.IsDisplayed = true;
                                menuItemViewModel.IsBeingRemoved = false;
                                menuItemViewModel.CurrentPosition = model.CurrentPosition;
                                view._animationTimer.Start();
                            }
                        }
                        //updateChildrenTargetPositions();
                    }
                    else
                    {
                        foreach (var menuItemViewModel in model.Children)
                        {
                            menuItemViewModel.IsEnabled = false;
                            menuItemViewModel.IsBeingRemoved = true;
                        }
                    }
                    this.SendToFront();
                }
                else if (e.PropertyName == model.GetPropertyName(() => model.TargetPosition))
                {
                    //updateChildrenTargetPositions();
                }
                else if (e.PropertyName == model.GetPropertyName(() => model.IsBeingRemoved))
                {
                    if (model.IsBeingRemoved)
                    {
                        ExponentialEase easingFunction = new ExponentialEase();
                        easingFunction.EasingMode = EasingMode.EaseInOut;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.Duration = TimeSpan.FromMilliseconds(400);
                        animation.From = this.Opacity;
                        animation.To = 0;
                        animation.EasingFunction = easingFunction;
                        Storyboard storyboard = new Storyboard();
                        storyboard.Children.Add(animation);
                        Storyboard.SetTarget(animation, this);
                        Storyboard.SetTargetProperty(animation, "Opacity");
                        storyboard.Begin();
                    }
                    else
                    {
                        ExponentialEase easingFunction = new ExponentialEase();
                        easingFunction.EasingMode = EasingMode.EaseInOut;

                        DoubleAnimation animation = new DoubleAnimation();
                        animation.Duration = TimeSpan.FromMilliseconds(400);
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

                updateChildrenTargetPositions();
            }
        }

        private void updateChildrenTargetPositions()
        {
            TileMenuItemViewModel model = (DataContext as TileMenuItemViewModel);
            if (model.AreChildrenExpanded)
            {
                if (model.TileMenuContentViewModel != null &&  model.TileMenuContentViewModel.Name == "bmp")
                {
                    
                }
                if (model.AttachPosition == AttachPosition.Left)
                {
                    for (int col = model.ChildrenNrColumns - 1; col >= 0; col--)
                    {
                        for (int row = 0; row < model.ChildrenNrRows; row++)
                        {
                            var itemsInSameCol =
                                model.Children.Where(mi => mi.Row < row && mi.Column == col).ToList();
                            var itemsInSameRow =
                                model.Children.Where(
                                    mi =>
                                        mi.Column > col &&
                                        (mi.Row == row || (mi.Row < row && mi.Row + mi.RowSpan - 1 >= row))).ToList();

                            double currentY = model.CurrentPosition.Y + itemsInSameCol.Sum(mi => mi.Size.Y) +
                                              itemsInSameCol.Count * model.Gap;
                            double currentX = model.CurrentPosition.X - itemsInSameRow.Sum(mi => mi.Size.X) -
                                              itemsInSameRow.Count() * model.Gap - model.Gap;

                            var rowItem = model.Children.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                rowItem.TargetPosition = new Pt(currentX - rowItem.Size.X, currentY);
                            }
                        }
                    }
                }
                else if (model.AttachPosition == AttachPosition.Right)
                {
                    for (int col = model.ChildrenNrColumns - 1; col >= 0; col--)
                    {
                        for (int row = 0; row < model.ChildrenNrRows; row++)
                        {
                            var itemsInSameCol =
                                model.Children.Where(mi => mi.Row < row && mi.Column == col).ToList();
                            var itemsInSameRow =
                                model.Children.Where(
                                    mi =>
                                        mi.Column > col &&
                                        (mi.Row == row || (mi.Row < row && mi.Row + mi.RowSpan - 1 >= row))).ToList();

                            double currentY = model.CurrentPosition.Y + itemsInSameCol.Sum(mi => mi.Size.Y) +
                                              itemsInSameCol.Count * model.Gap;
                            if (model.Alignment == Alignment.RightOrBottom)
                            {
                                var colMaxY = getMaxColumnHeight(model);
                                currentY -= colMaxY;
                                currentY += model.Size.Y + model.Gap;
                            }
                            if (model.Alignment == Alignment.Center)
                            {
                                var colMaxY = getMaxColumnHeight(model);
                                currentY -= colMaxY / 2.0;
                                currentY += model.Size.Y / 2.0;
                            }
                            double currentX = model.CurrentPosition.X + itemsInSameRow.Sum(mi => mi.Size.X) +
                                              model.Size.X +
                                              itemsInSameRow.Count() * model.Gap + model.Gap;

                            var rowItem = model.Children.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                rowItem.TargetPosition = new Pt(currentX, currentY);
                            }
                        }
                    }
                }
                else if (model.AttachPosition == AttachPosition.Bottom)
                {
                    for (int col = 0; col < model.ChildrenNrColumns; col++)
                    {
                        for (int row = model.ChildrenNrRows - 1; row >= 0; row--)
                        {
                            var itemsInSameCol =
                                model.Children.Where(mi => mi.Row > row && mi.Column == col).ToList();
                            var itemsInSameRow =
                                model.Children.Where(mi => mi.Column < col && mi.Row == row).ToList();

                            double currentY = model.CurrentPosition.Y + itemsInSameCol.Sum(mi => mi.Size.Y) +
                                              model.Size.Y +
                                              itemsInSameCol.Count * model.Gap + model.Gap;
                            double currentX = model.CurrentPosition.X + itemsInSameRow.Sum(mi => mi.Size.X) +
                                              itemsInSameRow.Count() * model.Gap;

                            var rowItem = model.Children.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                rowItem.TargetPosition = new Pt(currentX, currentY);
                            }
                        }
                    }
                }
                else if (model.AttachPosition == AttachPosition.Top)
                {
                    for (int col = 0; col < model.ChildrenNrColumns; col++)
                    {
                        for (int row = model.ChildrenNrRows - 1; row >= 0; row--)
                        {
                            var itemsInSameCol =
                                model.Children.Where(mi => mi.Row > row && mi.Column == col).ToList();
                            var itemsInSameRow =
                                model.Children.Where(mi => mi.Column < col && mi.Row == row).ToList();

                            double currentY = model.CurrentPosition.Y - itemsInSameCol.Sum(mi => mi.Size.Y) -
                                              itemsInSameCol.Count * model.Gap - model.Gap;
                            double currentX = model.CurrentPosition.X + itemsInSameRow.Sum(mi => mi.Size.X) +
                                              itemsInSameRow.Count() * model.Gap;

                            var rowItem = model.Children.FirstOrDefault(mi => mi.Row == row && mi.Column == col);
                            if (rowItem != null)
                            {
                                rowItem.TargetPosition = new Pt(currentX, currentY - rowItem.Size.Y);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var menuItemViewModel in model.Children)
                {
                    menuItemViewModel.TargetPosition = model.TargetPosition;
                }
            }
        }

        private double getMaxColumnHeight(TileMenuItemViewModel model)
        {
            double max = 0;
            for (int col = 0; col < model.ChildrenNrColumns; col++)
            {
                var items = model.Children.Where(
                    mi =>
                        (mi.Column == col || (mi.Column < col && mi.Column + mi.ColumnSpan - 1 >= col)));
                double current = items.Sum(mi => mi.Size.Y) + (items.Count() - 1) * model.Gap;
                max = Math.Max(max, current);
            }
            return max;
        }

    }
}

