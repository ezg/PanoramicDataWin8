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
            model.PropertyChanged -= OperationModel_PropertyChanged;
            model.OperationModelUpdated -= OperationModelUpdated;
            if (args.NewValue != null)
            {
                model.PropertyChanged += OperationModel_PropertyChanged;
                model.OperationModelUpdated += OperationModelUpdated;
            }
        }
        
        void OperationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Relayout();
        }
        void OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            Relayout();
        }
        private readonly SolidColorBrush _lightBrush = new SolidColorBrush(Helpers.GetColorFromString("#e6e6e6"));

        void Relayout()
        {
            var model = ((DataContext as DefinitionOperationViewModel).OperationModel as DefinitionOperationModel);
            Labels.Children.Clear();
            Labels.RowDefinitions.Clear();
            int numLabels = model.BrushColors.Count + 1;
            var panel = new Grid();
            var tb = new TextBox();
            var rd = new RowDefinition();
            foreach (var c in model.BrushColors)
            {
                rd = new RowDefinition();
                rd.Height = new GridLength(0.5, GridUnitType.Star);
                Labels.RowDefinitions.Add(rd);
                panel = new Grid();
                Grid.SetRow(panel, model.BrushColors.IndexOf(c));
                panel.Height = (ActualHeight - 20 * numLabels) / numLabels;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
                panel.HorizontalAlignment = HorizontalAlignment.Stretch;
                panel.Background = new SolidColorBrush(c);
                panel.Margin = new Thickness(10);

                tb = new TextBox();
                tb.HorizontalAlignment = HorizontalAlignment.Stretch;
                tb.Margin = new Thickness(10, 4, 10, 4);
                tb.Foreground = new SolidColorBrush(Colors.White);
                panel.Children.Add(tb);
                Labels.Children.Add(panel);
            }
            rd = new RowDefinition();
            rd.Height = new GridLength(0.5, GridUnitType.Star);
            Labels.RowDefinitions.Add(rd);
            panel = new Grid();
            Grid.SetRow(panel, model.BrushColors.Count);
            panel.Height = (ActualHeight - 20 * numLabels) / numLabels;
            panel.VerticalAlignment = VerticalAlignment.Stretch;
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            panel.Background = new SolidColorBrush(Colors.Black);
            panel.Margin = new Thickness(10);

            tb = new TextBox();
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.Foreground = new SolidColorBrush(Colors.White) ;
           
            tb.Margin = new Thickness(10, 4, 10, 4);
            panel.Children.Add(tb);
            Labels.Children.Add(panel);
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
