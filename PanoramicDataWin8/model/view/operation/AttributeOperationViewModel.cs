using IDEA_common.aggregates;
using IDEA_common.catalog;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.operation.computational;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using static PanoramicDataWin8.model.data.attribute.AttributeModel;
using static PanoramicDataWin8.view.vis.render.RawDataColumn;

namespace PanoramicDataWin8.model.view.operation
{
    public class AttributeOperationViewModel : LabeledOperationViewModel
    {
        MenuViewModel menuViewModel;
        List<MenuItemViewModel> types = new List<MenuItemViewModel>();
        List<MenuItemViewModel> hints = new List<MenuItemViewModel>();
        MenuItemViewModel createColumnOptionToggleMenuItem(MenuViewModel parentMenuViewModel, int row, int col, AttachmentOrientation orientation, string name, object hints=null)
        {
            var isChecked = (hints is DataType && this.AttributeOperationModel.GetAttributeModel().DataType == (DataType)hints) ||
                   (hints is VisualizationHint && this.AttributeOperationModel.GetAttributeModel().VisualizationHints.Contains((VisualizationHint)hints));
            var toggleMenuItem = new MenuItemViewModel
            {
                IsAlwaysDisplayed = false,
                MenuViewModel = parentMenuViewModel,
                Column = col,
                Row = row,
                RowSpan = 1,
                Position = this.Position,
                Size = new Vec(164, 20),
                TargetSize = new Vec(164, 20),
                Visible = Visibility.Collapsed,
                MenuItemComponentViewModel = new ToggleMenuItemComponentViewModel
                {
                    Label = name,
                    IsVisible = true,
                    IsChecked = isChecked,
                    Data = hints
                }
            };
            toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += toggleMenuItemChanged;
            return toggleMenuItem;
        }
        
        void toggleMenuItemChanged(object sender, PropertyChangedEventArgs args) { 
            var model = sender as ToggleMenuItemComponentViewModel; ;
            if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
            {
                var attrModel = AttributeOperationModel.GetAttributeModel();
                if (model.Data is VisualizationHint)
                {
                    if (model.IsChecked)
                        AttributeOperationModel.GetAttributeModel()?.VisualizationHints.Add((VisualizationHint)model.Data);
                    else
                        AttributeOperationModel.GetAttributeModel()?.VisualizationHints.Remove((VisualizationHint)model.Data);
                }
                else if (model.Data is DataType && model.IsChecked)
                {
                    foreach (var t in types.Select((t) => t.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).Where((m) => m != model))
                        t.IsChecked = false;
                    var funcMod = (attrModel.FuncModel as AttributeFuncModel.AttributeAssignedValueFuncModel);
                    funcMod.SetData(new AttributeFuncModel.AttributeAssignedValueFuncModel.AssignmentDictionary());
                    attrModel.DataType = (DataType)model.Data;
                    attrModel.SetCode(funcMod.ComputeCode(attrModel.DataType), attrModel.DataType, false);

                }
                AttributeOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            }
        }
        MenuViewModel CreateRightSideMenu(AttachmentViewModel attachmentViewModel)
        {
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 5,
                NrRows = 5,
            };

            int i = 0;
            foreach (var str in Enum.GetValues(typeof(VisualizationHint)).Cast<VisualizationHint>())
                hints.Add(createColumnOptionToggleMenuItem(menuViewModel, i++, 1, AttachmentOrientation.Right, str.ToString(), str));

            int j = 0;
            foreach (var str in Enum.GetValues(typeof(DataType)).Cast<DataType>())
                types.Add(createColumnOptionToggleMenuItem(menuViewModel, j++, 1, AttachmentOrientation.Right, str.ToString(), str));

