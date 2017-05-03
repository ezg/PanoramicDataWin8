using System;
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
using IDEA_common.operations;
using IDEA_common.operations.recommender;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.common;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis.render;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class RecommendedHistogramMenuItemView : UserControl
    {
        private PlotRendererContentProvider _plotRendererContentProvider = new PlotRendererContentProvider();
        private RecommendedHistogramMenuItemViewModel _model = null;
        public RecommendedHistogramMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += PlotRenderer_DataContextChanged;
            this.Loaded += RecommendedHistogram_Loaded;
        }

        void RecommendedHistogram_Loaded(object sender, RoutedEventArgs e)
        {
            _plotRendererContentProvider.CompositionScaleX = dxSurface.CompositionScaleX;
            _plotRendererContentProvider.CompositionScaleY = dxSurface.CompositionScaleY;
            dxSurface.ContentProvider = _plotRendererContentProvider;
        }
        
        void PlotRenderer_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (_model != null)
            {
                _model.PropertyChanged -= RecommendedHistogramMenuItemViewModel_PropertyChanged;
            }
            if (args.NewValue != null)
            {
                _model = (DataContext as MenuItemViewModel).MenuItemComponentViewModel as RecommendedHistogramMenuItemViewModel;
                _model.PropertyChanged += RecommendedHistogramMenuItemViewModel_PropertyChanged;
                
                if (_model.RecommendedHistogram.HistogramResult != null)
                {
                    loadResult(_model.RecommendedHistogram.HistogramResult);
                    render();
                }
            }
        }

        void loadResult(IResult result)
        {
            //_plotRendererContentProvider.UpdateData(result);
        }


        void render(bool sizeChanged = false)
        {
            dxSurface?.Redraw();
        }

        void RecommendedHistogramMenuItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            HistogramOperationModel operationModel = (HistogramOperationModel)((OperationViewModel)DataContext).OperationModel;
            if (e.PropertyName == operationModel.GetPropertyName(() => operationModel.Result))
            {
                var result = (DataContext as HistogramOperationViewModel).OperationModel.Result;
                if (result != null)
                {
                    loadResult(result);
                    render();
                }
            }
        }

    }
}
