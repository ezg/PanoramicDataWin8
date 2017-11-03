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
            AttachementViewModels.Add(new AttachmentViewModel
            {
                AttachmentOrientation = AttachmentOrientation.TopRight,
                OperationViewModel = this
            });
            var attachmentRightViewModel = AttachementViewModels.Last();
            var menuViewModelDrag = new MenuViewModel
            {
                AttachmentOrientation = attachmentRightViewModel.AttachmentOrientation,
                NrColumns = 1,
                NrRows = 1
            };

            var menuItemDrag = new MenuItemViewModel
            {
                MenuViewModel = menuViewModelDrag,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Size = new Vec(25, 25),
                Position = this.Position,
                TargetSize = new Vec(25, 25),
                IsAlwaysDisplayed = false
            };
            var attrDrag = new CreateLinkMenuItemViewModel();
            attrDrag.CreateLinkEvent += (sender, bounds) =>
            {
                controller.view.FilterLinkViewController.Instance.CreateFilterLinkViewModel(this, bounds);
            };


            OperationViewModelTapped += (args) =>
            {
                attachmentRightViewModel.ActiveStopwatch.Restart();
            };

            menuItemDrag.MenuItemComponentViewModel = attrDrag;
            menuViewModelDrag.MenuItemViewModels.Add(menuItemDrag);
            attachmentRightViewModel.MenuViewModel = menuViewModelDrag;
        }

        public OperationViewModel(OperationModel operationModel)
        {
            _operationModel = operationModel;
            selectColor();
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
    }
    public class AttributeUsageOperationViewModel : OperationViewModel {
        
        public AttributeUsageOperationViewModel(OperationModel operationModel):base(operationModel)
        {

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
        public delegate void InputAddedHandler(object sender);
        public event InputAddedHandler TopInputAdded;

        protected void createTopInputsExpandingMenu(int maxColumns=3)
        {
            var attachmentViewModel =
                AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Top);
            attachmentViewModel.ShowOnAttributeMove = true;
            OperationViewModelTapped += (args) => attachmentViewModel.ActiveStopwatch.Restart();

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = maxColumns,
                NrRows = 1
            };

            if (!MainViewController.Instance.MainModel.IsDarpaSubmissionMode)
            {
                attachmentViewModel.MenuViewModel = menuViewModel;

                var addMenuItem = new MenuItemViewModel
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
                        Label = "+",
                        TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                        CanDrag = false,
                        CanDrop = true,
                        DroppedTriggered = attributeViewModel =>
                        {
                            var attributeModel = attributeViewModel.AttributeModel;
                            if (!OperationModel.AttributeUsageModels.Contains(attributeModel) &&
                                !attributeModelContainsAttributeModel((OperationModel as AttributeGroupOperationModel)?.AttributeModel, attributeModel))
                            {
                                OperationModel.AttributeUsageModels.Add(attributeModel);
                                if (TopInputAdded != null)
                                    TopInputAdded(this);
                            }
                        }
                    }
                };
                menuViewModel.MenuItemViewModels.Add(addMenuItem);

                OperationModel.AttributeUsageModels.CollectionChanged += (sender, args) =>
                {
                    var coll = sender as ObservableCollection<AttributeModel>;
                    var attributeModel = coll.FirstOrDefault();

                    // remove old ones first
                    if (args.OldItems != null)
                        foreach (var oldItem in args.OldItems)
                        {
                            var oldAttributeModel = oldItem as AttributeModel;
                            var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                                (((AttributeMenuItemViewModel) mvm.MenuItemComponentViewModel)
                                 .AttributeViewModel != null) &&
                                (((AttributeMenuItemViewModel) mvm.MenuItemComponentViewModel)
                                 .AttributeViewModel.AttributeModel ==
                                 oldAttributeModel));
                            if (found != null)
                                menuViewModel.MenuItemViewModels.Remove(found);
                        }

                    menuViewModel.NrRows = (int) Math.Ceiling(1.0 * coll.Count / maxColumns) + 1;

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
                                Position = addMenuItem.Position
                            };
                            var newAttr = new AttributeMenuItemViewModel
                            {
                                Label = newAttributeModel.DisplayName,
                                AttributeViewModel = new AttributeViewModel(this, newAttributeModel),
                                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                                CanDrag = false,
                                CanDrop = false
                            };

                            if (newAttributeModel != null)
                            {
                                newAttributeModel.PropertyChanged += (sender2, args2) => newAttr.Label = (sender2 as AttributeModel).DisplayName;
                            }

                            newMenuItem.Deleted += (sender1, args1) =>
                            {
                                var atm = ((AttributeMenuItemViewModel) ((MenuItemViewModel) sender1).MenuItemComponentViewModel).AttributeViewModel.AttributeModel;
                                OperationModel.AttributeUsageModels.Remove(atm);
                            };
                            newMenuItem.MenuItemComponentViewModel = newAttr;
                            menuViewModel.MenuItemViewModels.Add(newMenuItem);
                        }
                    }

                    var count = 0;
                    foreach (var menuItemViewModel in menuViewModel.MenuItemViewModels.Where(mvm => mvm != addMenuItem))
                    {
                        menuItemViewModel.Column = count % maxColumns;
                        menuItemViewModel.Row = menuViewModel.NrRows - 1 - (int) Math.Floor(1.0* count / maxColumns);
                        count++;
                    }
                    attachmentViewModel.ActiveStopwatch.Restart();
                    menuViewModel.FireUpdate();
                };
            }
        }
        protected void createLabelMenu(AttachmentOrientation attachmentOrientation, IDEAAttributeModel code,
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
            menuViewModel.MenuItemViewModels.Add(menuItem);

            var attr1 = new AttributeMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                Label = code.DisplayName,
                DisplayOnTap = true,
                AttributeViewModel = new AttributeViewModel(this, code)
            };
            menuItem.MenuItemComponentViewModel = attr1;

            code.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "DisplayName")
                    attr1.Label = code.DisplayName;
            };
        }
    }

}