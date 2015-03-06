using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class ToggleMenuItemView : UserControl
    {
        public ToggleMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += ToggleMenuItemView_DataContextChanged;
            this.PointerPressed += ToggleMenuItemView_PointerPressed;
        }

        void ToggleMenuItemView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var model = ((DataContext as MenuItemViewModel).MenuItemComponentViewModel as ToggleMenuItemComponentViewModel);
            if (!model.IsChecked)
            {
                model.IsChecked = true;
            }
        }

        void ToggleMenuItemView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                (args.NewValue as MenuItemViewModel).MenuItemComponentViewModel.PropertyChanged += MenuItemComponentViewModel_PropertyChanged;
                if (((DataContext as MenuItemViewModel).MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).IsChecked)
                {
                    mainGrid.Background = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush);
                    txtBlock.Foreground = (Application.Current.Resources.MergedDictionaries[0]["backgroundBrush"] as SolidColorBrush);
                }
                else
                {
                    mainGrid.Background = (Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush);
                    txtBlock.Foreground = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush);
                }
            }
        }

        void MenuItemComponentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        void updateRendering()
        {
            if (DataContext != null)
            {
                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                ColorAnimation backgroundAnimation = new ColorAnimation();
                backgroundAnimation.EasingFunction = easingFunction;
                backgroundAnimation.Duration = TimeSpan.FromMilliseconds(300);
                backgroundAnimation.From = (mainGrid.Background as SolidColorBrush).Color;

                ColorAnimation foregroundAnimation = new ColorAnimation();
                foregroundAnimation.EasingFunction = easingFunction;
                foregroundAnimation.Duration = TimeSpan.FromMilliseconds(300);
                foregroundAnimation.From = (txtBlock.Foreground as SolidColorBrush).Color;

                if (((DataContext as MenuItemViewModel).MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).IsChecked)
                {
                    backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush).Color;
                    foregroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["backgroundBrush"] as SolidColorBrush).Color;
                }
                else
                {
                    backgroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush).Color;
                    foregroundAnimation.To = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush).Color;
                }


                Storyboard storyboard = new Storyboard();

                storyboard.Children.Add(backgroundAnimation);
                Storyboard.SetTarget(backgroundAnimation, mainGrid);
                Storyboard.SetTargetProperty(backgroundAnimation, "(Border.Background).(SolidColorBrush.Color)");

                storyboard.Children.Add(foregroundAnimation);
                Storyboard.SetTarget(foregroundAnimation, txtBlock);
                Storyboard.SetTargetProperty(foregroundAnimation, "(TextBlock.Foreground).Color");

                storyboard.Begin();
            }
        }
    }
}
