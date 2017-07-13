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
using GeoAPI.Geometries;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.model.data.operation;
using System.ComponentModel;
using Windows.UI;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.data.attribute;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8.view.vis.render
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalculationRenderer : Renderer, IScribbable
    {
        public CalculationRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += CalculationRenderer_DataContextChanged;
            this.SizeChanged += CalculationRenderer_SizeChanged;
            this.PointerExited += CalculationRenderer_PointerExited;
        }
        private void CalculationRenderer_OperationViewModelTapped(object sender, EventArgs e)
        {
            Labels.IsHitTestVisible = true;
        }

        private void CalculationRenderer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Labels.IsHitTestVisible = false;
        }
        private void CalculationRenderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Relayout();
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as CalculationOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as CalculationOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;

        }

        private void CalculationRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var model = ((DataContext as CalculationOperationViewModel).OperationModel as CalculationOperationModel);
            model.PropertyChanged -= OperationModel_PropertyChanged;
            model.OperationModelUpdated -= OperationModelUpdated;
            if (args.NewValue != null)
            {
                model.PropertyChanged += OperationModel_PropertyChanged;
                model.OperationModelUpdated += OperationModelUpdated;
            }
            (DataContext as CalculationOperationViewModel).OperationViewModelTapped += CalculationRenderer_OperationViewModelTapped;
        }


        void OperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Relayout();
        }
        void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            Relayout();
        }
        private SolidColorBrush _textBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"));
        private readonly SolidColorBrush _lightBrush = new SolidColorBrush(Helpers.GetColorFromString("#e6e6e6"));

        void Relayout()
        {

        }

        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((DataContext as CalculationOperationViewModel).OperationModel as CalculationOperationModel).Code = CodeBox.Text;
            foreach (var att in (this.DataContext as CalculationOperationViewModel).AttachementViewModels)
                if (att.MenuViewModel != null)
                    foreach (var menuItem in att.MenuViewModel.MenuItemViewModels)
                    {
                        var model = menuItem.MenuItemComponentViewModel as AttributeTransformationMenuItemViewModel;
                        if (model != null)
                        {
                            var attTransModel = (menuItem.MenuItemComponentViewModel as AttributeTransformationMenuItemViewModel).AttributeTransformationViewModel.OperationViewModel;

                            var attributeModel = new model.data.idea.IDEAAttributeComputedFieldModel(
                               model.AttributeTransformationViewModel.AttributeTransformationModel.AttributeModel.RawName,
                               model.AttributeTransformationViewModel.AttributeTransformationModel.AttributeModel.DisplayName,
                               CodeBox.Text == null ? "" : CodeBox.Text,// "C# Code For Boolean Field Goes Here",
                               IDEA_common.catalog.DataType.String,
                               "numeric",
                               new List<IDEA_common.catalog.VisualizationHint>());
                            var attr = new AttributeTransformationModel(attributeModel);
                            model.AttributeTransformationViewModel.AttributeTransformationModel = attr;
                        }
                    }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }

        public IGeometry Geometry
        {
            get
            {
                var model = this.DataContext as CalculationOperationViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public bool IsDeletable
        {
            get { return false; }
        }

        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }
    }
}
