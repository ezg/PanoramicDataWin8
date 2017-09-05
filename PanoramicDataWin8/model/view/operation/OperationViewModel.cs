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
        public AttributeUsageOperationModel AttributeUsageOperationModel => (AttributeUsageOperationModel)OperationModel;

        static List<AttributeGroupModel> findAllNestedGroups(AttributeGroupModel attributeGroupModel)
        {
            var models = new List<AttributeGroupModel>();
            if (attributeGroupModel != null)
            {
                models.Add(attributeGroupModel);
                foreach (var at in attributeGroupModel.InputModels)
                    if (at is AttributeGroupModel)
                        models.AddRange(findAllNestedGroups(at as AttributeGroupModel));
            }
            return models;
        }
        static public bool attributeTransformationModelContainsAttributeModel(AttributeModel testAttributeModel, AttributeTransformationModel newAttributeTransformationModel)
        {
            return findAllNestedGroups(newAttributeTransformationModel.AttributeModel as AttributeGroupModel).Contains(testAttributeModel);
        }
        public delegate void InputAddedHandler(object sender);
        public event InputAddedHandler TopInputAdded;
        protected void createTopInputsExpandingMenu()
        {
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Top);
            attachmentViewModel.ShowOnAttributeMove = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 3,
                NrRows = 1
            };

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
                Position = Position
            };
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                Label = "+",
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#171717")),
                CanDrag = false,
                CanDrop = true
            };
            attr1.DroppedTriggered = attributeTransformationModel => {
                if (!AttributeUsageOperationModel.AttributeUsageTransformationModels.Contains(attributeTransformationModel) &&
                    !attributeTransformationModelContainsAttributeModel((AttributeUsageOperationModel as AttributeGroupOperationModel)?.AttributeGroupModel, attributeTransformationModel))
                {
                    AttributeUsageOperationModel.AttributeUsageTransformationModels.Add(attributeTransformationModel);
                    TopInputAdded(this);
                }
            };

            addMenuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(addMenuItem);
            attachmentViewModel.MenuViewModel = menuViewModel;

            AttributeUsageOperationModel.AttributeUsageTransformationModels.CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                var attributeTransformationModel = coll.FirstOrDefault();
                
                // remove old ones first
                if (args.OldItems != null)
                    foreach (var oldItem in args.OldItems)
                    {
                        var oldAttributeTransformationModel = oldItem as AttributeTransformationModel;
                        var found = menuViewModel.MenuItemViewModels.FirstOrDefault(mvm =>
                            (((AttributeTransformationMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeTransformationViewModel != null) &&
                            (((AttributeTransformationMenuItemViewModel)mvm.MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel ==
                             oldAttributeTransformationModel));
                        if (found != null)
                            menuViewModel.MenuItemViewModels.Remove(found);
                    }

                menuViewModel.NrRows = (int)Math.Ceiling(coll.Count / 3.0) + 1;

                // add new ones
                if (args.NewItems != null)
                    foreach (var newItem in args.NewItems)
                    {
                        var newAttributeTransformationModel = newItem as AttributeTransformationModel;
                        var newMenuItem = new MenuItemViewModel
                        {
                            MenuViewModel = menuViewModel,
                            Size = new Vec(50, 50),
                            TargetSize = new Vec(50, 50),
                            Position = addMenuItem.Position
                        };
                        var newAttr = new AttributeTransformationMenuItemViewModel
                        {
                            Label = newAttributeTransformationModel.GetLabel(),
                            AttributeTransformationViewModel = new AttributeTransformationViewModel(this, newAttributeTransformationModel),
                            TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5")),
                            CanDrag = false,
                            CanDrop = false
                        };

                        if (newAttributeTransformationModel != null)
                        {
                            newAttributeTransformationModel.PropertyChanged += (sender2, args2) =>
                            {
                                newAttr.Label = (sender2 as AttributeTransformationModel).GetLabel();
                            };
                            newAttributeTransformationModel.AttributeModel.PropertyChanged += (sender2, arg2) =>
                            {
                                newAttr.Label = (sender2 as AttributeModel).DisplayName;
                            };
                        }

                        newMenuItem.Deleted += (sender1, args1) =>
                        {
                            var atm =
                                ((AttributeTransformationMenuItemViewModel)((MenuItemViewModel)sender1).MenuItemComponentViewModel).AttributeTransformationViewModel.AttributeTransformationModel;
                            AttributeUsageOperationModel.AttributeUsageTransformationModels.Remove(atm);
                        };
                        newMenuItem.MenuItemComponentViewModel = newAttr;
                        menuViewModel.MenuItemViewModels.Add(newMenuItem);
                    }

                var count = 0;
                foreach (var menuItemViewModel in menuViewModel.MenuItemViewModels.Where(mvm => mvm != addMenuItem))
                {
                    menuItemViewModel.Column = count % 3;
                    menuItemViewModel.Row = menuViewModel.NrRows - 1 - (int)Math.Floor(count / 3.0);
                    count++;
                }
                attachmentViewModel.ActiveStopwatch.Restart();
                menuViewModel.FireUpdate();
            };


            OperationViewModelTapped += (args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };
        }
    }

}