using IDEA_common.catalog;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media;
using IDEA_common.aggregates;

namespace PanoramicDataWin8.model.view.operation
{
    public class RawDataOperationViewModel : AttributeUsageOperationViewModel
    {
        private void createAxisMenu(AttachmentOrientation attachmentOrientation,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 5,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = Position,
                TargetSize = size,
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent
            };
            var attr1 = new AttributeMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"))
            };
            RawDataOperationModel.GetAttributeUsageTransformationModel(axis).CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                var attributeTransformationModel = coll.FirstOrDefault();
                attr1.Label = attributeTransformationModel == null ? "" : attributeTransformationModel.GetLabel();

                if (attributeTransformationModel != null)
                {
                    attr1.AttributeViewModel = new AttributeViewModel(this, coll.FirstOrDefault().AttributeModel);
                    attributeTransformationModel.PropertyChanged += (sender2, args2) => attr1.Label = (sender2 as AttributeTransformationModel).GetLabel();
                    attributeTransformationModel.AttributeModel.PropertyChanged += (sender2, arg2) => attr1.Label = attributeTransformationModel.GetLabel();
                }
                else
                    attr1.AttributeViewModel = null;

                // remove old ones first
                foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                    menuViewModel.MenuItemViewModels.Remove(mvm);
            };
            attr1.TappedTriggered = () => { attachmentViewModel.ActiveStopwatch.Restart(); };
            attr1.DroppedTriggered = attributeViewModel =>
            {
                var attributeModel = (attributeViewModel is AttributeViewModel) ? (attributeViewModel as AttributeViewModel).AttributeModel :
                                     (attributeViewModel as AttributeTransformationViewModel)?.AttributeTransformationModel.AttributeModel;
                if (attributeModel.DataType == DataType.Undefined)
                    return;

                var existingModel = RawDataOperationModel.GetAttributeUsageTransformationModel(axis).Any() ?
                      RawDataOperationModel.GetAttributeUsageTransformationModel(axis).First() : null;
                if (existingModel != null)
                    RawDataOperationModel.RemoveAttributeUsageTransformationModel(axis, existingModel);

                var attributeTransformationModel = new AttributeTransformationModel(attributeModel) { AggregateFunction = AggregateFunction.None };
                if (!RawDataOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                {
                    RawDataOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, attributeTransformationModel);
                }
                RawDataOperationModel.AddAttributeUsageTransformationModel(axis, attributeTransformationModel);
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);
        }

        public RawDataOperationViewModel(RawDataOperationModel rawDataOperationModel, AttributeModel attributeModel) : base(rawDataOperationModel)
        {
            addAttachmentViewModels();

            // axis attachment view models
            createAxisMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            createTopRightFilterDragMenu();
            createTopInputsExpandingMenu(8);

            if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.ENUM ||
                attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.CATEGORY ||
                attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
            {
                var x = new AttributeTransformationModel(attributeModel) { AggregateFunction = AggregateFunction.None };
                rawDataOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
            }
            else if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
            {
            }
            else if (attributeModel?.InputVisualizationType == InputVisualizationTypeConstants.VECTOR)
            {
            }
            else
            {
            }
        }

        public RawDataOperationModel RawDataOperationModel => (RawDataOperationModel)OperationModel;
        
    }
}