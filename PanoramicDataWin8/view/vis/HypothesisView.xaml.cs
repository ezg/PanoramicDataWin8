using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view
{
    public sealed partial class HypothesisView : UserControl
    {
        private HypothesisViewModel _model = null;
        private Brush _lightBrush = new SolidColorBrush(Helpers.GetColorFromString("#e6e6e6"));
        private Brush _acceptedBrush = new SolidColorBrush(Color.FromArgb(255, 253, 93, 76));
        private Brush _rejectedBrush = new SolidColorBrush(Color.FromArgb(255, 107, 197, 101));

        public HypothesisView()
        {
            this.InitializeComponent();
            this.DataContextChanged += HypothesisView_DataContextChanged;
        }

        private void HypothesisView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
            }
            if (args.NewValue is HypothesisViewModel)
            {
                _model = (HypothesisViewModel) args.NewValue;
                _model.PropertyChanged += _model_PropertyChanged;
                updateRendering();
            }
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _model.GetPropertyName(() => _model.Decision))
            {
                updateRendering();
            }
            else if (e.PropertyName == _model.GetPropertyName(() => _model.IsExpanded))
            {
                updateRendering();
            }
        }

        private void updateRendering()
        {
            if (_model.Decision != null && _model.Decision.Progress > 0)
            {
                pValueTB.Text = _model.Decision.PValue.ToString("F3");
                if (_model.Decision.Significance)
                {
                    backgroundGrid.Background = _rejectedBrush;
                }
                else
                {
                    backgroundGrid.Background = _acceptedBrush;
                }
            }
            else
            {
                backgroundGrid.BorderBrush = _lightBrush;
            }

            if (_model.IsExpanded)
            {
                pLabelTB.Visibility = Visibility.Visible;
                pValueTB.Visibility = Visibility.Visible;
            }
            else
            {
                pLabelTB.Visibility = Visibility.Collapsed;
                pValueTB.Visibility = Visibility.Collapsed;
            }

            tbDist0.Text = _model.StatisticalComparisonSaveViewModel.FilterDist0;
            tbDist1.Text = _model.StatisticalComparisonSaveViewModel.FilterDist1;
        }
    }
}
