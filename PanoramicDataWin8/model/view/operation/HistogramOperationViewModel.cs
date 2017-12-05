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
using System.ComponentModel;

namespace PanoramicDataWin8.model.view.operation
{
    public class HistogramOperationViewModel : LabeledOperationViewModel
    {
        private RecommenderOperationViewModel _recommenderOperationViewModel;

        private void createTopHistogramMenu()
        {
            
            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Top);
            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 2,
                NrRows = 2
            };

            HistogramOperationModel.PropertyChanged += (sender, args) =>
            {
                var model = HistogramOperationModel;
                if (args.PropertyName == model.GetPropertyName(() => model.StatisticalComparisonOperationModel))
                {
                    if (model.StatisticalComparisonOperationModel == null)
                    {
                        foreach (var mvm in menuViewModel.MenuItemViewModels)
                        {
                            menuViewModel.MenuItemViewModels.Remove(mvm);
                        }
                    }
                    else
                    {
                        var menuItem = new MenuItemViewModel
                        {
                            MenuViewModel = menuViewModel,
                            Row = 0,
                            RowSpan = 2,
                            Column = 0,
                            ColumnSpan = 1,
                            Size = new Vec(54, 54),
                            Position = Position,
                            TargetSize = new Vec(54, 54),
                            IsAlwaysDisplayed = true,
                            MenuItemComponentViewModel = new StatisticalComparisonMenuItemViewModel
                            {
                                StatisticalComparisonOperationModel = model.StatisticalComparisonOperationModel
                            }
                        };

                        menuViewModel.MenuItemViewModels.Add(menuItem);
                        
                        var count = 0;
                        foreach (var type in new TestType[] { TestType.chi2, TestType.ttest })
                        {
                            menuViewModel.MenuItemViewModels.Add(createTopFunctionToggleMenuItem(menuViewModel, model, count++, type));
                        }
                        var toggles = menuViewModel.MenuItemViewModels.Select(i => i.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel);
                        foreach (var t in toggles.Where(t => t != null))
                            t.OtherToggles.AddRange(toggles.Where(ti => ti !=t));
                    }
                }
            };

            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        MenuItemViewModel createTopFunctionToggleMenuItem(MenuViewModel menuViewModel, HistogramOperationModel model, int count, TestType type)
        {
            var toggleMenuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row        = count,
                RowSpan    = 0,
                Position   = Position,
                Column     = 1,
                Size       = new Vec(54, 25),
                TargetSize = new Vec(54, 25),
                IsAlwaysDisplayed = true,
                MenuItemComponentViewModel = new ToggleMenuItemComponentViewModel
                {
                    Label = type.ToString(),
                    IsChecked = model.StatisticalComparisonOperationModel?.TestType == type
                }
            };
            toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
            {
                var toogleModel = sender2 as ToggleMenuItemComponentViewModel;
                if (args2.PropertyName == model.GetPropertyName(() => toogleModel.IsChecked))
                {
                    if (toogleModel.IsChecked)
                    {
                        model.StatisticalComparisonOperationModel.TestType = type;
                        model.StatisticalComparisonOperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                        foreach (var tg in toogleModel.OtherToggles)
                            tg.IsChecked = false;
                    }
                }
            };
            return toggleMenuItem;
        }

        private void createRightHistogramMenu()
        {
            var rovm = RecommenderViewController.Instance.CreateRecommenderOperationViewModel(this);

            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            attachmentViewModel.ShowOnAttributeTapped = true;

            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 4,
                NrRows = 4,
                IsRigid = true,
                RigidSize = 54
            };
            attachmentViewModel.MenuViewModel = menuViewModel;

            var menuItem = new MenuItemViewModel
            {
                MenuViewModel = menuViewModel,
                Row = 0,
                ColumnSpan = 1,
                RowSpan = 1,
                Column = 0,
                Size = new Vec(54, 54),
                Position = Position,
                TargetSize = new Vec(54, 54),
                IsAlwaysDisplayed = false,
                MenuItemComponentViewModel = new RecommenderMenuItemViewModel()
                {
                    AttachmentViewModel = attachmentViewModel,
                    RecommenderOperationViewModel = rovm
                }
            };
            menuViewModel.MenuItemViewModels.Add(menuItem);
            (menuItem.MenuItemComponentViewModel as RecommenderMenuItemViewModel).CreateRecommendationEvent += (sender, bounds, percentage) =>
            {
                RecommenderOperationViewModel.RecommenderOperationModel.Page = 0;
                RecommenderOperationViewModel.RecommenderOperationModel.Budget =
                    (percentage / 100.0) * HypothesesViewController.Instance.HypothesesViewModel.StartWealth;
                RecommenderOperationViewModel.RecommenderOperationModel.ModelId = HypothesesViewController.Instance.RiskOperationModel.ModelId;
                MainViewController.Instance.MainModel.QueryExecuter.ExecuteOperationModel(
                    RecommenderOperationViewModel.OperationModel, true);
                RecommenderOperationViewModel.RecommenderOperationModel.Result = null;

                menuItem.IsAlwaysDisplayed = true;
                if (!menuViewModel.MenuItemViewModels.Any(
                    mi => mi.MenuItemComponentViewModel is RecommenderProgressMenuItemViewModel))
                {
                    var subMenuItem = new MenuItemViewModel
                    {
                        MenuViewModel = menuViewModel,
                        Row = 1,
                        ColumnSpan = 1,
                        RowSpan = 0,
                        Column = 0,
                        Size = new Vec(54, 54),
                        Position = Position,
                        TargetSize = new Vec(54, 54),
                        IsAlwaysDisplayed = true
                    };
                    var attr2 = new RecommenderProgressMenuItemViewModel()
                    {
                        HistogramOperationViewModel = this
                    };
                    subMenuItem.MenuItemComponentViewModel = attr2;
                    menuViewModel.MenuItemViewModels.Add(subMenuItem);
                }
            };
        }

        public class AttributeUsageMenu
        {
            HistogramOperationViewModel HistogramOperationViewModel;
            AttributeTransformationModel attributeTransformationModel;
            MenuItemViewModel menuItemViewModel;
            AttachmentViewModel attachmentViewModel;
            MenuViewModel menuViewModel;
            AttachmentOrientation AttachmentOrientation;
            AttributeUsage Axis;

            private void AttributeTransformationModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
                attributeMenuItemViewModel.Label = (sender as AttributeTransformationModel).GetLabel;
            }
            private void AttributeModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
                attributeMenuItemViewModel.Label = attributeTransformationModel.GetLabel;
            }
            public HistogramOperationModel HistogramOperationModel => (HistogramOperationModel)HistogramOperationViewModel.OperationModel;

            public bool CanSwapAxes { get; set; } = true;
            public bool CanTransformAxes { get; set; } = true;
            public AttributeUsageMenu(HistogramOperationViewModel histogramOperationViewModel, AttributeModel attributeModel, AttachmentOrientation attachmentOrientation, AttributeUsage axis, Vec size, double textAngle, bool isWidthBoundToParent, bool isHeightBoundToParent)
            {
                Axis = axis;
                AttachmentOrientation = attachmentOrientation;
                HistogramOperationViewModel = histogramOperationViewModel;
                attachmentViewModel = HistogramOperationViewModel.AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation);

                menuViewModel = HistogramOperationViewModel.createAttributeLabelMenu(AttachmentOrientation, attributeModel, Axis, size, textAngle,
                            isWidthBoundToParent, isHeightBoundToParent, droppedTriggered, out menuItemViewModel);
                menuViewModel.SynchronizeItemsDisplay = false;

                HistogramOperationModel.GetAttributeUsageTransformationModel(axis).CollectionChanged += AxisMenu_CollectionChanged;
            }

            void droppedTriggered(AttributeViewModel attributeViewModel)
            {
                var droppedTransformationModel = new AttributeTransformationModel(attributeViewModel.AttributeModel)
                { // copy ATM because drop target doesn't want to share parameters with source
                    AggregateFunction = CanTransformAxes ? (attributeViewModel.AttributeTransformationModel?.AggregateFunction ?? AggregateFunction.None) : AggregateFunction.None
                };
                if (droppedTransformationModel.AttributeModel.DataType != DataType.Undefined)
                {
                    var otherAxis = Axis == AttributeUsage.X ? AttributeUsage.Y : AttributeUsage.X;
                    var existingModel = HistogramOperationModel.GetAttributeUsageTransformationModel(Axis).FirstOrDefault();
                    var existingOtherModel = HistogramOperationModel.GetAttributeUsageTransformationModel(otherAxis).FirstOrDefault();
                    var swapAxes = CanSwapAxes && existingModel != null && existingOtherModel.AttributeModel == droppedTransformationModel.AttributeModel &&
                                                                      existingOtherModel.AggregateFunction == droppedTransformationModel.AggregateFunction;

                    AssignTransformationModel(droppedTransformationModel, existingModel, Axis);

                    if (swapAxes)
                    {
                        AssignTransformationModel(existingModel, existingOtherModel, otherAxis);
                    }
                }
            }

            void AssignTransformationModel(AttributeTransformationModel newModel, AttributeTransformationModel oldModel, AttributeUsage axis)
            {
                if (oldModel != null)
                {
                    HistogramOperationModel.RemoveAttributeUsageTransformationModel(axis, oldModel);
                    oldModel.PropertyChanged -= AttributeTransformationModel_PropertyChanged;
                    oldModel.AttributeModel.PropertyChanged -= AttributeModel_PropertyChanged;
                }
                if (attributeTransformationModel != null && !HistogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                {
                    HistogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue,
                        new AttributeTransformationModel(attributeTransformationModel.AttributeModel) { AggregateFunction = AggregateFunction.Count });
                }
                HistogramOperationModel.AddAttributeUsageTransformationModel(axis, newModel);
            }

            void AxisMenu_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
            {
                var oldAttributeTransformationModel = args.OldItems?.Count > 0 ? args.OldItems[0] as AttributeTransformationModel : null;
                if (oldAttributeTransformationModel != null)
                {
                    oldAttributeTransformationModel.PropertyChanged -= AttributeTransformationModel_PropertyChanged;
                    oldAttributeTransformationModel.AttributeModel.PropertyChanged -= AttributeModel_PropertyChanged;
                }

                var attributeMenuItemViewModel = menuItemViewModel.MenuItemComponentViewModel as AttributeMenuItemViewModel;
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                attributeTransformationModel = coll.FirstOrDefault();
                attributeMenuItemViewModel.Label = attributeTransformationModel?.GetLabel;
                attributeMenuItemViewModel.AttributeViewModel = new AttributeViewModel(HistogramOperationViewModel, attributeTransformationModel);
                if (attributeTransformationModel != null)
                {
                    attributeTransformationModel.PropertyChanged += AttributeTransformationModel_PropertyChanged;
                    attributeTransformationModel.AttributeModel.PropertyChanged += AttributeModel_PropertyChanged;
                }

                if (CanTransformAxes)  // display menu options of the transformations that can be performed on the attribute
                {
                    addAttributeTransformationToggleMenuItems(attributeMenuItemViewModel);
                }
            }

            void addAttributeTransformationToggleMenuItems(AttributeMenuItemViewModel attributeMenuItemViewModel)
            {
                // remove old ones first
                foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                    menuViewModel.MenuItemViewModels.Remove(mvm);

                var atm = (attributeMenuItemViewModel.AttributeViewModel as AttributeViewModel)?.AttributeTransformationModel;
                if (atm != null)
                {
                    var count = 0;
                    foreach (var aggregationFunction in atm.AggregateFunctions)
                    {
                        menuViewModel.MenuItemViewModels.Add(createAggregateTransformationToggleMenuItem(atm, count++, aggregationFunction));
                    }

                    var toggles = menuViewModel.MenuItemViewModels.Select(i => i.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel);
                    foreach (var t in toggles.Where(t => t != null))
                        t.OtherToggles.AddRange(toggles.Where(ti => ti != null && ti != t));
                }
            }

            MenuItemViewModel createAggregateTransformationToggleMenuItem(AttributeTransformationModel atm, int count, AggregateFunction aggregationFunction)
            {
                var toggleMenuItem = new MenuItemViewModel
                {
                    MenuViewModel = menuViewModel,
                    Column = AttachmentOrientation == AttachmentOrientation.Bottom ? count : 0,
                    Row = AttachmentOrientation == AttachmentOrientation.Bottom ? 1 : count,
                    RowSpan = 0,
                    Position = HistogramOperationViewModel.Position,
                    Size = new Vec(32, 32),
                    TargetSize = new Vec(32, 32),
                    MenuItemComponentViewModel = new ToggleMenuItemComponentViewModel
                    {
                        Label = aggregationFunction.ToString(),
                        IsChecked = atm.AggregateFunction == aggregationFunction
                    }
                };

                toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender, args) =>
                {
                    var model = sender as ToggleMenuItemComponentViewModel;
                    if (args.PropertyName == model.GetPropertyName(() => model.IsChecked))
                        if (model.IsChecked)
                        {
                            atm.AggregateFunction = aggregationFunction;
                            foreach (var tg in model.OtherToggles)
                                tg.IsChecked = false;
                        }
                };
                return toggleMenuItem;
            }
        }
        public HistogramOperationViewModel(HistogramOperationModel histogramOperationModel, AttributeModel attributeModel) : base(histogramOperationModel)
        {
            // axis attachment view models
            var xAxisMenu = new AttributeUsageMenu(this, attributeModel, AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            var yAxisMenu = new AttributeUsageMenu(this, attributeModel, AttachmentOrientation.Left, AttributeUsage.Y, new Vec(50, 200), 270, false, true);

            if (!MainViewController.Instance.MainModel.IsDarpaSubmissionMode && !MainViewController.Instance.MainModel.IsIGTMode)
            {
                // commented for demo
                //createRightHistogramMenu();
            }
            createTopHistogramMenu();
            createTopRightFilterDragMenu();

            if (attributeModel.InputVisualizationType == InputVisualizationTypeConstants.ENUM ||
                attributeModel.InputVisualizationType == InputVisualizationTypeConstants.NUMERIC)
            {
                histogramOperationModel.VisualizationType = VisualizationType.plot;

                var x     = new AttributeTransformationModel(attributeModel) { AggregateFunction = AggregateFunction.None };
                var y     = new AttributeTransformationModel(attributeModel) { AggregateFunction = AggregateFunction.Count };
                var value = new AttributeTransformationModel(attributeModel) { AggregateFunction = AggregateFunction.Count };
                var flipAxis = attributeModel.VisualizationHints.Contains(VisualizationHint.DefaultFlipAxis);
                histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X,            flipAxis ? y : x);
                histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.Y,            flipAxis ? x: y);
                histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
            }
            else if (attributeModel.InputVisualizationType == InputVisualizationTypeConstants.GEOGRAPHY)
            {
            }
            else if (attributeModel.InputVisualizationType == InputVisualizationTypeConstants.VECTOR)
            {
                histogramOperationModel.VisualizationType = VisualizationType.plot;
            }
            else
            {
                histogramOperationModel.VisualizationType = VisualizationType.table;
                var x = new AttributeTransformationModel(attributeModel);
                histogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.X, x);
            }
        }

        public HistogramOperationModel HistogramOperationModel => (HistogramOperationModel) OperationModel;

        public RecommenderOperationViewModel RecommenderOperationViewModel
        {
            get { return _recommenderOperationViewModel; }
            set { SetProperty(ref _recommenderOperationViewModel, value); }
        }
    }
}