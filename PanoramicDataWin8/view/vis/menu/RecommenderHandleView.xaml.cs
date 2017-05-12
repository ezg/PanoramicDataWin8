﻿using System;
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
using Windows.UI.Xaml.Navigation;
using IDEA_common.operations.risk;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class RecommenderHandleView : UserControl
    {
        private RecommenderHandleViewModel _model = null;
        public RecommenderHandleView()
        {
            this.InitializeComponent();
            this.Loaded += RecommenderHandleView_Loaded;
            this.DataContextChanged += RecommenderHandleView_DataContextChanged;
        }

        private void RecommenderHandleView_Loaded(object sender, RoutedEventArgs e)
        {
            if (HypothesesViewController.Instance == null)
            {
                HypothesesViewController.Initialized += HypothesesViewController_Initialized;
            }
            else
            {
                HypothesesViewController.Instance.RiskOperationModel.PropertyChanged += RiskOperationModel_PropertyChanged;
            }
            updateRendering();
        }

        private void RecommenderHandleView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
                _model = null;
            }
            if (args.NewValue != null)
            {
                _model = (RecommenderHandleViewModel) args.NewValue;
                _model.PropertyChanged += _model_PropertyChanged;
                updatePercentage();
            }
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _model.GetPropertyName(() => _model.Percentage))
            {
                updatePercentage();
            }

            if (e.PropertyName == _model.GetPropertyName(() => _model.Position))
            {
                updatePosition();
            }

            _model.AttachmentViewModel.ActiveStopwatch.Restart();
        }

        private void updatePosition()
        {
            Vec diff = _model.Position - _model.StartPosition;
            var w = 1.0 - Math.Min(Math.Max(0.01, Math.Pow(Math.Abs(diff.X) / 600.0, 2)), 1.0);
            var y = Math.Min(Math.Max(0, Math.Pow(Math.Abs(diff.Y) / 300.0, 2)), 1.0) * w * Math.Sign(diff.Y) * 100.0;

            _model.Percentage = Math.Min(100, Math.Max(1, _model.StartPercentage - y));
            Debug.WriteLine(_model.Percentage);
        }

        private void updatePercentage()
        {
            lblPercentage.Text = _model.Percentage.ToString("F0") + "%";
        }

        private void RiskOperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as RiskOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.RiskControlType))
            {
                updateRendering();
            }
        }

        private void updateRendering()
        {
            if (HypothesesViewController.Instance != null && HypothesesViewController.Instance.RiskOperationModel != null)
            {
                if (HypothesesViewController.Instance.RiskOperationModel.RiskControlType == RiskControlType.PCER)
                {
                    defaultGrid.Visibility = Visibility.Visible;
                    alphaGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    defaultGrid.Visibility = Visibility.Collapsed;
                    alphaGrid.Visibility = Visibility.Visible;
                }
            }
        }

        private void HypothesesViewController_Initialized(object sender, EventArgs e)
        {
            HypothesesViewController.Initialized -= HypothesesViewController_Initialized;
            HypothesesViewController.Instance.RiskOperationModel.PropertyChanged += RiskOperationModel_PropertyChanged;
        }
    }
}