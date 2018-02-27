using IDEA_common.aggregates;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.operation.computational;
using PanoramicDataWin8.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        MenuItemViewModel AttributeNameMenuItemViewModel;
        MenuItemViewModel menuItemViewModel;
        MenuViewModel menuViewModel;
        public AttributeOperationViewModel(AttributeOperationModel attributeGroupOperationModel, bool editable) : base(attributeGroupOperationModel)
        {
            Editable = editable;
            //var attributeParameterGroups = new Dictionary<string, ObservableCollection<AttributeTransformationModel>>();
            //attributeParameterGroups.Add("Hints", new ObservableCollection<AttributeTransformationModel>());
            //attributeParameterGroups.Add("DType", new ObservableCollection<AttributeTransformationModel>());
            //createExpandingMenu(AttachmentOrientation.TopStacked, attributeParameterGroups, 50, 100, !Editable, false, Editable, out menuItemViewModel);
            
            menuViewModel = createExpandingMenu(AttachmentOrientation.TopStacked, AttributeOperationModel.AttributeTransformationModelParameters, "+", 50, 100, false, true, true, out menuItemViewModel);
            createAttributeLabelMenu(AttachmentOrientation.Bottom, AttributeOperationModel.GetAttributeModel(), AttributeUsage.X, new Vec(200, 50), 0, true, false, null, out AttributeNameMenuItemViewModel);
            AttributeOperationModel.AttributeTransformationModelParameters.CollectionChanged += AttributeUsageModels_CollectionChanged;
            AttributeOperationModel.AttributeTransformationModelParameters.Add(new AttributeTransformationModel(attributeGroupOperationModel.GetAttributeModel()));
        }

        public bool Editable = true;
        public void ForceDrop(AttributeViewModel am)
        {
            (menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel).DroppedTriggered(am);
        }
        private void AttributeUsageModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var mivm in menuViewModel.MenuItemViewModels.Where((mivm) => e.NewItems.Contains((mivm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel?.AttributeTransformationModel)))
                {
                    var amivm = (mivm.MenuItemComponentViewModel as AttributeMenuItemViewModel);
                    var avm = amivm.AttributeViewModel;
                    AttributeOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                    avm.AttributeTransformationModel.AggregateFunction = AggregateFunction.None; // strip off any aggregate function when dropping onto a RawData View

                    amivm.TappedTriggered = ((args) =>
                    {
                        if (!args.IsRightMouse)
                        {
                            AttachementViewModels.First(atvm => atvm.AttachmentOrientation == AttachmentOrientation.TopStacked).StartDisplayActivationStopwatch();

                            foreach (var toggle in menuViewModel.MenuItemViewModels.Where((t) => t.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).Select((t) => t.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel))
                                toggle.IsVisible = !toggle.IsVisible;
                        }
                    });
                }
            }
            if (e.OldItems != null)
            {
                AttributeOperationModel.FuncModel.SetData(null);
                AttributeOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
            }
        }

        public AttributeOperationModel AttributeOperationModel => (AttributeOperationModel)OperationModel;
    }
}
