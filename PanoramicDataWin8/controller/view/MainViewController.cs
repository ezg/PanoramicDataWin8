﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Core;
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.idea;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis;
using PanoramicDataWin8.view.vis.menu;

namespace PanoramicDataWin8.controller.view
{
    public class MainViewController
    {
        private readonly Gesturizer _gesturizer = new Gesturizer();

        private MainViewController(InkableScene inkableScene, MainPage mainPage)
        {
            InkableScene = inkableScene;
            MainPage = mainPage;

            // disable for cidr demo
            BrushableViewController.CreateInstance(OperationViewModels);
            ComparisonViewController.CreateInstance(OperationViewModels);

            MainModel = new MainModel();
           
            AttributeTransformationViewModel.AttributeTransformationViewModelDropped += AttributeTransformationViewModelDropped;
            IDisposable disposable = Observable.FromEventPattern<AttributeTransformationViewModelEventArgs>(typeof(AttributeTransformationViewModel), "AttributeTransformationViewModelMoved")
                .Sample(TimeSpan.FromMilliseconds(20))
                .Subscribe(async arg =>
                {
                    var dispatcher = MainPage.Dispatcher;
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        AttributeTransformationViewModelMoved(arg.Sender, arg.EventArgs);
                    });
                });

            InputGroupViewModel.InputGroupViewModelDropped += InputGroupViewModelDropped;
            InputGroupViewModel.InputGroupViewModelMoved += InputGroupViewModelMoved;

            OperationTypeModel.OperationTypeModelDropped += OperationTypeModelDropped;
            OperationTypeModel.OperationTypeModelMoved += OperationTypeModelMoved;

            InkableScene.InkCollectedEvent += InkableSceneInkCollectedEvent;
            OperationViewModels.CollectionChanged += OperationViewViewModels_CollectionChanged;

