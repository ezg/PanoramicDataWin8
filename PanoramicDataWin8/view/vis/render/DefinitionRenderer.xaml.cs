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
        Dictionary<Color, string> _brushNames = new Dictionary<Color, string>();
        public DefinitionRenderer()
        {
            _brushNames.Add(Colors.Black, "All");
            this.InitializeComponent();
            this.DataContextChanged += DefinitionRenderer_DataContextChanged;
            this.SizeChanged += DefinitionRenderer_SizeChanged;
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
            foreach (var c in model.BrushColors)
            {
                createBrushLabel(model, numLabels, c);
            }
            createBrushLabel(model, numLabels,  Colors.Black);

        }

        private void createBrushLabel(DefinitionOperationModel model, int numLabels, Color c)
        {
            var rd = new RowDefinition();
            rd.Height = new GridLength(0.5, GridUnitType.Star);
            Labels.RowDefinitions.Add(rd);
            var panelHeight = (ActualHeight - 20 * numLabels) / numLabels;
            var panel = new Grid();
            if (model.BrushColors.Contains(c))
                Grid.SetRow(panel, model.BrushColors.IndexOf(c));
            else Grid.SetRow(panel, model.BrushColors.Count);
            panel.Height = panelHeight;
            panel.VerticalAlignment = VerticalAlignment.Stretch;
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            panel.Background = new SolidColorBrush(c);
            panel.Margin = new Thickness(10);

            var tb = new TextBox();
            tb.Tag = c;
            tb.TextChanged += Tb_TextChanged;
            tb.HorizontalAlignment = HorizontalAlignment.Stretch;
            tb.Margin = new Thickness(10, 4, 10, 4);
            tb.Foreground = new SolidColorBrush(Colors.White);
            tb.Background = new SolidColorBrush(c);
            tb.FontSize = panelHeight / 4;
            if (_brushNames.ContainsKey(c))
                tb.Text = _brushNames[c];
            tb.Margin = new Thickness(0);
            panel.Children.Add(tb);
            Labels.Children.Add(panel);
        }

        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var senderCol = (Color)((sender as TextBox).Tag);
            _brushNames[senderCol] = (sender as TextBox).Text;
  
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
