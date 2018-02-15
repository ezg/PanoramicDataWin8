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
    public class OperationViewModel : ExtendedBindableBase
    {
        public static double WIDTH = 200;
        public static double HEIGHT = 200;

        public static double MIN_WIDTH = 100;
        public static double MIN_HEIGHT = 100;

        private static int _nextColorId;

        public static Color[] COLORS =
        {
            Color.FromArgb(255, 26, 188, 156),
            Color.FromArgb(255, 52, 152, 219),
            Color.FromArgb(255, 52, 73, 94),
            Color.FromArgb(255, 142, 68, 173),
            Color.FromArgb(255, 241, 196, 15),
            Color.FromArgb(255, 231, 76, 60),
            Color.FromArgb(255, 149, 165, 166),
            Color.FromArgb(255, 211, 84, 0),
            Color.FromArgb(255, 189, 195, 199),
            Color.FromArgb(255, 46, 204, 113),
            Color.FromArgb(255, 155, 89, 182),
            Color.FromArgb(255, 22, 160, 133),
            Color.FromArgb(255, 41, 128, 185),
            Color.FromArgb(255, 44, 62, 80),
            Color.FromArgb(255, 230, 126, 34),
            Color.FromArgb(255, 39, 174, 96),
            Color.FromArgb(255, 243, 156, 18),
            Color.FromArgb(255, 192, 57, 43),
            Color.FromArgb(255, 127, 140, 141)
        };

        private Stopwatch _activeStopwatch = new Stopwatch();

        private ObservableCollection<AttachmentViewModel> _attachementViewModels = new ObservableCollection<AttachmentViewModel>();

        private SolidColorBrush _brush;

        private Color _color = Color.FromArgb(0xff, 0x00, 0x00, 0x00);

        private SolidColorBrush _faintBrush;

        private OperationModel _operationModel;

        private Pt _postion;

        private Vec _size = new Vec(180, 100);

        protected void addAttachmentViewModels()
        {
            foreach (var attachmentOrientation in Enum.GetValues(typeof(AttachmentOrientation)).Cast<AttachmentOrientation>())
                AttachementViewModels.Add(new AttachmentViewModel
                {
                    AttachmentOrientation = attachmentOrientation,
                    OperationViewModel = this
                });
        }
        protected void createTopRightFilterDragMenu()
        {
            var attachmentRightViewModel = new AttachmentViewModel
            {
                AttachmentOrientation = AttachmentOrientation.TopRight,
                OperationViewModel    = this,
                ShowOnAttributeTapped = true,
                MenuViewModel = new MenuViewModel
                {
                    AttachmentOrientation = AttachmentOrientation.TopRight,
                    NrColumns = 1, 
                    NrRows    = 1,
                    MenuItemViewModels = new ObservableCollection<MenuItemViewModel>(new MenuItemViewModel[] {
                        new MenuItemViewModel()  {
                            Row = 0,
                            ColumnSpan = 1,
                            RowSpan = 1,
                            Column = 0,
                            Size = new Vec(25, 25),
                            Position = this.Position,
                            TargetSize = new Vec(25, 25),
                            IsAlwaysDisplayed = false,
                            MenuItemComponentViewModel = new CreateLinkMenuItemViewModel(this)
                        }
                    })
                }
            };
            AttachementViewModels.Add(attachmentRightViewModel);
        }

        public OperationViewModel(OperationModel operationModel)
        {
            _operationModel = operationModel;
            selectColor();
            addAttachmentViewModels();
        }

        public Stopwatch ActiveStopwatch
        {
            get { return _activeStopwatch; }
            set { SetProperty(ref _activeStopwatch, value); }
        }

        public OperationModel OperationModel
        {
            get { return _operationModel; }
            set { SetProperty(ref _operationModel, value); }
        }

        public ObservableCollection<AttachmentViewModel> AttachementViewModels
        {
            get { return _attachementViewModels; }
            set { SetProperty(ref _attachementViewModels, value); }
        }

        public SolidColorBrush Brush
        {
            get { return _brush; }
            set { SetProperty(ref _brush, value); }
        }

        public SolidColorBrush FaintBrush
        {
            get { return _faintBrush; }
            set { SetProperty(ref _faintBrush, value); }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                SetProperty(ref _color, value);
                Brush = new SolidColorBrush(_color);
                FaintBrush = new SolidColorBrush(Color.FromArgb(70, _color.R, _color.G, _color.B));
            }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public Pt Position
        {
            get { return _postion; }
            set { SetProperty(ref _postion, value); }
        }

        public Rct Bounds
        {
            get { return new Rct(Position, Size); }
        }

        public delegate void PointerEventHandler(PointerRoutedEventArgs e);
        public event PointerEventHandler OperationViewModelTapped;

        public void FireOperationViewModelTapped(PointerRoutedEventArgs e)
        {
            OperationViewModelTapped?.Invoke(e);
        }

        private void selectColor()
        {
            if (_nextColorId >= COLORS.Count() - 1)
                _nextColorId = 0;
            Color = COLORS[_nextColorId++];
        }

        public delegate void InputAddedHandler(object sender, ObservableCollection<AttributeTransformationModel> usageModels);
        public event InputAddedHandler ExpandingMenuInputAdded;
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
        protected MenuViewModel createExpandingMenu(
           AttachmentOrientation orientation,
           ObservableCollection<AttributeTransformationModel> operationAttributeModels,
           string label,
           int menuHeight,
           int maxExpansionSlots,
           bool isAlwaysDisplayed,
           bool clickToDismiss,
           bool canDeleteItems,
           out MenuItemViewModel menuItemViewModel
           )
        {
            var dict = new Dictionary<string, ObservableCollection<AttributeTransformationModel>>()
            {
                [label] = operationAttributeModels
            };
            return createExpandingMenu(orientation, dict, menuHeight, maxExpansionSlots, isAlwaysDisplayed, clickToDismiss, canDeleteItems, out menuItemViewModel);
        }

        protected MenuViewModel createExpandingMenu(
            AttachmentOrientation orientation, 
            Dictionary<string, ObservableCollection<AttributeTransformationModel>> operationAttributeModels,
            int menuHeight,
            int maxExpansionSlots,
            bool isAlwaysDisplayed,
            bool canDeleteItems,
            bool clickToDismiss, 
            out MenuItemViewModel menuItemViewModel
            )
        {
            var swapOrientation = orientation == AttachmentOrientation.Left || orientation == AttachmentOrientation.Right;
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == orientation);
            attachmentViewModel.ShowOnAttributeMove = true;
            attachmentViewModel.ShowOnAttributeTapped = true;

            var maxDimension = (int)Math.Ceiling(1.0 * operationAttributeModels.Count / maxExpansionSlots) + 1;
            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = swapOrientation ? maxDimension : maxExpansionSlots,
                NrRows    = swapOrientation ? maxExpansionSlots : maxDimension,
                ClickToDismiss = clickToDismiss,
                ParentSize = this.Size,
                MenuItemViewModels = new ObservableCollection<MenuItemViewModel>()
            };
            
            PropertyChanged += (sender, e) =>  {
                if (e.PropertyName == "Size")
                    menuViewModel.ParentSize = this.Size;
            };
            
            menuItemViewModel = operationAttributeModels.Select((opAtMo, colIndex) =>
                AddExpandingMenuItem(attachmentViewModel, menuViewModel, opAtMo.Value, opAtMo.Key,
                                         menuHeight, isAlwaysDisplayed, colIndex++, swapOrientation, canDeleteItems, maxExpansionSlots)).LastOrDefault();

            //if (!MainViewController.Instance.MainModel.IsDarpaSubmissionMode)
            {
                attachmentViewModel.MenuViewModel = menuViewModel;
            }
            return menuViewModel;
        }

        MenuItemViewModel createMenuItem(int menuHeight, bool isAlwaysDisplayed, 
            ObservableCollection<AttributeTransformationModel> attributeTransformationModelCollection, 
            AttributeTransformationModel newAttributeTransformationModel, 
            bool canDeleteItems)
        {
            var newMenuItem = new MenuItemViewModel
            {
                Size = new Vec(50, menuHeight),
                IsAlwaysDisplayed = isAlwaysDisplayed,
                TargetSize = new Vec(50, menuHeight),
                ProportionalSize = new Vec(newAttributeTransformationModel.AttributeModel.DataType == IDEA_common.catalog.DataType.String ? 2 : 1, 50),
                Visible = Visibility.Collapsed,
                MenuItemComponentViewModel = new AttributeMenuItemViewModel
                {
                    Label = newAttributeTransformationModel.GetLabel,
                    AttributeViewModel = new AttributeViewModel(this, newAttributeTransformationModel),
                    TextBrush = Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush,
                    CanDrag = true,
                    CanDelete = canDeleteItems,
                    CanDrop = false
                }
            };

            newMenuItem.Deleted += (sender1, args1) =>
            {
                var atm = ((AttributeMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeTransformationModel;
                attributeTransformationModelCollection.Remove(atm);
            };

            if (newAttributeTransformationModel != null)
            {
                newAttributeTransformationModel.PropertyChanged += (sender2, args2) =>
                    (newMenuItem.MenuItemComponentViewModel as AttributeMenuItemViewModel).Label = (sender2 as AttributeTransformationModel).GetLabel;
            }
            return newMenuItem;
        }

        static void layoutExpandingMenuItems(int maxExpansionSlots, MenuItemViewModel menuItemViewModel, bool swapOrientation, MenuViewModel menuViewModel)
        {
            menuItemViewModel?.SubMenuItemViewModels.ForEach((mItemViewModel, count) => {
                mItemViewModel.Visible = Visibility.Visible;
                mItemViewModel.Row = swapOrientation ? count % maxExpansionSlots : menuViewModel.NrRows - 1 - (int)Math.Floor(1.0 * count / maxExpansionSlots);
                mItemViewModel.Column = swapOrientation ? menuViewModel.NrColumns - 1 - (int)Math.Floor(1.0 * count / maxExpansionSlots) : count % maxExpansionSlots;
            });
        }

        void CollectionChanged(AttachmentViewModel attachmentViewModel, 
            MenuViewModel menuViewModel, 
            ObservableCollection<AttributeTransformationModel> operationAttributeModels, 
            MenuItemViewModel menuItemViewModelCaptured, int menuHeight,
            bool isAlwaysDisplayed, bool swapOrientation, int maxExpansionSlots)
        {
            operationAttributeModels.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                var attributeModel = coll.FirstOrDefault();

                var oldItems = args.OldItems;
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                    oldItems = menuViewModel.MenuItemViewModels.Select((mvm) => (mvm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel?.AttributeTransformationModel).ToList();
                // remove old ones first
                if (oldItems != null)
                    foreach (var oldItem in oldItems)
                    {
                        var oldAttributeTransformationModel = oldItem as AttributeTransformationModel;
                        var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                            ((mvm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel != null) &&
                            ((mvm.MenuItemComponentViewModel as AttributeMenuItemViewModel)?.AttributeViewModel?.AttributeTransformationModel ==
                                oldAttributeTransformationModel));
                        if (found != null)
                        {
                            menuViewModel.MenuItemViewModels.Remove(found);
                            menuItemViewModelCaptured.SubMenuItemViewModels.Remove(found);
                        }
                    }

                // add new ones
                if (args.NewItems != null)
                {
                    foreach (var newItem in args.NewItems)
                    {   
                        var newMenuItem = createMenuItem(menuHeight, isAlwaysDisplayed, operationAttributeModels, newItem as AttributeTransformationModel, true);
                        menuItemViewModelCaptured.AddSubMenuItemViewModel(newMenuItem);
                    }
                }

                var maxStacking = (int)Math.Ceiling(1.0 * coll.Count / maxExpansionSlots) + 1;
                if (swapOrientation)
                    menuViewModel.NrColumns = maxStacking;
                else menuViewModel.NrRows = maxStacking;
                layoutExpandingMenuItems(maxExpansionSlots, menuItemViewModelCaptured, swapOrientation, menuViewModel);
                menuItemViewModelCaptured.Focus();
                attachmentViewModel.StartDisplayActivationStopwatch();
                menuViewModel.FireUpdate();
                if (ExpandingMenuInputAdded != null)
                    ExpandingMenuInputAdded(this, operationAttributeModels);
                MainViewController.Instance.MainPage.clearAndDisposeMenus(true);
            };
        }

        MenuItemViewModel AddExpandingMenuItem(
            AttachmentViewModel attachmentViewModel, 
            MenuViewModel menuViewModel,
            ObservableCollection<AttributeTransformationModel> operationAttributeModels, 
            string label, 
            int menuHeight,
            bool isAlwaysDisplayed, 
            int column, 
            bool swapOrientation,
            bool canDeleteItems,
            int maxExpansionSlots)
        {
            var capturedMenuItemViewModel = new MenuItemViewModel
            {
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = column,
                Size = new Vec(18+label.Length*7, 25),
                TargetSize = new Vec(18 + label.Length * 7, 25),
                IsAlwaysDisplayed = isAlwaysDisplayed,
                IsWidthBoundToParent = false,
                IsHeightBoundToParent = false,
                Position = Position,
                MenuItemComponentViewModel = label == "" ? null : new AttributeMenuItemViewModel
                {
                    Label = label,
                    TextBrush = Application.Current.Resources.MergedDictionaries[0]["darkBrush"] as SolidColorBrush,
                    CanDrag = false,
                    CanDrop = true,
                    CanDelete = true,
                    DroppedTriggered = attributeViewModel =>
                    {
                        var attributeModel = new AttributeTransformationModel(attributeViewModel.AttributeTransformationModel.AttributeModel) { AggregateFunction = attributeViewModel.AttributeTransformationModel.AggregateFunction };
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

            CollectionChanged(attachmentViewModel, menuViewModel, operationAttributeModels, capturedMenuItemViewModel, menuHeight, isAlwaysDisplayed, swapOrientation,  maxExpansionSlots);

            menuViewModel.AddMenuItemViewModel(capturedMenuItemViewModel);

            foreach (var attributeTransformationModel in operationAttributeModels)
            {
                var newMenuItem = createMenuItem(menuHeight, isAlwaysDisplayed, operationAttributeModels, attributeTransformationModel, canDeleteItems);
                capturedMenuItemViewModel.AddSubMenuItemViewModel(newMenuItem);
            }
            layoutExpandingMenuItems(maxExpansionSlots, capturedMenuItemViewModel, swapOrientation, menuViewModel);
            return capturedMenuItemViewModel;
        }
    }

}