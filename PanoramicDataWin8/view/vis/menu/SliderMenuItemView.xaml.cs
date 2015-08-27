using PanoramicDataWin8.model.view;
using PanoramicDataWin8.utils;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PanoramicDataWin8.view.vis.menu
{
    public sealed partial class SliderMenuItemView : UserControl
    {
        private PointerManager _mainPointerManager = new PointerManager();
        private Point _mainPointerManagerPreviousPoint = new Point();

        public SliderMenuItemView()
        {
            this.InitializeComponent();
            this.DataContextChanged += SliderMenuItemView_DataContextChanged;

            _mainPointerManager.Added += mainPointerManager_Added;
            _mainPointerManager.Moved += mainPointerManager_Moved;
            _mainPointerManager.Removed += mainPointerManager_Removed;
            _mainPointerManager.Attach(this);
        }

        void SliderMenuItemView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var d = DataContext;
            if (args.NewValue != null)
            {
                var model = (args.NewValue as MenuItemViewModel).MenuItemComponentViewModel as SliderMenuItemComponentViewModel;
                model.PropertyChanged += MenuItemComponentViewModel_PropertyChanged;
                updateRendering();
                
            }
        }

        void MenuItemComponentViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            updateRendering();
        }
        
        void updateRendering()
        {
            if (DataContext != null)
            {
                var menuItemModel = (DataContext as MenuItemViewModel);
                var sliderModel = menuItemModel.MenuItemComponentViewModel as SliderMenuItemComponentViewModel;

                txtBlock.Text = sliderModel.Label + " : " + sliderModel.Value;

                double w = ((sliderModel.Value - sliderModel.MinValue) / (sliderModel.MaxValue - sliderModel.MinValue)) * menuItemModel.TargetSize.X;
                rct.Width = Math.Max(1, w);
            }
        }

        void mainPointerManager_Added(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                var menuItemModel = (DataContext as MenuItemViewModel);
                var sliderModel = menuItemModel.MenuItemComponentViewModel as SliderMenuItemComponentViewModel;

                Pt pos = e.CurrentContacts[e.TriggeringPointer.PointerId].Position;
                double x = Math.Min(Math.Max(pos.X, 0), menuItemModel.TargetSize.X) / menuItemModel.TargetSize.X;
                sliderModel.Value = Math.Round(x * (sliderModel.MaxValue - sliderModel.MinValue) + sliderModel.MinValue);
                //updateRendering();
            }
        }

        void mainPointerManager_Moved(object sender, PointerManagerEvent e)
        {
            if (e.NumActiveContacts == 1)
            {
                var menuItemModel = (DataContext as MenuItemViewModel);
                var sliderModel = menuItemModel.MenuItemComponentViewModel as SliderMenuItemComponentViewModel;

                Pt pos = e.CurrentContacts[e.TriggeringPointer.PointerId].Position;
                double x = Math.Min(Math.Max(pos.X, 0), menuItemModel.TargetSize.X) / menuItemModel.TargetSize.X;
                sliderModel.Value = Math.Round(x * (sliderModel.MaxValue - sliderModel.MinValue) + sliderModel.MinValue);
                //updateRendering();
            }
        }

        void mainPointerManager_Removed(object sender, PointerManagerEvent e)
        {
            var menuItemModel = (DataContext as MenuItemViewModel);
            var sliderModel = menuItemModel.MenuItemComponentViewModel as SliderMenuItemComponentViewModel;
            sliderModel.FinalValue = sliderModel.Value;
        }
    }
}
