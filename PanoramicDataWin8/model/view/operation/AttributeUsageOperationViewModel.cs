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

        static List<AttributeModel> findAllNestedGroups(AttributeModel attributeGroupModel)
        {
            var models = new List<AttributeModel>();
            if (attributeGroupModel != null)
            {
                models.Add(attributeGroupModel);
                var inputModels = (attributeGroupModel.FuncModel as AttributeModel.AttributeFuncModel.AttributeGroupFuncModel)?.InputModels;
                if (inputModels != null)
                    foreach (var at in inputModels)
                        models.AddRange(findAllNestedGroups(at));
            }
            return models;
        }
        static public bool attributeModelContainsAttributeModel(AttributeModel testAttributeModel, AttributeModel newAttributeModel)
        {
            return findAllNestedGroups(newAttributeModel).Contains(testAttributeModel);
        }
        public delegate void InputAddedHandler(object sender, ObservableCollection<AttributeModel> usageModels);
        public event         InputAddedHandler ExpandingMenuInputAdded;
        public delegate void DroppedTriggeredHandler(AttributeViewModel model);
        protected MenuViewModel createExpandingMenu(AttachmentOrientation orientation, ObservableCollection<AttributeModel> operationAttributeModels, 
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
                    DroppedTriggered = attributeViewModel =>
                    {
                        var attributeModel = attributeViewModel.AttributeModel;
                        if (!operationAttributeModels.Contains(attributeModel) &&
                            !attributeModelContainsAttributeModel((OperationModel as AttributeGroupOperationModel)?.AttributeModel, attributeModel))
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
                    var coll = sender as ObservableCollection<AttributeModel>;
                    var attributeModel = coll.FirstOrDefault();

                    // remove old ones first
                    if (args.OldItems != null)
                        foreach (var oldItem in args.OldItems)
                        {
                            var oldAttributeModel = oldItem as AttributeModel;
                            var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                                (((AttributeMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeViewModel != null) &&
                                (((AttributeMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeViewModel.AttributeModel ==
                                 oldAttributeModel));
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
                            var newAttributeModel = newItem as AttributeModel;
                            var newMenuItem = new MenuItemViewModel
                            {
                                MenuViewModel = menuViewModel,
                                Size = new Vec(50, 50),
                                TargetSize = new Vec(50, 50),
                                Position = menuItemViewModelCaptured.Position,
                                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                                {
                                    Label = newAttributeModel.DisplayName,
                                    AttributeViewModel = new AttributeViewModel(this, newAttributeModel),
                                    TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                                    CanDrag = true,
                                    CanDrop = false
                                }
                            };
                            newMenuItem.Deleted += (sender1, args1) =>
                            {
                                var atm = ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeModel;
                                OperationModel.AttributeUsageModels.Remove(atm);
                            };
                            menuViewModel.MenuItemViewModels.Add(newMenuItem);
                            
                            if (newAttributeModel != null)
                            {
                                newAttributeModel.PropertyChanged += (sender2, args2) => 
                                    (newMenuItem.MenuItemComponentViewModel as AttributeMenuItemViewModel).Label = (sender2 as AttributeModel).DisplayName;
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
            attributeMenuItemViewModel.TappedTriggered += (() => // when label is tapped, display any attached menu options
                attachmentViewModel.StartDisplayActivationStopwatch());
            return menuViewModel;
        }
    }
}
