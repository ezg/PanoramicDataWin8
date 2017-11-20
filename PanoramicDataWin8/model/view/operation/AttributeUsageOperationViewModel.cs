using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml.Media;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.model.data.attribute;
using System.Collections.Generic;
using Windows.UI.Xaml.Input;
using PanoramicDataWin8.model.data.idea;
using Windows.UI.Xaml;
using PanoramicDataWin8.controller.view;

namespace PanoramicDataWin8.model.view.operation
{
    public class AttributeUsageOperationViewModel : OperationViewModel
    {
        public AttributeUsageOperationViewModel(OperationModel operationModel) : base(operationModel)
        {
            addAttachmentViewModels();
        }

        static List<AttributeTransformationModel> findAllNestedGroups(AttributeTransformationModel attributeGroupModel)
        {
            var models = new List<AttributeTransformationModel>();
            if (attributeGroupModel != null)
            {
                models.Add(attributeGroupModel);
                var inputModels = (attributeGroupModel.AttributeModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeGroupFuncModel)?.InputModels;
                if (inputModels != null)
                    foreach (var at in inputModels)
                        models.AddRange(findAllNestedGroups(new AttributeTransformationModel(at)));
            }
            return models;
        }
        static public bool attributeModelContainsAttributeModel(AttributeTransformationModel testAttributeModel, AttributeTransformationModel newAttributeModel)
        {
            return findAllNestedGroups(newAttributeModel).Contains(testAttributeModel);
        }
        public delegate void InputAddedHandler(object sender, ObservableCollection<AttributeTransformationModel> usageModels);
        public event         InputAddedHandler ExpandingMenuInputAdded;
        public delegate void DroppedTriggeredHandler(AttributeViewModel model);
        protected MenuViewModel createExpandingMenu(AttachmentOrientation orientation, ObservableCollection<AttributeTransformationModel> operationAttributeModels, 
            string label,  int maxExpansionSlots, out MenuItemViewModel menuItemViewModel)
        {
            var swapOrientation = orientation == AttachmentOrientation.Left || orientation == AttachmentOrientation.Right;
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == orientation);
            attachmentViewModel.ShowOnAttributeMove = true;
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = swapOrientation ? 1 : maxExpansionSlots,
                NrRows = swapOrientation ? maxExpansionSlots : 1
            };

