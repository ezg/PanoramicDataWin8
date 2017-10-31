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
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class StatisticalComparisonMenuItemView : UserControl
    {
        private StatisticalComparisonMenuItemViewModel _model = null;
        private StatisticalComparisonOperationModel _statModel = null;
        private Brush _lightBrush = new SolidColorBrush(Helpers.GetColorFromString("#e6e6e6"));
        private Brush _acceptedBrush = new SolidColorBrush(Color.FromArgb(255, 253, 93, 76));
        private Brush _rejectedBrush = new SolidColorBrush(Color.FromArgb(255, 107, 197, 101));

        public StatisticalComparisonMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += HypothesisView_DataContextChanged;
        }
        private void HypothesisView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
                if (_statModel != null)
                {
                    _statModel.PropertyChanged -= _statModel_PropertyChanged;
                }
            }
            if (args.NewValue is MenuItemViewModel)
            {
                _model = ((MenuItemViewModel)args.NewValue).MenuItemComponentViewModel as StatisticalComparisonMenuItemViewModel;
                _model.PropertyChanged += _model_PropertyChanged;

                if (_model.StatisticalComparisonOperationModel != null)
                {
                    if (_statModel != null)
                    {
                        _statModel.PropertyChanged -= _statModel_PropertyChanged;
                    }
                    _statModel = _model.StatisticalComparisonOperationModel;
                    _statModel.PropertyChanged += _statModel_PropertyChanged;
                }
                updateRendering();
            }
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _model.GetPropertyName(() => _model.StatisticalComparisonOperationModel))
            {
                if (_statModel != null)
                {
                    _statModel.PropertyChanged -= _statModel_PropertyChanged;
                }
                _statModel = _model.StatisticalComparisonOperationModel;
                _statModel.PropertyChanged += _statModel_PropertyChanged;
                updateRendering();
            }
        }

        private void _statModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _statModel.GetPropertyName(() => _statModel.Decision))
            {
                updateRendering();
            }
        }

        private void updateRendering()
        {
            if (_model.StatisticalComparisonOperationModel != null && 
                _model.StatisticalComparisonOperationModel.Decision != null && 
                _model.StatisticalComparisonOperationModel.Decision.Progress > 0)
            {
                pLabelTB.Visibility = Visibility.Visible;
                pValueTB.Visibility = Visibility.Visible;

                pValueTB.Text = _model.StatisticalComparisonOperationModel.Decision.PValue.ToString("F3");
                if (_model.StatisticalComparisonOperationModel.Decision.Significance)
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
                pLabelTB.Visibility = Visibility.Collapsed;
                pValueTB.Visibility = Visibility.Collapsed;
                backgroundGrid.BorderBrush = _lightBrush;
            }
        }
    }
}
