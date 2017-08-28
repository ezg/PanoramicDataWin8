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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8.view.vis.render
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefinitionRenderer : Renderer, IScribbable
    {
        public DefinitionRenderer()
        {
            this.InitializeComponent();
            this.DataContextChanged += DefinitionRenderer_DataContextChanged;
            this.SizeChanged += DefinitionRenderer_SizeChanged;
            this.PointerExited += DefinitionRenderer_PointerExited;
            this.Tapped += DefinitionRenderer_Tapped;
        }
        
        private void DefinitionRenderer_OperationViewModelTapped(object sender, EventArgs e)
        {
            Labels.IsHitTestVisible = true;
        }

        private void DefinitionRenderer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Labels.IsHitTestVisible = false;
        }
        private void DefinitionRenderer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Relayout();
        }

        public override void Dispose()
        {
            base.Dispose();
            (DataContext as DefinitionOperationViewModel).OperationModel.OperationModelUpdated -= OperationModelUpdated;
            (DataContext as DefinitionOperationViewModel).OperationModel.PropertyChanged -= OperationModel_PropertyChanged;
           
        }

        private void DefinitionRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var model = ((DataContext as DefinitionOperationViewModel).OperationModel as DefinitionOperationModel);

            if (model.GetDescriptorFromColor(_textBrush.Color) == null)
                model.SetDescriptorForColor(_textBrush.Color, new BrushDescriptor(_textBrush.Color, "Rest"));
            if (model.GetDescriptorFromColor(Colors.Black) == null)
                model.SetDescriptorForColor(Colors.Black,     new BrushDescriptor(Colors.Black, "Overlap"));
            model.PropertyChanged -= OperationModel_PropertyChanged;
            model.OperationModelUpdated -= OperationModelUpdated;
            if (args.NewValue != null)
            {
                model.PropertyChanged += OperationModel_PropertyChanged;
                model.OperationModelUpdated += OperationModelUpdated;
            }
            (DataContext as DefinitionOperationViewModel).OperationViewModelTapped += DefinitionRenderer_OperationViewModelTapped;
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
            var model = ((DataContext as DefinitionOperationViewModel).OperationModel as DefinitionOperationModel);
            Labels.Children.Clear();
            Labels.RowDefinitions.Clear();
            int numLabels = model.BrushColors.Count + 1 + (model.BrushColors.Count > 1 ? 1 : 0);
            foreach (var c in model.BrushColors)
            {
                if (model.GetDescriptorFromColor(c) == null)
                    model.SetDescriptorForColor(c, new BrushDescriptor(c, "<name>"));
                createBrushLabel(model, numLabels, model.GetDescriptorFromColor(c), model.BrushColors.IndexOf(c));
            }
            if (model.BrushColors.Count > 1)
                createBrushLabel(model, numLabels, model.GetDescriptorFromColor(Colors.Black), model.BrushColors.Count);
            createBrushLabel(model, numLabels, model.GetDescriptorFromColor(_textBrush.Color), model.BrushColors.Count + 1);
            model.UpdateCode();
        }
        private void createBrushLabel(DefinitionOperationModel model, int numLabels, BrushDescriptor d, int rowIndex)
        {
            var rd = new RowDefinition() { Height = new GridLength(0.5, GridUnitType.Star) };
            Labels.RowDefinitions.Add(rd);

            var textGrid = new DefinitionLabel() { DataContext = d, DefinitionOperationModel = model };
            textGrid.Background = new SolidColorBrush(d.Color);
            textGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            textGrid.Height = (ActualHeight - 5 * numLabels) / numLabels;

            Grid.SetRow(textGrid, rowIndex);
            Labels.Children.Add(textGrid);
        }

        private void DefinitionRenderer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var elementStack =
              VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), null);
            foreach (var label in elementStack.Where((el) => el is DefinitionLabel).Select((vb) => vb as DefinitionLabel))
                label.Activate();
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }

        public IGeometry Geometry
        {
            get
            {
                var model = this.DataContext as DefinitionOperationViewModel;

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
