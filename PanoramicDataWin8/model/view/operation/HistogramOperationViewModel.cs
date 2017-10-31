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
    public class HistogramOperationViewModel : OperationViewModel
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
                        foreach (var mvm in menuViewModel.MenuItemViewModels.ToArray())
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
                            ColumnSpan = 1,
                            RowSpan = 2,
                            Column = 0,
                            Size = new Vec(54, 54),
                            Position = Position,
                            TargetSize = new Vec(54, 54),
                            IsAlwaysDisplayed = true
                        };
                        var attr1 = new StatisticalComparisonMenuItemViewModel
                        {
                            StatisticalComparisonOperationModel = model.StatisticalComparisonOperationModel
                        };

                        menuItem.MenuItemComponentViewModel = attr1;
                        menuViewModel.MenuItemViewModels.Add(menuItem);

                        var toggles = new List<ToggleMenuItemComponentViewModel>();
                        var items = new List<MenuItemViewModel>();
                        TestType[] types = new TestType[] { TestType.chi2, TestType.ttest };
                        int count = 0;
                        foreach (var type in types)
                        {
                            var toggleMenuItem = new MenuItemViewModel
                            {
                                MenuViewModel = menuViewModel,
                                Row = count,
                                RowSpan = 0,
                                Position = Position,
                                Column = 1,
                                Size = new Vec(54, 25),
                                TargetSize = new Vec(54, 25),
                                IsAlwaysDisplayed = true
                            };
                            //toggleMenuItem.Position = attachmentItemViewModel.Position;
                            var toggle = new ToggleMenuItemComponentViewModel
                            {
                                Label = type.ToString(),
                                IsChecked = model.StatisticalComparisonOperationModel.TestType == type
                            };
                            toggles.Add(toggle);
                            toggleMenuItem.MenuItemComponentViewModel = toggle;
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
                                        {
                                            tg.IsChecked = false;
                                        }
                                    }
                                }
                            };
                            menuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                            items.Add(toggleMenuItem);
                            count++;
                        }
                        foreach (var mi in items)
                        {
                            (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
                        }
                    }
                }
            };

            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        private void createRightHistogramMenu()
        {
            var rovm = RecommenderViewController.Instance.CreateRecommenderOperationViewModel(this);

            var attachmentViewModel = AttachementViewModels.First(avm => avm.AttachmentOrientation == AttachmentOrientation.Right);
            var menuViewModel = new MenuViewModel
            {
                AttachmentOrientation = attachmentViewModel.AttachmentOrientation,
                NrColumns = 4,
                NrRows = 4,
                IsRigid = true,
                RigidSize = 54
            };

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
                IsAlwaysDisplayed = false
            };
            var attr1 = new RecommenderMenuItemViewModel()
            {
                AttachmentViewModel = attachmentViewModel,
                RecommenderOperationViewModel = rovm
            };
            attr1.CreateRecommendationEvent += (sender, bounds, percentage) =>
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
            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem);

            OperationViewModelTapped += (args) =>
            {
                attachmentViewModel.ActiveStopwatch.Restart();
            };

            attachmentViewModel.MenuViewModel = menuViewModel;
        }

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
            var attr1 = new AttributeTransformationMenuItemViewModel
            {
                TextAngle = textAngle,
                TextBrush = new SolidColorBrush(Helpers.GetColorFromString("#29aad5"))
            };
            HistogramOperationModel.GetAttributeUsageTransformationModel(axis).CollectionChanged += (sender, args) =>
            {
                var coll = sender as ObservableCollection<AttributeTransformationModel>;
                var attributeTransformationModel = coll.FirstOrDefault();
                attr1.Label = attributeTransformationModel == null ? "" : attributeTransformationModel.GetLabel();
                attr1.AttributeTransformationViewModel = new AttributeTransformationViewModel(this, coll.FirstOrDefault());

                if (attributeTransformationModel != null)
                {
                    attributeTransformationModel.PropertyChanged += (sender2, args2) =>
                    {
                        attr1.Label = (sender2 as AttributeTransformationModel).GetLabel();
                    };
                    attributeTransformationModel.AttributeModel.PropertyChanged += (sender2, arg2) =>
                    {
                        attr1.Label = attributeTransformationModel.GetLabel();
                    };
                }

                // remove old ones first
                foreach (var mvm in menuViewModel.MenuItemViewModels.Where(mvm => mvm.MenuItemComponentViewModel is ToggleMenuItemComponentViewModel).ToArray())
                    menuViewModel.MenuItemViewModels.Remove(mvm);

                var aom = attr1.AttributeTransformationViewModel.AttributeTransformationModel;
                var aggregateFunctions = new[] { AggregateFunction.None, AggregateFunction.Count }.ToList();
                if (aom != null)
                {
                    if (aom.AttributeModel.DataType == DataType.Float ||
                        aom.AttributeModel.DataType == DataType.Double ||
                        aom.AttributeModel.DataType == DataType.Int)
                    {
                        aggregateFunctions.Add(AggregateFunction.Avg);
                        aggregateFunctions.Add(AggregateFunction.Sum);
                        if (MainViewController.Instance.MainModel.IsUnknownUnknownEnabled)
                        {
                            aggregateFunctions.Add(AggregateFunction.SumE);
                        }
                    }

                    var toggles = new List<ToggleMenuItemComponentViewModel>();
                    var items = new List<MenuItemViewModel>();

                    var count = 0;
                    foreach (var aggregationFunction in aggregateFunctions)
                    {
                        var toggleMenuItem = new MenuItemViewModel
                        {
                            MenuViewModel = menuViewModel,
                            Row = attachmentOrientation == AttachmentOrientation.Bottom ? 1 : count,
                            RowSpan = 0,
                            Position = Position,
                            Column = attachmentOrientation == AttachmentOrientation.Bottom ? count : 0,
                            Size = new Vec(32, 32),
                            TargetSize = new Vec(32, 32)
                        };
                        //toggleMenuItem.Position = attachmentItemViewModel.Position;
                        var toggle = new ToggleMenuItemComponentViewModel
                        {
                            Label = aggregationFunction.ToString(),
                            IsChecked = aom.AggregateFunction == aggregationFunction
                        };
                        toggles.Add(toggle);
                        toggleMenuItem.MenuItemComponentViewModel = toggle;
                        toggleMenuItem.MenuItemComponentViewModel.PropertyChanged += (sender2, args2) =>
                        {
                            var model = sender2 as ToggleMenuItemComponentViewModel;
                            if (args2.PropertyName == model.GetPropertyName(() => model.IsChecked))
                                if (model.IsChecked)
                                {
                                    aom.AggregateFunction = aggregationFunction;
                                    foreach (var tg in model.OtherToggles)
                                        tg.IsChecked = false;
                                }
                        };
                        menuViewModel.MenuItemViewModels.Add(toggleMenuItem);
                        items.Add(toggleMenuItem);
                        count++;
                    }

                    foreach (var mi in items)
                        (mi.MenuItemComponentViewModel as ToggleMenuItemComponentViewModel).OtherToggles.AddRange(toggles.Where(ti => ti != mi.MenuItemComponentViewModel));
                }

            };
            attr1.TappedTriggered = () => { attachmentViewModel.ActiveStopwatch.Restart(); };
            attr1.DroppedTriggered = attributeTransformationModel =>
            {
                if (attributeTransformationModel.AttributeModel.DataType == DataType.Undefined)
                    return;
                var otherAxis = axis == AttributeUsage.X ? AttributeUsage.Y : AttributeUsage.X;
                var existingModel = HistogramOperationModel.GetAttributeUsageTransformationModel(axis).Any() ?
                      HistogramOperationModel.GetAttributeUsageTransformationModel(axis).First() : null;
                var existingOtherModel = HistogramOperationModel.GetAttributeUsageTransformationModel(otherAxis).Any() ?
                    HistogramOperationModel.GetAttributeUsageTransformationModel(otherAxis).First() : null;
                var swapAxes = existingModel != null && existingOtherModel.AttributeModel == attributeTransformationModel.AttributeModel &&
                existingOtherModel.AggregateFunction == attributeTransformationModel.AggregateFunction;

                if (existingModel != null)
                    HistogramOperationModel.RemoveAttributeUsageTransformationModel(axis, existingModel);
                if (!HistogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                {
                    var value = new AttributeTransformationModel(attributeTransformationModel.AttributeModel);
                    value.AggregateFunction = AggregateFunction.Count;
                    HistogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                }
                HistogramOperationModel.AddAttributeUsageTransformationModel(axis, attributeTransformationModel);
                attachmentViewModel.ActiveStopwatch.Restart();
                if (swapAxes)
                {
                    HistogramOperationModel.RemoveAttributeUsageTransformationModel(otherAxis, existingOtherModel);
                    if (!HistogramOperationModel.GetAttributeUsageTransformationModel(AttributeUsage.DefaultValue).Any())
                    {
                        var value = new AttributeTransformationModel(attributeTransformationModel.AttributeModel);
                        value.AggregateFunction = AggregateFunction.Count;
                        HistogramOperationModel.AddAttributeUsageTransformationModel(AttributeUsage.DefaultValue, value);
                    }
                    HistogramOperationModel.AddAttributeUsageTransformationModel(otherAxis, existingModel);

                }
            };

            menuItem.MenuItemComponentViewModel = attr1;
            menuViewModel.MenuItemViewModels.Add(menuItem); 
            attachmentViewModel.MenuViewModel = menuViewModel;
        }

        public HistogramOperationViewModel(HistogramOperationModel histogramOperationModel, AttributeModel attributeModel) : base(histogramOperationModel)
        {
            addAttachmentViewModels();

            // axis attachment view models
            createAxisMenu(AttachmentOrientation.Bottom, AttributeUsage.X, new Vec(200, 50), 0, true, false);
            createAxisMenu(AttachmentOrientation.Left, AttributeUsage.Y, new Vec(50, 200), 270, false, true);
            if (!MainViewController.Instance.MainModel.IsDarpaSubmissionMode &&
                !MainViewController.Instance.MainModel.IsIGTMode)
            {
                createRightHistogramMenu();
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