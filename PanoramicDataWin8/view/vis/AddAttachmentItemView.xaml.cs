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

namespace PanoramicDataWin8.view.vis
{
    public sealed partial class AddAttachmentItemView : UserControl
    {
        public AddAttachmentItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += AddAttachmentItemView_DataContextChanged;
        }

        void AddAttachmentItemView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            (args.NewValue as AddAttachmentItemViewModel).PropertyChanged += AddAttachmentItemView_PropertyChanged;
        }

        void AddAttachmentItemView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = (DataContext as AddAttachmentItemViewModel);
            if (e.PropertyName == model.GetPropertyName(() => model.IsActive))
            {

                ExponentialEase easingFunction = new ExponentialEase();
                easingFunction.EasingMode = EasingMode.EaseInOut;

                ColorAnimation animation = new ColorAnimation();
                animation.From = (mainGrid.Background as SolidColorBrush).Color;
                if (model.IsActive)
                {
                    //mainGrid.Background = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush);
                    animation.To = (Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush).Color;
                }
                else
                {
                    //mainGrid.Background = (Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush);
                    animation.To = (Application.Current.Resources.MergedDictionaries[0]["lightBrush"] as SolidColorBrush).Color;
                }
                animation.EasingFunction = easingFunction;
                animation.Duration = TimeSpan.FromMilliseconds(300);
                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, mainGrid);
                Storyboard.SetTargetProperty(animation, "(Border.Background).(SolidColorBrush.Color)");
                storyboard.Begin();

            }
        }
    }
}
