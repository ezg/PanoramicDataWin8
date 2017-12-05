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
    public class HistogramOperationViewModel : BaseVisualizationOperationViewModel
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