            _gesturizer.AddGesture(new FilterGesture(InkableScene));
            _gesturizer.AddGesture(new ConnectGesture(InkableScene));
            _gesturizer.AddGesture(new EraseGesture(InkableScene));
            //_gesturizer.AddGesture(new ScribbleGesture(_root));
        }

        public static MainViewController Instance { get; private set; }

        public InkableScene InkableScene { get; }

        public ObservableCollection<OperationViewModel> OperationViewModels { get; } = new ObservableCollection<OperationViewModel>();

        public MainModel MainModel { get; }

        public MainPage MainPage { get; }

        public async void LoadConfig()
        {
            var installedLoc = Package.Current.InstalledLocation;
            var mainConifgContent = await installedLoc.GetFileAsync(@"Assets\data\main.ini").AsTask().ContinueWith(t => FileIO.ReadTextAsync(t.Result)).Result;
            MainModel.Backend = mainConifgContent.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("backend"))
                .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            MainModel.StartDataset = mainConifgContent.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("startdataset"))
                .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            MainModel.ThrottleInMillis = double.Parse(mainConifgContent.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("throttle"))
                .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
            MainModel.SampleSize = double.Parse(mainConifgContent.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("samplesize"))
                .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

            MainModel.DatasetConfigurations.Clear();
            if (MainModel.Backend.ToLower() == "progressive")
            {
                try
                {
                    MainModel.Ip = mainConifgContent.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("ip"))
                        .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                    MainModel.RenderShadingIn1DHistograms = bool.Parse(mainConifgContent.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("rendershadingin1dhistograms"))
                        .Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

                    LoadCatalog();
                }
                catch (Exception exc)
                {
                    ErrorHandler.HandleError(exc.Message);
                }
            }
        }

        public async void LoadCatalog()
        {
            var catalogCommand = new CatalogCommand();
            var catalog = await catalogCommand.GetCatalog();

            MainModel.DatasetConfigurations.Clear();
            foreach (var schema in catalog.Schemas)
            {
                var dataSetConfig = new DatasetConfiguration
                {
                    Schema = schema,
                    Backend = "progressive",
                    ThrottleInMillis = MainModel.ThrottleInMillis,
                    SampleSize = MainModel.SampleSize
                };
                MainModel.DatasetConfigurations.Add(dataSetConfig);
            }

            if (MainModel.DatasetConfigurations.Any(ds => ds.Schema.DisplayName.ToLower().Contains(MainModel.StartDataset)))
            {
                LoadData(MainModel.DatasetConfigurations.First(ds => ds.Schema.DisplayName.ToLower().Contains(MainModel.StartDataset)));
            }
            else
            {
                LoadData(MainModel.DatasetConfigurations.First());
            }

            setupOperationTypeModels();
        }

        private void setupOperationTypeModels()
        {
            var parent = new OperationTypeGroupModel();
            var vis = new OperationTypeGroupModel {Name = "vis", OperationType = OperationType.Group};
            parent.OperationTypeModels.Add(vis);
            vis.OperationTypeModels.Add(new OperationTypeModel {Name = "hist", OperationType = OperationType.Histogram});

            var other = new OperationTypeGroupModel {Name = "create", OperationType = OperationType.Group};
            parent.OperationTypeModels.Add(other);
            other.OperationTypeModels.Add(new OperationTypeModel { Name = "attribute\ngroup", OperationType = OperationType.AttributeGroup });
            other.OperationTypeModels.Add(new OperationTypeModel { Name = "filter", OperationType = OperationType.Filter });
            other.OperationTypeModels.Add(new OperationTypeModel { Name = "definition", OperationType = OperationType.Definition });
            other.OperationTypeModels.Add(new OperationTypeModel { Name = "calculation", OperationType = OperationType.Calculation });
            other.OperationTypeModels.Add(new OperationTypeModel { Name = "classifier", OperationType = OperationType.Classifier });
            other.OperationTypeModels.Add(new OperationTypeModel { Name = "regresser", OperationType = OperationType.Regresser });
            other.OperationTypeModels.Add(new OperationTypeModel { Name = "example", OperationType = OperationType.Example });

            var funcs = new OperationTypeGroupModel { Name = "functions", OperationType = OperationType.Group };
            parent.OperationTypeModels.Add(funcs);
            funcs.OperationTypeModels.Add(new OperationTypeModel { Name = "MinMaxScale", OperationType = OperationType.Function, FunctionType= new MinMaxScaleFunctionSubtypeModel() });

            MainModel.OperationTypeModels = parent.OperationTypeModels.ToList();
        }

        public void LoadData(DatasetConfiguration datasetConfiguration)
        {
            if (MainModel.QueryExecuter != null)
            {
                MainModel.QueryExecuter.HaltAllJobs();
            }

            if (datasetConfiguration.Backend.ToLower() == "progressive")
            {
                MainModel.SchemaModel = new IDEASchemaModel();
                MainModel.ThrottleInMillis = datasetConfiguration.ThrottleInMillis;
                MainModel.SampleSize = datasetConfiguration.SampleSize;
                MainModel.QueryExecuter = new IDEAQueryExecuter();
                HypothesesViewController.Instance.ClearAllStatisticalComparison();
                ((IDEASchemaModel) MainModel.SchemaModel).RootOriginModel = new IDEAOriginModel(datasetConfiguration);
                ((IDEASchemaModel) MainModel.SchemaModel).RootOriginModel.LoadInputFields();
            }
        }


        public static void CreateInstance(InkableScene root, MainPage mainPage)
        {
            Instance = new MainViewController(root, mainPage);
            Instance.LoadConfig();
        }

        public HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(AttributeModel attributeModel, Pt position)
        {
            var visModel = OperationViewModelFactory.CreateDefaultHistogramOperationViewModel(MainModel.SchemaModel, attributeModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }
        public FunctionOperationViewModel CreateDefaultFunctionOperationViewModel(Pt position, FunctionSubtypeModel functionSubtypeModel)
        {
            var visModel = OperationViewModelFactory.CreateDefaultFunctionOperationViewModel(MainModel.SchemaModel, position, functionSubtypeModel);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }

        public CalculationOperationViewModel CreateDefaultCalculationOperationViewModel(Pt position)
        {
            var visModel = OperationViewModelFactory.CreateDefaultCalculationOperationViewModel(MainModel.SchemaModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }

        public DefinitionOperationViewModel CreateDefaultDefinitionOperationViewModel(Pt position)
        {
            var visModel = OperationViewModelFactory.CreateDefaultDefinitionOperationViewModel(MainModel.SchemaModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }
        public ClassifierOperationViewModel CreateDefaultClassifierOperationViewModel(Pt position)
        {
            var visModel = OperationViewModelFactory.CreateDefaultClassifierOperationViewModel(MainModel.SchemaModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }
        public RegresserOperationViewModel CreateDefaultRegresserOperationViewModel(Pt position)
        {
            var visModel = OperationViewModelFactory.CreateDefaultRegresserOperationViewModel(MainModel.SchemaModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }
        public ExampleOperationViewModel CreateDefaultExampleOperationViewModel(Pt position)
        {
            var visModel = OperationViewModelFactory.CreateDefaultExampleOperationViewModel(MainModel.SchemaModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }
        public AttributeGroupOperationViewModel CreateDefaultAttributeGroupOperationViewModel(Pt position)
        {
            var visModel = OperationViewModelFactory.CreateDefaultAttributeGroupOperationViewModel(MainModel.SchemaModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }

        public FilterOperationViewModel CreateDefaultFilterOperationViewModel(Pt position, bool fromMouse)
        {
            var visModel = OperationViewModelFactory.CreateDefaultFilterOperationViewModel(MainModel.SchemaModel, position, fromMouse);
            visModel.Position = position;
            addAttachmentViews(visModel);
            OperationViewModels.Add(visModel);
            return visModel;
        }


        public OperationContainerView CopyOperationViewModel(OperationViewModel operationViewModel, Pt? centerPoint)
        {
            var newOperationContainerView = new OperationContainerView();
            var newOperationViewModel = OperationViewModelFactory.CopyOperationViewModel(operationViewModel);
            if (newOperationViewModel != null)
            {
                addAttachmentViews(newOperationViewModel);
                OperationViewModels.Add(newOperationViewModel);

                if (centerPoint.HasValue)
                    newOperationViewModel.Position = (Pt) centerPoint - operationViewModel.Size/2.0;

                newOperationContainerView.DataContext = newOperationViewModel;
                InkableScene.Add(newOperationContainerView);
                return newOperationContainerView;
            }
            return null;
        }

        private void addAttachmentViews(OperationViewModel visModel)
        {
            foreach (var attachmentViewModel in visModel.AttachementViewModels)
            {
                var attachmentView = new AttachmentView
                {
                    DataContext = attachmentViewModel
                };
                InkableScene.Add(attachmentView);
            }
        }
        
        public void RemoveOperationViewModel(OperationContainerView operationContainerView)
        {
            var operationViewModel = (OperationViewModel) operationContainerView.DataContext;
            MainModel.QueryExecuter.HaltJob(operationViewModel.OperationModel);
            OperationViewModels.Remove(operationViewModel);
            Instance.InkableScene.Remove(operationContainerView);

            operationContainerView.Dispose();
            foreach (var attachmentView in Instance.InkableScene.Elements.Where(e => e is AttachmentView).ToList())
            {
                if (((AttachmentViewModel) attachmentView.DataContext).OperationViewModel == operationViewModel)
                {
                    ((AttachmentView) attachmentView).Dispose();
                    Instance.InkableScene.Remove(attachmentView);
                }
            }
            if (operationViewModel.OperationModel is IFilterConsumerOperationModel)
            {
                foreach (var model in ((IFilterConsumerOperationModel) operationViewModel.OperationModel).ConsumerLinkModels.ToArray())
                {
                    ((IFilterConsumerOperationModel) model.FromOperationModel).ConsumerLinkModels.Remove(model);
                    ((IFilterConsumerOperationModel) model.ToOperationModel).ConsumerLinkModels.Remove(model);
                }
            }
            if (operationViewModel.OperationModel is IFilterProviderOperationModel)
            {
                foreach (var model in ((IFilterProviderOperationModel)operationViewModel.OperationModel).ProviderLinkModels.ToArray())
                {
                    ((IFilterProviderOperationModel)model.FromOperationModel).ProviderLinkModels.Remove(model);
                    ((IFilterProviderOperationModel)model.ToOperationModel).ProviderLinkModels.Remove(model);
                }
            }
        }

        private void OperationTypeModelMoved(object sender, OperationTypeModelEventArgs e)
        {
        }

        private void OperationTypeModelDropped(object sender, OperationTypeModelEventArgs e)
        {
            var width = OperationViewModel.WIDTH;
            var height = OperationViewModel.HEIGHT;
            var size = new Vec(width, height);
            var position = (Pt) new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size/2.0;

            var operationTypeModel = sender as OperationTypeModel;

            OperationViewModel operationViewModel = null;
            if (operationTypeModel.OperationType == OperationType.Histogram)
            {
                operationViewModel = CreateDefaultHistogramOperationViewModel(null, position);
            }
            else if (operationTypeModel.OperationType == OperationType.Classifier)
            {
                operationViewModel = CreateDefaultClassifierOperationViewModel(position);
            }
            else if (operationTypeModel.OperationType == OperationType.Regresser)
            {
                operationViewModel = CreateDefaultRegresserOperationViewModel(position);
            }
            else if (operationTypeModel.OperationType == OperationType.Example)
            {
                operationViewModel = CreateDefaultExampleOperationViewModel(position);
            }
            else if (operationTypeModel.OperationType == OperationType.AttributeGroup)
            {
                operationViewModel = CreateDefaultAttributeGroupOperationViewModel(position);
                height = 50;
                size = new Vec(width, height);
            }
            else if (operationTypeModel.OperationType == OperationType.Filter)
            {
                operationViewModel = CreateDefaultFilterOperationViewModel(position, controller.view.MainViewController.Instance.MainPage.LastTouchWasMouse);
                height = controller.view.MainViewController.Instance.MainPage.LastTouchWasMouse ? 50 : height;
                size = new Vec(width, height);
            }
            else if (operationTypeModel.OperationType == OperationType.Definition)
                operationViewModel = CreateDefaultDefinitionOperationViewModel(position);
            else if (operationTypeModel.OperationType == OperationType.Calculation)
                operationViewModel = CreateDefaultCalculationOperationViewModel(position);
            else if (operationTypeModel.OperationType == OperationType.Function)
            {
                operationViewModel = CreateDefaultFunctionOperationViewModel(position, operationTypeModel.FunctionType);
                height = 50;
                size = new Vec(width, height);
            }
            if (operationViewModel != null)
            {
                operationViewModel.Size = size;
                var operationContainerView = new OperationContainerView() { DataContext = operationViewModel };
                InkableScene.Add(operationContainerView);
            }
        }

        private void InputGroupViewModelMoved(object sender, InputGroupViewModelEventArgs e)
        {
            /*IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<InputGroupViewModelEventHandler> hits = new List<InputGroupViewModelEventHandler>();
            foreach (var element in InkableScene.GetDescendants().Where(ele => ele is InputGroupViewModelEventHandler).Select(ele => ele as InputGroupViewModelEventHandler))
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }
            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();

            foreach (var element in InkableScene.GetDescendants().Where(ele => ele is InputGroupViewModelEventHandler).Select(ele => ele as InputGroupViewModelEventHandler))
            {
                element.InputGroupViewModelMoved(
                        sender as InputGroupViewModel, e,
                        hits.Count() > 0 ? orderderHits[0] == element : false);
            }*/
        }


        private void InputGroupViewModelDropped(object sender, InputGroupViewModelEventArgs e)
        {
            /*IGeometry mainPageBounds = e.Bounds.GetPolygon();
             List<InputGroupViewModelEventHandler> hits = new List<InputGroupViewModelEventHandler>();
             foreach (var element in InkableScene.GetDescendants().OfType<InputGroupViewModelEventHandler>())
             {
                 var geom = element.BoundsGeometry;
                 if (geom != null && mainPageBounds.Intersects(geom))
                 {
                     hits.Add(element);
                 }
             }
 
             var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();
             foreach (var element in InkableScene.GetDescendants().OfType<InputGroupViewModelEventHandler>())
             {
                 element.InputGroupViewModelDropped(
                         sender as InputGroupViewModel, e,
                         hits.Count() > 0 && orderderHits[0] == element);
             }*/
        }

        private void AttributeTransformationViewModelMoved(object sender, AttributeTransformationViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            var hits = new List<AttributeTransformationViewModelEventHandler>();
            var attTransDescendants = InkableScene.GetDescendants().OfType<AttributeTransformationViewModelEventHandler>().ToList();
            foreach (var element in attTransDescendants)
            {
                var geom = element.BoundsGeometry;
                if ((geom != null) && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }

            var ops = InkableScene.GetDescendants().OfType<OperationContainerView>().ToList();
            foreach (var element in OperationViewModels)
            {
                var geom = element.Bounds.GetPolygon();
                if ((geom != null) && (mainPageBounds.Overlaps(geom)))
                {
                    foreach (var a in element.AttachementViewModels)
                    {
                        if (a.ShowOnAttributeMove)
                        {
                            a.ActiveStopwatch.Restart();
                        }
                    }
                }
            }

            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();

            foreach (var element in attTransDescendants)
            {
                element.AttributeTransformationViewModelMoved(
                    sender as AttributeTransformationViewModel, e,
                    (hits.Count() > 0) && (orderderHits[0] == element));
            }
        }

        private void AttributeTransformationViewModelDropped(object sender, AttributeTransformationViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            var hits = new List<AttributeTransformationViewModelEventHandler>();
            var attTransDescendants = InkableScene.GetDescendants().OfType<AttributeTransformationViewModelEventHandler>().ToList();
            foreach (var element in attTransDescendants)
            {
                var geom = element.BoundsGeometry;
                if ((geom != null) && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }

            var width = OperationViewModel.WIDTH;
            var height = OperationViewModel.HEIGHT;
            var size = new Vec(width, height);
            var position = (Pt) new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size/2.0;

            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();
            foreach (var element in attTransDescendants)
            {
                element.AttributeTransformationViewModelDropped(
                    sender as AttributeTransformationViewModel, e,
                    (hits.Count() > 0) && (orderderHits[0] == element));
            }

            if (!hits.Any())
            {
                var operationContainerView = new OperationContainerView();
                var histogramOperationViewModel = CreateDefaultHistogramOperationViewModel(e.AttributeTransformationModel.AttributeModel, position);
                histogramOperationViewModel.Size = size;
                operationContainerView.DataContext = histogramOperationViewModel;
                InkableScene.Add(operationContainerView);
            }
        }

        private void OperationViewViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((OperationViewModel) item).OperationModel.OperationModelUpdated -= OperationModel_OperationModelUpdated;
                    if (((OperationViewModel) item).OperationModel is IFilterProviderOperationModel)
                    {
                        foreach (var link in ((IFilterProviderOperationModel) ((OperationViewModel) item).OperationModel).ProviderLinkModels.ToArray())
                        {
                            FilterLinkViewController.Instance.RemoveFilterLinkViewModel(link);
                        }
                    }
                    if (((OperationViewModel)item).OperationModel is IFilterConsumerOperationModel)
                    {
                        foreach (var link in ((IFilterConsumerOperationModel)((OperationViewModel)item).OperationModel).ConsumerLinkModels.ToArray())
                        {
                            FilterLinkViewController.Instance.RemoveFilterLinkViewModel(link);
                        }
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    ((OperationViewModel) item).OperationModel.OperationModelUpdated += OperationModel_OperationModelUpdated;
                    ((OperationViewModel) item).OperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                }
            }
        }

        private void OperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            var model = (OperationModel) sender;
            if (model is IFilterProviderOperationModel &&
                e is FilterOperationModelUpdatedEventArgs &&
                ((e as FilterOperationModelUpdatedEventArgs).FilterOperationModelUpdatedEventType == FilterOperationModelUpdatedEventType.Links))
            {
                if (model.ResetFilterModelWhenInputLinksChange)
                    ((IFilterProviderOperationModel) model).ClearFilterModels();
            }

            if (!(e is FilterOperationModelUpdatedEventArgs) || (e is FilterOperationModelUpdatedEventArgs &&
         ((e as FilterOperationModelUpdatedEventArgs).FilterOperationModelUpdatedEventType != FilterOperationModelUpdatedEventType.FilterModels)))
            {
                MainModel.QueryExecuter.ExecuteOperationModel(model, true);
            }
        }


        private void InkableSceneInkCollectedEvent(object sender, InkCollectedEventArgs e)
        {
            var recognizedGestures = _gesturizer.Recognize(e.InkStroke.Clone());

            foreach (var recognizedGesture in recognizedGestures.ToList())
            {
                if (recognizedGesture is ConnectGesture)
                {
                    var connect = recognizedGesture as ConnectGesture;
                    bool created = false;
                    if (connect.FilterConsumerOperationViewModel == null)
                    {
                        created = true;
                        connect.CreateConsumer(e.InkStroke.Clone());
                    }
                    if (created && connect.FilterProviderOperationViewModel is FilterOperationModel &&
                        connect.FilterConsumerOperationViewModel is FilterOperationModel )
                    {
                        FilterLinkViewController.Instance.CreateFilterLinkViewModel(connect.FilterConsumerOperationViewModel,
                                                                                    connect.FilterProviderOperationViewModel);
                    } else
                        FilterLinkViewController.Instance.CreateFilterLinkViewModel(connect.FilterProviderOperationViewModel, 
                                                                                    connect.FilterConsumerOperationViewModel);

                }
                else if (recognizedGesture is HitGesture)
                {
                    var hitGesture = recognizedGesture as HitGesture;
                    foreach (var hitScribbable in hitGesture.HitScribbables)
                    {
                        if (hitScribbable is InkStroke)
                        {
                            InkableScene.Remove(hitScribbable as InkStroke);
                        }
                        else if (hitScribbable is OperationContainerView)
                        {
                            RemoveOperationViewModel(hitScribbable as OperationContainerView);
                        }
                        else if (hitScribbable is FilterLinkView)
                        {
                            var models = (hitScribbable as FilterLinkView).GetLinkModelsToRemove(e.InkStroke.Geometry);
                            foreach (var model in models)
                            {
                                FilterLinkViewController.Instance.RemoveFilterLinkViewModel(model);
                            }
                        }
                        else if (hitScribbable is MenuItemView)
                        {
                            var model = (hitScribbable as MenuItemView).DataContext as MenuItemViewModel;
                            model.FireDeleted();
                        }
                    }
                }
            }


            if (!recognizedGestures.Any())
            {
                var allScribbables = new List<IScribbable>();
                IScribbleHelpers.GetScribbablesRecursive(allScribbables, InkableScene.Elements.OfType<IScribbable>().ToList());
                var inkStroke = e.InkStroke.GetResampled(20);
                var inkStrokeLine = inkStroke.GetLineString();

                var consumed = false;
                foreach (var existingScribbable in allScribbables)
                {
                    var geom = existingScribbable.Geometry;
                    if (geom != null)
                    {
                        if (inkStrokeLine.Intersects(geom))
                        {
                            consumed = existingScribbable.Consume(e.InkStroke);
                            if (consumed)
                            {
                                break;
                            }
                        }
                    }
                }
                if (!consumed && !e.InkStroke.IsErase)
                {
                    InkableScene.Add(e.InkStroke);
                }
            }
        }

        public void UpdateJobStatus()
        {
            foreach (var current in OperationViewModels.ToArray())
            {
                // check if we need to halt or resume the job
                var tg = InkableScene.TransformToVisual(MainPage);
                var tt = tg.TransformBounds(current.Bounds);

                if (!MainPage.GetBounds().IntersectsWith(tt) && MainModel.QueryExecuter.IsJobRunning(current.OperationModel))
                {
                    MainModel.QueryExecuter.HaltJob(current.OperationModel);
                }
                else if (MainPage.GetBounds().IntersectsWith(tt) &&
                         !MainModel.QueryExecuter.IsJobRunning(current.OperationModel))
                {
                    MainModel.QueryExecuter.ResumeJob(current.OperationModel);
                }
            }
        }
    }
}