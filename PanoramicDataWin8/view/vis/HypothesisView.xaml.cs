﻿using System;
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
            if (e.PropertyName == _model.GetPropertyName(() => _model.GetDecisionResult))
            {
                updateRendering();
            }
        }

        private void updateRendering()
        {
            if (_model.GetDecisionResult != null)
            {
                pValueTB.Text = _model.GetDecisionResult.Decision.PValue.ToString("F3");
                if (_model.GetDecisionResult.Decision.Significance)
                {
                    mainGrid.Background = _rejectedBrush;
                }
                else
                {
                    mainGrid.Background = _acceptedBrush;
                }
            }
            else
            {
                mainGrid.BorderBrush = _lightBrush;
            }
        }
    }
}
