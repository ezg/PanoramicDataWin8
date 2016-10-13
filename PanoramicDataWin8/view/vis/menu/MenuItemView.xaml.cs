using PanoramicDataWin8.model.view;
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
using PanoramicDataWin8.view.inq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class MenuItemView : UserControl, IScribbable
    {
        public MenuItemView()
        {
            this.InitializeComponent();
            DataContextChanged += MenuItemView_DataContextChanged;
            this.PointerPressed += MenuItemView_PointerPressed;
        }

        private void MenuItemView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            
        }

        void MenuItemView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (args.NewValue != null)
            {
                ((MenuItemViewModel) args.NewValue).PropertyChanged -= MenuItemView_PropertyChanged;
                ((MenuItemViewModel) args.NewValue).PropertyChanged += MenuItemView_PropertyChanged;
                updateRendering();
            }
        }

        void MenuItemView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MenuItemViewModel model = ((MenuItemViewModel) DataContext);
            if (e.PropertyName == model.GetPropertyName(() => model.MenuItemComponentViewModel))
            {
                updateRendering();
            }
        }

        private void updateRendering()
        {
            MenuItemViewModel model = ((MenuItemViewModel) DataContext);
            if (model.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel)
            {
                mainGrid.Children.Clear();
                mainGrid.Children.Add(new ToggleMenuItemView());
            }
            else if (model.MenuItemComponentViewModel is SliderMenuItemComponentViewModel)
            {
                mainGrid.Children.Clear();
                mainGrid.Children.Add(new SliderMenuItemView());
            }
            else if (model.MenuItemComponentViewModel is AttributeTransformationMenuItemViewModel)
            {
                mainGrid.Children.Clear();
                mainGrid.Children.Add(new AttributeTransformationMenuItemView());
            }
        }


        public bool IsDeletable { get { return true; } }

        public IGeometry Geometry
        {
            get
            {
                MenuItemViewModel model = this.DataContext as MenuItemViewModel;

                Rct bounds = new Rct(model.Position, model.Size);
                return bounds.GetPolygon();
            }
        }

        public List<IScribbable> Children
        {
            get { return new List<IScribbable>(); }
        }
        public bool Consume(InkStroke inkStroke)
        {
            return false;
        }
    }
}