            menuItemViewModel = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Size = new Vec(25, 25),
                TargetSize = new Vec(25, 25),
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    Label = label,
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                    CanDrag = false,
                    CanDrop = true,
                    CanDelete = true,
                    DroppedTriggered = attributeViewModel =>
                    {
                        var attributeModel = attributeViewModel.AttributeTransformationModel;
                        if (!operationAttributeModels.Contains(attributeModel) &&
                            !attributeModelContainsAttributeModel(new AttributeTransformationModel((OperationModel as AttributeGroupOperationModel)?.AttributeModel), attributeModel))
                        {
                            operationAttributeModels.Add(attributeModel);
                            if (ExpandingMenuInputAdded != null)
                                ExpandingMenuInputAdded(this, operationAttributeModels);
                        }
                    }
                }
            };

            if (!MainViewController.Instance.MainModel.IsDarpaSubmissionMode)
            {
                attachmentViewModel.MenuViewModel = menuViewModel;
                menuViewModel.MenuItemViewModels.Add(menuItemViewModel);

                var menuItemViewModelCaptured = menuItemViewModel;
                operationAttributeModels.CollectionChanged += (sender, args) =>
                {
                    var coll = sender as ObservableCollection<AttributeTransformationModel>;
                    var attributeModel = coll.FirstOrDefault();

                    // remove old ones first
                    if (args.OldItems != null)
                        foreach (var oldItem in args.OldItems)
                        {
                            var oldAttributeTransformationModel = oldItem as AttributeTransformationModel;
                            var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                                ((mvm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel != null) &&
                                ((mvm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel?.AttributeTransformationModel ==
                                 oldAttributeTransformationModel));
                            if (found != null)
                                menuViewModel.MenuItemViewModels.Remove(found);
                        }

                    if (swapOrientation)
                         menuViewModel.NrColumns = (int)Math.Ceiling(1.0 * coll.Count / maxExpansionSlots) + 1;
                    else menuViewModel.NrRows = (int)Math.Ceiling(1.0 * coll.Count / maxExpansionSlots) + 1;

                    // add new ones
                    if (args.NewItems != null)
                    {
                        foreach (var newItem in args.NewItems)
                        {
                            var newAttributeTransformationModel = newItem as AttributeTransformationModel;
                            var newMenuItem = new MenuItemViewModel
                            {
                                MenuViewModel = menuViewModel,
                                Size = new Vec(50, 50),
                                TargetSize = new Vec(50, 50),
                                Position = menuItemViewModelCaptured.Position,
                                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                                {
                                    Label = newAttributeTransformationModel.GetLabel,
                                    AttributeViewModel = new AttributeViewModel(this, newAttributeTransformationModel),
                                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                                    CanDrag = true,
                                    CanDelete = true,
                                    CanDrop = false
                                }
                            };
                         
                            newMenuItem.Deleted += (sender1, args1) =>
                            {
                                var atm = ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeTransformationModel;
                                operationAttributeModels.Remove(atm);
                            };
                            menuViewModel.MenuItemViewModels.Add(newMenuItem);
                            
                            if (newAttributeTransformationModel != null)
                            {
                                newAttributeTransformationModel.PropertyChanged += (sender2, args2) => 
                                    (newMenuItem.MenuItemComponentViewModel as AttributeMenuItemViewModel).Label = (sender2 as AttributeTransformationModel).GetLabel;
                            }
                        }
                    }

                    var count = 0;
                    foreach (var mItemViewModel in menuViewModel.MenuItemViewModels.Where(mvm => mvm != menuItemViewModelCaptured).Where((mivm) => !(mivm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel)))
                    {
                        mItemViewModel.Row    = swapOrientation ? count % maxExpansionSlots : menuViewModel.NrRows - 1 - (int)Math.Floor(1.0 * count / maxExpansionSlots);
                        mItemViewModel.Column = swapOrientation ? menuViewModel.NrColumns - 1 - (int)Math.Floor(1.0 * count / maxExpansionSlots) : count % maxExpansionSlots;
                        count++;
                    }
                    attachmentViewModel.StartDisplayActivationStopwatch();
                    menuViewModel.FireUpdate();
                };
            }
            return menuViewModel;
        }

        protected void createApplyAttributeMenu(AttributeModel attributeModel, AttachmentOrientation attachmentOrientation,
              AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);
            attachmentViewModel.ShowOnAttributeTapped = true;

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
                IsAlwaysDisplayed = false,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    TextAngle = textAngle,
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                    Label = "Apply",
                    AttributeViewModel = new AttributeViewModel(this, attributeModel),
                    EditNameOnTap = true
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);
        }

        protected MenuViewModel createAttributeLabelMenu(AttachmentOrientation attachmentOrientation, AttributeModel code,
            AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent, 
            DroppedTriggeredHandler droppedTriggered,
            out MenuItemViewModel menuItemViewModel)
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == attachmentOrientation);

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 2,
                NrRows = attachmentOrientation == AttachmentOrientation.Bottom ? 2 : 5
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            menuItemViewModel = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 5 : 1,
                RowSpan = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : 5,
                Column = attachmentOrientation == AttachmentOrientation.Bottom ? 0 : 1,
                Size = size,
                Position = this.Position,
                TargetSize = size,
                IsAlwaysDisplayed = true,
                IsWidthBoundToParent = isWidthBoundToParent,
                IsHeightBoundToParent = isHeightBoundToParent,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    TextAngle = textAngle,
                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                    EditNameOnTap = !( code?.FuncModel is AttributeModel.AttributeFuncModel.AttributeColumnFuncModel),
                    Label = code?.DisplayName,
                    AttributeViewModel = new AttributeViewModel(this, code),
                    DroppedTriggered = droppedTriggered == null ? null : new Action<AttributeViewModel>(droppedTriggered)
                }
            };
            var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
            menuViewModel.MenuItemViewModels.Add(menuItemViewModel);
            if (code != null)
            {
                code.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "DisplayName")
                        attributeMenuItemViewModel.Label = code.DisplayName;
                };
            }
            attributeMenuItemViewModel.TappedTriggered += ((e) => // when label is tapped, display any attached menu options
                attachmentViewModel.StartDisplayActivationStopwatch());
            return menuViewModel;
        }
    }
}
