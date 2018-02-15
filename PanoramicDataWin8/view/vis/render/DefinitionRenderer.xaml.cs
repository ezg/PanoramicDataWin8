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
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.idea;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PanoramicDataWin8.view.vis.render
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefinitionRenderer : Renderer, IScribbable
    {
        private SolidColorBrush _textBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"));
        private readonly SolidColorBrush _lightBrush = new SolidColorBrush(Helpers.GetColorFromString("#e6e6e6"));
        public DefinitionRenderer()
        {
            this.InitializeComponent();
            this.Loaded += DefinitionRenderer_Loaded;
            this.Unloaded += DefinitionRenderer_Unloaded;
        }

        private void DefinitionRenderer_Unloaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged -= DefinitionRenderer_DataContextChanged;
        }

        private void DefinitionRenderer_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContextChanged += DefinitionRenderer_DataContextChanged;
            configureDataContext();
        }
        
        private void DefinitionRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            configureDataContext();
            args.Handled = true;
        }
        void configureDataContext()
        {
            var viewModel = (DataContext as DefinitionOperationViewModel);
            if (viewModel != null)
            {
                if (viewModel.DefinitionOperationModel?.GetDescriptorFromColor(_textBrush.Color) == null)
                    viewModel.DefinitionOperationModel.SetDescriptorForColor(_textBrush.Color, new BrushDescriptor(_textBrush.Color, "Rest"));
                if (viewModel.DefinitionOperationModel?.GetDescriptorFromColor(Colors.Black) == null)
                    viewModel.DefinitionOperationModel.SetDescriptorForColor(Colors.Black, new BrushDescriptor(Colors.Black, "Overlap"));
                viewModel.DefinitionOperationModel.OperationModelUpdated -= RelayoutHandler;
                viewModel.DefinitionOperationModel.OperationModelUpdated += RelayoutHandler;
                Relayout();
            }
        }

        public override void StartSelection(Point point)
        {
            foreach (var label in Labels.Children.Select((c) => c as DefinitionLabel))
            {
                var bounds = label.GetBoundingRect(MainViewController.Instance.InkableScene);
                if (bounds.Contains(point))
                    label.Activate();
            }
        }

        public DefinitionOperationModel  DefinitionOperationModel { get => (DataContext as DefinitionOperationViewModel)?.DefinitionOperationModel; }

        void RelayoutHandler(object sender, OperationModelUpdatedEventArgs e) { Relayout(); }

        public override void Refactor(string oldName, string newName) { DefinitionOperationModel.UpdateCode(true); }
        void Relayout()
        {
            var model = DefinitionOperationModel;
            if (model == null)
            {
                Debug.WriteLine("WHY is model null???");
                return;
            }
            Labels.Children.Clear();
            Labels.RowDefinitions.Clear();
            int numLabels = model.BrushColors.Count + 1 + (model.BrushColors.Count > 1 ? 1 : 0);
            foreach (var c in model.BrushColors)
            {
                createBrushLabel(model, numLabels, model.GetDescriptorFromColor(c, "<name>"), model.BrushColors.IndexOf(c));
            }
            if (model.BrushColors.Count > 1)
                createBrushLabel(model, numLabels, model.GetDescriptorFromColor(Colors.Black), model.BrushColors.Count);
            createBrushLabel(model, numLabels, model.GetDescriptorFromColor(_textBrush.Color), model.BrushColors.Count + 1);
            model.UpdateCode();
        }
        private void createBrushLabel(DefinitionOperationModel model, int numLabels, BrushDescriptor d, int rowIndex)
        {
            var rd = new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) };
            Labels.RowDefinitions.Add(rd);

            var textGrid = new DefinitionLabel() {
                DataContext = d,
                DefinitionOperationModel = model,
                Height = double.NaN, // FontSize = (ActualHeight - 5 * numLabels) / numLabels
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetRow(textGrid, rowIndex);
            Labels.Children.Add(textGrid);
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
