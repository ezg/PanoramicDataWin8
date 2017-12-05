using GeoAPI.Geometries;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class FunctionRenderer : Renderer, IScribbable
    {
        DispatcherTimer _keyboardTimer = new DispatcherTimer();
        public FunctionRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += dataContextChanged;
            this.InitializeComponent();
        }

        public bool IsDeletable { get { return false; } }

        public FunctionOperationViewModel FunctionOperationViewModel => DataContext as FunctionOperationViewModel;

        public IGeometry Geometry => new Rct(FunctionOperationViewModel.Position, FunctionOperationViewModel.Size).GetPolygon();

        public List<IScribbable> Children => new List<IScribbable>();
        public bool Consume(InkStroke inkStroke) { return false; }

        void dataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            MinHeight = OperationViewModel.MIN_HEIGHT / 2;
            if (args.NewValue != null)
            {
                NameTextBox.Text = FunctionOperationViewModel.FunctionOperationModel.GetAttributeModel().DisplayName;
                FunctionOperationViewModel.OperationViewModelTapped -= OperationViewModelTapped;
                FunctionOperationViewModel.OperationViewModelTapped += OperationViewModelTapped;
            }
        }

        private void OperationViewModelTapped(PointerRoutedEventArgs e)
        {
            NameTextBox.IsEnabled = true;
            NameTextBox.Focus(FocusState.Keyboard);
        }


        private void NameTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {

                e.Handled = true;
            }
            else
                _keyboardTimer.Start();
        }

        private void NameTextBox_PointerExited(object sender, PointerRoutedEventArgs e)
        {

            if (!NameTextBox.GetBounds().Contains(e.GetCurrentPoint(NameTextBox).Position))
            {
                var model = (this.DataContext as FunctionOperationViewModel).OperationModel as FunctionOperationModel;
                NameTextBox.IsEnabled = false;
                model.SetRawName(NameTextBox.Text);
                MainViewController.Instance.MainPage.addAttributeButton.Focus(FocusState.Pointer);
                MainViewController.Instance.MainPage.clearAndDisposeMenus();
            }
        }
    }
}