            var TypesMenuItemViewModel = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                Column = 0,
                RowSpan = 1,
                ColumnSpan = 1,
                Size = new Vec(50, 25),
                TargetSize = new Vec(50, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    Label = "Types",
                    TappedTriggered = ((args) => {
                        foreach (var m in types)
                            m.Visible = m.Visible == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                        foreach (var m in hints)
                            m.Visible = Visibility.Collapsed;
                        menuViewModel.FireUpdate();
                        attachmentViewModel.StartDisplayActivationStopwatch();
                    })
                }
            };

            var HintsMenuItemViewModel = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 1,
                Column = 0,
                RowSpan = 1,
                ColumnSpan = 1,
                Size = new Vec(50, 25),
                TargetSize = new Vec(50, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    Label = "Hints",
                    TappedTriggered = ((args) => {
                        foreach (var m in types)
                            m.Visible = Visibility.Collapsed;
                        foreach (var m in hints)
                            m.Visible = m.Visible == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                        menuViewModel.FireUpdate();
                        attachmentViewModel.StartDisplayActivationStopwatch();
                    })
                }
            };

            menuViewModel.MenuItemViewModels.Add(TypesMenuItemViewModel);
            menuViewModel.MenuItemViewModels.Add(HintsMenuItemViewModel);
            for (int x = 2; x < 7; x++)
                menuViewModel.MenuItemViewModels.Add( new MenuItemViewModel  {
                    MenuViewModel = menuViewModel,
                    Row = x,
                    Column = 0,
                    RowSpan = 1,
                    ColumnSpan = 1,
                    Placeholding = true
                } );
            foreach (var h in hints)
                menuViewModel.MenuItemViewModels.Add(h);
            foreach (var t in types)
                menuViewModel.MenuItemViewModels.Add(t);
            attachmentViewModel.MenuViewModel = menuViewModel;
            return menuViewModel;
        }
        void AttributeUsageModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var mivm in menuViewModel.MenuItemViewModels.Where((mivm) => e.NewItems.Contains((mivm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel?.AttributeTransformationModel)))
                {
                    var amivm = (mivm.MenuItemComponentViewModel as AttributeMenuItemViewModel);
                    var avm = amivm.AttributeViewModel;
                    AttributeOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    avm.AttributeTransformationModel.AggregateFunction = AggregateFunction.None; // strip off any aggregate function when dropping onto a RawData View
                }
            }
            if (e.OldItems != null)
            {
                var funcMod = (AttributeOperationModel.FuncModel as AttributeFuncModel.AttributeAssignedValueFuncModel);
                var attrModel = AttributeOperationModel.GetAttributeModel();
                funcMod.SetData(null);
                attrModel.SetCode(funcMod.ComputeCode(attrModel.DataType), attrModel.DataType, false);
                AttributeOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            }
        }

        public AttributeOperationViewModel(AttributeOperationModel attributeGroupOperationModel) : base(attributeGroupOperationModel)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            var pmenu = CreateRightSideMenu(attachmentViewModel);

            MenuItemViewModel menuItemViewModel, AttributeNameMenuItemViewModel;
            menuViewModel = createExpandingMenu(AttachmentOrientation.TopStacked, AttributeOperationModel.AttributeTransformationModelParameters, "+", 50, 100, true, false, true, out menuItemViewModel);
            createAttributeLabelMenu(AttachmentOrientation.Bottom, AttributeOperationModel.GetAttributeModel(), AttributeUsage.X, new Vec(200, 50), 0, true, false, null, out AttributeNameMenuItemViewModel);
            AttributeOperationModel.AttributeTransformationModelParameters.CollectionChanged += AttributeUsageModels_CollectionChanged;
            AttributeOperationModel.AttributeTransformationModelParameters.Add(new AttributeTransformationModel(attributeGroupOperationModel.GetAttributeModel())); // this creates a menu item that we configure to be not deletable below.
            (menuViewModel.MenuItemViewModels.Last().MenuItemComponentViewModel as AttributeMenuItemViewModel).CanDelete = false;
        }
        
        public AttributeOperationModel AttributeOperationModel => (AttributeOperationModel)OperationModel;
    }
}
