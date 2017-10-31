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
using Windows.UI.Xaml.Navigation;
using IDEA_common.operations.risk;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.common
{
    public sealed partial class BudgetView : UserControl
    {
        private BudgetViewModel _model = null;

        public BudgetView()
        {
            this.InitializeComponent();
            this.Loaded += BudgetView_Loaded;
        }

        private void BudgetView_Loaded(object sender, RoutedEventArgs e)
        {
            if (HypothesesViewController.Instance == null)
            {
                HypothesesViewController.Initialized += HypothesesViewController_Initialized;
            }
            else
            {
                HypothesesViewController.Instance.HypothesesViewModel.PropertyChanged += HypothesesViewModel_PropertyChanged;
                HypothesesViewController.Instance.RiskOperationModel.PropertyChanged += RiskOperationModel_PropertyChanged;
            }
            updateRendering();

            DataContextChanged += BudgetView_DataContextChanged;
        }

        private void BudgetView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= _model_PropertyChanged;
            }
            if (args.NewValue != null)
            {
                _model = args.NewValue as BudgetViewModel;
                _model.PropertyChanged += _model_PropertyChanged;
            }
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }

        private void RiskOperationModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as RiskOperationModel;
            if (e.PropertyName == model.GetPropertyName(() => model.RiskControlType))
            {
                updateRendering();
            }
        }

        private void HypothesesViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as HypothesesViewModel;
            if (e.PropertyName == model.GetPropertyName(() => model.Wealth) ||
                e.PropertyName == model.GetPropertyName(() => model.StartWealth))
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
                    if (_model != null)
                    {
                        defaultGridTB.Text = _model.DefaultLabel;
                    }
                }
                else
                {
                    defaultGrid.Visibility = Visibility.Collapsed;
                    alphaGrid.Visibility = Visibility.Visible;

                    if (HypothesesViewController.Instance.HypothesesViewModel.Wealth != -1 &&
                        HypothesesViewController.Instance.HypothesesViewModel.StartWealth != -1)
                    {
                        var total = Math.Max(HypothesesViewController.Instance.HypothesesViewModel.StartWealth, HypothesesViewController.Instance.HypothesesViewModel.Wealth);
                        var percentage = HypothesesViewController.Instance.HypothesesViewModel.Wealth / total;
                        tbPercentage.Text = (percentage * 100.0).ToString("F0") + "%";

                        var percentageToSpend = (DataContext as BudgetViewModel) != null ? (DataContext as BudgetViewModel).BudgetToSpend : 0;

                        bottom.Height = new GridLength(Math.Max(percentage - percentageToSpend, 0), GridUnitType.Star);
                        middle.Height = new GridLength(percentageToSpend, GridUnitType.Star);
                        top.Height = new GridLength((total / total) - percentage, GridUnitType.Star);
                    }
                }
            }
        }

        private void HypothesesViewController_Initialized(object sender, EventArgs e)
        {
            HypothesesViewController.Initialized -= HypothesesViewController_Initialized;
            HypothesesViewController.Instance.HypothesesViewModel.PropertyChanged += HypothesesViewModel_PropertyChanged;
            HypothesesViewController.Instance.RiskOperationModel.PropertyChanged += RiskOperationModel_PropertyChanged;
        }
    }
}
