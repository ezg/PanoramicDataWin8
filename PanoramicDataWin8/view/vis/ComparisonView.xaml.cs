using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using IDEA_common.operations.risk;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.result;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.vis.menu;
using WinRTXamlToolkit.Tools;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class ComparisonView : UserControl
    {
        private StatisticalComparisonOperationViewModel _model = null;
        private Storyboard _pulsingOpeningStoryboard = null;
        private Storyboard _closingStoryboard = null;

        private MenuViewModel _menuViewModel = null;
        private MenuView _menuView = null;

        public ComparisonView()
        {
            this.InitializeComponent();
            this.DataContextChanged += ComparisonViewModel_DataContextChanged;
            this.Loaded += SetOperationView_Loaded;
        }

        void SetOperationView_Loaded(object sender, RoutedEventArgs e)
        {
            CubicEase easingFunction = new CubicEase();
            easingFunction.EasingMode = EasingMode.EaseInOut;
            Duration duration = new Duration(TimeSpan.FromMilliseconds(1000));
            _pulsingOpeningStoryboard = new Storyboard();

            DoubleAnimation animation = new DoubleAnimation();
            animation.EnableDependentAnimation = true;
            animation.Duration = duration;
            animation.From = 0;
            animation.To = 1;
            animation.EasingFunction = easingFunction;
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, "Opacity");
            _pulsingOpeningStoryboard.Children.Add(animation);

            _pulsingOpeningStoryboard.Begin();
        }

        void ComparisonViewModel_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
                _model.StatisticalComparisonOperationModel.PropertyChanged -= StatisticalComparisonOperationModel_PropertyChanged;
                foreach (var vis in _model.OperationViewModels)
                {
                    vis.PropertyChanged -= VisModel_PropertyChanged;
                }
            }
            if (args.NewValue != null)
            {
                _model = (StatisticalComparisonOperationViewModel)args.NewValue;
                _model.PropertyChanged += _model_PropertyChanged;
                _model.StatisticalComparisonOperationModel.PropertyChanged += StatisticalComparisonOperationModel_PropertyChanged;
                foreach (var vis in _model.OperationViewModels)
                {
                    vis.PropertyChanged += VisModel_PropertyChanged;
                }
                updateRendering();

                if (_menuView == null)
                {
                   _menuViewModel = new MenuViewModel
                    {
                        AttachmentOrientation = AttachmentOrientation.Right,
                        NrColumns = 1,
                        NrRows = 3
                    };
                    var toggles = new List<ToggleMenuItemComponentViewModel>();
                    var items = new List<MenuItemViewModel>();
                    TestType[] types = new TestType[] {TestType.chi2,  TestType.ttest, TestType.corr };
                    int count = 0;
                    foreach (var type in types)
                    {
                        var toggleMenuItem = new MenuItemViewModel
                        {
                            MenuViewModel = _menuViewModel,
                            Row = count,
                            RowSpan = 0,
                            Position = new Pt(0, 0),
                            Column = 0,
                            Size = new Vec(50, 22.333),
                            TargetSize = new Vec(50, 22.333),
                        };
                        //toggleMenuItem.Position = attachmentItemViewModel.Position;
                        var toggle = new ToggleMenuItemComponentViewModel
                        {
                            Label = type.ToString(),
                            IsChecked = _model.StatisticalComparisonOperationModel.TestType == type
                        };
                        toggles.Add(toggle);
                        toggleMenuItem.MenuItemComponentViewModel = toggle;
                        toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                        {
                            var model = sender2 as ToggleMenuItemComponentViewModel;
                            if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                            {
                                if (model.IsChecked)
                                {
                                    _model.StatisticalComparisonOperationModel.TestType = type;
                                    _model.StatisticalComparisonOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                                    foreach (var tg in model.OtherToggles)
                                    {
                                        tg.IsChecked = false;
                                    }
                                }
                            }
                        };
                        _menuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                        items.Add(toggleMenuItem);
                        count++;
                    }
                    foreach (var mi in items)
                    {
                        (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
                    }

                    _menuView = new MenuView()
                    {
                        DataContext = _menuViewModel
                    };
                    menuCanvas.Children.Add(_menuView);
                    _menuViewModel.IsDisplayed = true;

                }
            }
        }

        private void StatisticalComparisonOperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = (StatisticalComparisonOperationModel) sender;
            if (e.PropertyName == model.GetPropertyName(() => model.Decision))
            {
                updateRendering();
            }
        }

        private void VisModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _model.GetPropertyName(() => _model.ComparisonViewModelState))
            {
                if (_model.ComparisonViewModelState == ComparisonViewModelState.Closing)
                {
                    if (_closingStoryboard != null)
                    {
                        _closingStoryboard.Stop();
                    }
                    _pulsingOpeningStoryboard.Stop();

                    CubicEase easingFunction = new CubicEase();
                    easingFunction.EasingMode = EasingMode.EaseInOut;
                    _closingStoryboard = new Storyboard();

                    DoubleAnimation animation = new DoubleAnimation();
                    animation.EnableDependentAnimation = true;
                    animation.Duration = new Duration(TimeSpan.FromMilliseconds(400));
                    animation.From = this.Opacity;
                    animation.To = 0;
                    animation.EasingFunction = easingFunction;
                    Storyboard.SetTarget(animation, this);
                    Storyboard.SetTargetProperty(animation, "Opacity");
                    _closingStoryboard.Children.Add(animation);

                    _closingStoryboard.Begin();
                }
                else if (_model.ComparisonViewModelState == ComparisonViewModelState.Opened)
                {
                    /* CubicEase easingFunction = new CubicEase();
                     easingFunction.EasingMode = EasingMode.EaseInOut;
                     Duration duration = new Duration(TimeSpan.FromMilliseconds(400));
                     Storyboard storyboard = new Storyboard();

                     DoubleAnimation animation = new DoubleAnimation();
                     animation.EnableDependentAnimation = true;
                     animation.Duration = duration;
                     animation.From = fullView.Opacity;
                     animation.To = 1;
                     animation.EasingFunction = easingFunction;
                     Storyboard.SetTarget(animation, fullView);
                     Storyboard.SetTargetProperty(animation, "Opacity");
                     storyboard.Children.Add(animation);

                     animation = new DoubleAnimation();
                     animation.EnableDependentAnimation = true;
                     animation.Duration = duration;
                     animation.From = ellipse.Opacity;
                     animation.To = 0;
                     animation.EasingFunction = easingFunction;
                     Storyboard.SetTarget(animation, ellipse);
                     Storyboard.SetTargetProperty(animation, "Opacity");
                     storyboard.Children.Add(animation);

                     storyboard.Begin();*/
                }
            }
            updateRendering();
        }
        

        private void updateRendering()
        {
            this.SendToFront();

            var left = _model.OperationViewModels[0];
            var right = _model.OperationViewModels[1];

            if (left.Bounds.Left > right.Bounds.Left)
            {
                var temp = right;
                right = left;
                left = temp;
            }

            var lineFrom = (new Pt(left.Bounds.Right, left.Bounds.Center.Y) - _model.Position).GetWindowsPoint();
            var lineTo = (new Pt(right.Bounds.Left, right.Bounds.Center.Y) - _model.Position).GetWindowsPoint();
            

            _model.Position =
                (((left.Bounds.Center.GetVec() + new Vec(left.Size.X / 2.0, 0)) +
                  (right.Bounds.Center.GetVec() - new Vec(right.Size.X / 2.0, 0))) / 2.0 - _model.Size / 2.0 - new Vec(25, 0)).GetWindowsPoint();

            
            var getDeciRes = _model.StatisticalComparisonOperationModel.Decision;
            if (getDeciRes != null && getDeciRes.Progress > 0)
            {
                
                tbPValue.Visibility = Visibility.Visible;
                pLabelTB.Visibility = Visibility.Visible;
                
                if (getDeciRes.Significance)
                {
                    mainGrid.Background = Application.Current.Resources.MergedDictionaries[0]["rejectBrush"] as SolidColorBrush;
                }
                else
                {
                    mainGrid.Background = Application.Current.Resources.MergedDictionaries[0]["acceptBrush"] as SolidColorBrush;
                }
                tbPValue.Text = getDeciRes.PValue.ToString("F3");
            }
            else
            {
                mainGrid.Background = Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush;
                tbPValue.Visibility = Visibility.Collapsed;
                pLabelTB.Visibility = Visibility.Collapsed;
            }
        }

    }
}
