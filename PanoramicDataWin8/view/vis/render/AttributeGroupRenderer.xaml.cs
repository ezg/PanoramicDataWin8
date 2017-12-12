using GeoAPI.Geometries;
using IDEA_common.catalog;
using IDEA_common.operations.recommender;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using InkStroke = PanoramicDataWin8.view.inq.InkStroke;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.render
{
    public sealed partial class AttributeGroupRenderer : Renderer, IScribbable
    {
        DispatcherTimer _keyboardTimer = new DispatcherTimer();
        public AttributeGroupRenderer()
        {
            this.DataContextChanged += dataContextChanged;
            this.InitializeComponent();
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public IGeometry Geometry
        {
            get
            {
                var model = this.DataContext as AttributeGroupOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }
        void dataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            MinHeight = OperationViewModel.MIN_HEIGHT / 2;
            if (args.NewValue != null)
            {
                var attributeGroupOperationViewModel = DataContext as AttributeGroupOperationViewModel;
                NameTextBox.Text = attributeGroupOperationViewModel.AttributeGroupOperationModel.AttributeModel.DisplayName ?? "";
                attributeGroupOperationViewModel.OperationModel.OperationModelUpdated -= OperationModelUpdated;
                attributeGroupOperationViewModel.OperationModel.OperationModelUpdated += OperationModelUpdated;
                attributeGroupOperationViewModel.OperationModel.PropertyChanged -= OperationModelPropertyChanged;
                attributeGroupOperationViewModel.OperationModel.PropertyChanged += OperationModelPropertyChanged;

                attributeGroupOperationViewModel.OperationViewModelTapped += AttributeGroupRenderer_OperationViewModelTapped;
                MainViewController.Instance.MainPage.clearAndDisposeMenus();
            }
        }

        private void AttributeGroupRenderer_OperationViewModelTapped(PointerRoutedEventArgs e)
        {
            var attributeGroupOperationViewModel = DataContext as AttributeGroupOperationViewModel;

            NameTextBox.IsEnabled = true && attributeGroupOperationViewModel.Editable;
            NameTextBox.Focus(FocusState.Keyboard);
        }

        private void OperationModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var attributeGroupOperationViewModel = DataContext as AttributeGroupOperationViewModel;
            NameTextBox.Text = attributeGroupOperationViewModel.AttributeGroupOperationModel.AttributeModel.DisplayName ?? "";
        }

        private void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
        }

        public List<IScribbable> Children {
            get { return new List<IScribbable>(); }
        }
        public bool Consume(InkStroke inkStroke)
        {
            return false;
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

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

            var model = (this.DataContext as AttributeGroupOperationViewModel).OperationModel as AttributeGroupOperationModel;
            NameTextBox.IsEnabled = false;
            model.SetName(NameTextBox.Text);
            MainViewController.Instance.MainPage.addAttributeButton.Focus(FocusState.Pointer);
            MainViewController.Instance.MainPage.clearAndDisposeMenus();
        }
    }
}
