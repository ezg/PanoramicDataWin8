using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using GeoAPI.Geometries;
using IDEA_common.catalog;
using Newtonsoft.Json.Linq;
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
using PanoramicDataWin8.view.vis.render;

namespace PanoramicDataWin8.controller.view
{
    public class MainViewController
    {
        private Gesturizer _gesturizer = new Gesturizer();
        private static MainViewController _instance;

        private MainViewController(InkableScene inkableScene, MainPage mainPage)
        {
            _inkableScene = inkableScene;
            _mainPage = mainPage;

            BrushableViewController.CreateInstance(_operationViewModels);
            ComparisonViewController.CreateInstance(_operationViewModels);

            _mainModel = new MainModel();
            
            AttributeTransformationViewModel.AttributeTransformationViewModelDropped += AttributeTransformationViewModelDropped;
            AttributeTransformationViewModel.AttributeTransformationViewModelMoved += AttributeTransformationViewModelMoved;

            InputGroupViewModel.InputGroupViewModelDropped += InputGroupViewModelDropped;
            InputGroupViewModel.InputGroupViewModelMoved += InputGroupViewModelMoved;

            OperationTypeModel.OperationTypeModelDropped += OperationTypeModelDropped;
            OperationTypeModel.OperationTypeModelMoved += OperationTypeModelMoved;

            _inkableScene.InkCollectedEvent += InkableSceneInkCollectedEvent;
            OperationViewModels.CollectionChanged += OperationViewViewModels_CollectionChanged;

            _gesturizer.AddGesture(new ConnectGesture(_inkableScene));
            _gesturizer.AddGesture(new EraseGesture(_inkableScene));
            //_gesturizer.AddGesture(new ScribbleGesture(_root));
        }
        
        public async void LoadConfig()
        {
            var installedLoc = Package.Current.InstalledLocation;
            string mainConifgContent = await installedLoc.GetFileAsync(@"Assets\data\main.ini").AsTask().ContinueWith(t => Windows.Storage.FileIO.ReadTextAsync(t.Result)).Result;
            MainModel.Backend = mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("backend"))
                .Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            MainModel.StartDataset = mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("startdataset"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            MainModel.ThrottleInMillis = double.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
               .First(l => l.ToLower().StartsWith("throttle"))
               .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
            MainModel.SampleSize = double.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("samplesize"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

            MainModel.DatasetConfigurations.Clear();
            if (MainModel.Backend.ToLower() == "progressive")
            {
                try
                {
                    MainModel.Ip = mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("ip"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                    MainModel.RenderShadingIn1DHistograms = bool.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("rendershadingin1dhistograms"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

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
            CatalogCommand catalogCommand = new CatalogCommand();
            Catalog catalog = await catalogCommand.GetCatalog();

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
                _mainModel.DatasetConfigurations.Add(dataSetConfig);
            }

            if (_mainModel.DatasetConfigurations.Any(ds => ds.Schema.DisplayName.ToLower().Contains(MainModel.StartDataset)))
            {
                LoadData(_mainModel.DatasetConfigurations.First(ds => ds.Schema.DisplayName.ToLower().Contains(MainModel.StartDataset)));
            }
            else
            {
                LoadData(_mainModel.DatasetConfigurations.First());
            }

            setupOperationTypeModels();
        }

        private void setupOperationTypeModels()
        {
            OperationTypeGroupModel parent = new OperationTypeGroupModel();
            OperationTypeGroupModel vis = new OperationTypeGroupModel() {Name = "vis", OperationType = OperationType.Group };
            parent.OperationTypeModels.Add(vis);
            vis.OperationTypeModels.Add(new OperationTypeModel() {Name = "hist", OperationType = OperationType.Histogram});

            OperationTypeGroupModel other = new OperationTypeGroupModel() { Name = "other", OperationType = OperationType.Group };
            parent.OperationTypeModels.Add(other);
            other.OperationTypeModels.Add(new OperationTypeModel() { Name = "example", OperationType = OperationType.Example });

            _mainModel.OperationTypeModels = parent.OperationTypeModels.ToList();
        }

        public void LoadData(DatasetConfiguration datasetConfiguration)
        {
            if (MainModel.SchemaModel != null && MainModel.SchemaModel.QueryExecuter != null)
            {
                MainModel.SchemaModel.QueryExecuter.HaltAllJobs();
            }

            if (datasetConfiguration.Backend.ToLower() == "progressive")
            {
                MainModel.SchemaModel = new IDEASchemaModel();
                MainModel.ThrottleInMillis = datasetConfiguration.ThrottleInMillis;
                MainModel.SampleSize = datasetConfiguration.SampleSize;
                ((IDEASchemaModel) MainModel.SchemaModel).QueryExecuter = new IDEAQueryExecuter();
                ((IDEASchemaModel) MainModel.SchemaModel).RootOriginModel = new IDEAOriginModel(datasetConfiguration);
                ((IDEASchemaModel) MainModel.SchemaModel).RootOriginModel.LoadInputFields();
            }
        }


        public static void CreateInstance(InkableScene root, MainPage mainPage)
        {
            _instance = new MainViewController(root, mainPage);
            _instance.LoadConfig();
        }
        
        public static MainViewController Instance
        {
            get
            {
                return _instance;
            }
        }

        private InkableScene _inkableScene;
        public InkableScene InkableScene
        {
            get
            {
                return _inkableScene;
            }
        }

        private ObservableCollection<OperationViewModel> _operationViewModels = new ObservableCollection<OperationViewModel>();
        public ObservableCollection<OperationViewModel> OperationViewModels
        {
            get
            {
                return _operationViewModels;
            }
        }

        private MainModel _mainModel;
        public MainModel MainModel
        {
            get
            {
                return _mainModel;
            }
        }

        private MainPage _mainPage;
        public MainPage MainPage
        {
            get
            {
                return _mainPage;
            }
        }

        public HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(AttributeModel attributeModel, Pt position)
        {
            HistogramOperationViewModel visModel = OperationViewModelFactory.CreateDefaultHistogramOperationViewModel(MainModel.SchemaModel, attributeModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            _operationViewModels.Add(visModel);
            return visModel;
        }

        public ExampleOperationViewModel CreateDefaultExampleOperationViewModel(Pt position)
        {
            ExampleOperationViewModel visModel = OperationViewModelFactory.CreateDefaultExampleOperationViewModel(MainModel.SchemaModel, position);
            visModel.Position = position;
            addAttachmentViews(visModel);
            _operationViewModels.Add(visModel);
            return visModel;
        }



        public void CopyOperationViewModel(OperationViewModel operationViewModel, Pt centerPoint)
        {
            OperationContainerView newOperationContainerView = new OperationContainerView();
            OperationViewModel newOperationViewModel = OperationViewModelFactory.CopyOperationViewModel(operationViewModel);
            addAttachmentViews(newOperationViewModel);
            _operationViewModels.Add(newOperationViewModel);

            newOperationViewModel.Position = centerPoint - (operationViewModel.Size / 2.0);

            newOperationContainerView.DataContext = newOperationViewModel;
            InkableScene.Add(newOperationContainerView);
        }

        private void addAttachmentViews(OperationViewModel visModel)
        {
            foreach (var attachmentViewModel in visModel.AttachementViewModels)
            {
                AttachmentView attachmentView = new AttachmentView()
                {
                    DataContext = attachmentViewModel
                };
                InkableScene.Add(attachmentView);
            }
        }


        public void RemoveOperationViewModel(OperationContainerView operationContainerView)
        {
            var operationViewModel = (OperationViewModel) operationContainerView.DataContext;
            MainModel.SchemaModel.QueryExecuter.RemoveJob((operationViewModel).OperationModel);
            _operationViewModels.Remove(operationViewModel);
            MainViewController.Instance.InkableScene.Remove(operationContainerView);

            operationContainerView.Dispose();
            foreach (var attachmentView in MainViewController.Instance.InkableScene.Elements.Where(e => e is AttachmentView).ToList())
            {
                if (((AttachmentViewModel) attachmentView.DataContext).OperationViewModel == operationViewModel)
                {
                    ((AttachmentView) attachmentView).Dispose();
                    MainViewController.Instance.InkableScene.Remove(attachmentView);
                }
            }
            if (operationViewModel.OperationModel is IFilterConsumerOperationModel)
            {
                foreach (var model in ((IFilterConsumerOperationModel) operationViewModel.OperationModel).LinkModels.ToArray())
                {
                    ((IFilterConsumerOperationModel) model.FromOperationModel).LinkModels.Remove(model);
                    ((IFilterConsumerOperationModel) model.ToOperationModel).LinkModels.Remove(model);
                }
            }
        }

        void OperationTypeModelMoved(object sender, OperationTypeModelEventArgs e)
        {
            
        }

        void OperationTypeModelDropped(object sender, OperationTypeModelEventArgs e)
        {
            double width = OperationViewModel.WIDTH;
            double height = OperationViewModel.HEIGHT;
            Vec size = new Vec(width, height);
            Pt position = (Pt)new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size / 2.0;

            var operationTypeModel = sender as OperationTypeModel;

            OperationViewModel operationViewModel = null; 
            if (operationTypeModel.OperationType == OperationType.Histogram)
            {
                operationViewModel = CreateDefaultHistogramOperationViewModel(null, position);
            }
            else if (operationTypeModel.OperationType == OperationType.Example)
            {
                operationViewModel = CreateDefaultExampleOperationViewModel(position);
            }

            if (operationViewModel != null)
            {
                OperationContainerView operationContainerView = new OperationContainerView();
                operationViewModel.Size = size;
                operationContainerView.DataContext = operationViewModel;
                InkableScene.Add(operationContainerView);
            }
        }
        void InputGroupViewModelMoved(object sender, InputGroupViewModelEventArgs e)
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


        void InputGroupViewModelDropped(object sender, InputGroupViewModelEventArgs e)
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
        
        void AttributeTransformationViewModelMoved(object sender, AttributeTransformationViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<AttributeTransformationViewModelEventHandler> hits = new List<AttributeTransformationViewModelEventHandler>();
            var tt = InkableScene.GetDescendants().OfType<AttributeTransformationViewModelEventHandler>().ToList();
            foreach (var element in tt)
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom)) 
                {
                    hits.Add(element);
                }
            }

            var ops = InkableScene.GetDescendants().OfType<OperationContainerView>().ToList();
            foreach (var element in ops)
            {
                var geom = element.Geometry;
                if (geom != null && mainPageBounds.Distance(geom) < 100)
                {
                    foreach (var a in (element.DataContext as OperationViewModel).AttachementViewModels)
                    {
                        if (a.ShowOnAttributeMove)
                        {
                            a.ActiveStopwatch.Restart();
                        }
                    }
                }
            }

            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();

            foreach (var element in InkableScene.GetDescendants().OfType<AttributeTransformationViewModelEventHandler>())
            {
                element.AttributeTransformationViewModelMoved(
                        sender as AttributeTransformationViewModel, e,
                        hits.Count() > 0 && orderderHits[0] == element);
            }
        }

        void AttributeTransformationViewModelDropped(object sender, AttributeTransformationViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<AttributeTransformationViewModelEventHandler> hits = new List<AttributeTransformationViewModelEventHandler>();
            foreach (var element in InkableScene.GetDescendants().OfType<AttributeTransformationViewModelEventHandler>())
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }

            double width = OperationViewModel.WIDTH;
            double height = OperationViewModel.HEIGHT;
            Vec size = new Vec(width, height);
            Pt position = (Pt) new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size / 2.0;

            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();
            foreach (var element in InkableScene.GetDescendants().OfType<AttributeTransformationViewModelEventHandler>())
            {
                element.AttributeTransformationViewModelDropped(
                        sender as AttributeTransformationViewModel, e,
                        hits.Count() > 0 && orderderHits[0] == element);
            }

            if (!hits.Any())
            {
                OperationContainerView operationContainerView = new OperationContainerView();
                HistogramOperationViewModel histogramOperationViewModel = CreateDefaultHistogramOperationViewModel(e.AttributeTransformationModel.AttributeModel, position);
                histogramOperationViewModel.Size = size;
                operationContainerView.DataContext = histogramOperationViewModel;
                InkableScene.Add(operationContainerView);
            }
        }

        void OperationViewViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((OperationViewModel) item).OperationModel.OperationModelUpdated -= OperationModel_OperationModelUpdated;
                    if (((OperationViewModel) item).OperationModel is IFilterConsumerOperationModel)
                    {
                        foreach (var link in ((IFilterConsumerOperationModel) ((OperationViewModel) item).OperationModel).LinkModels.ToArray())
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
                    ((OperationViewModel)item).OperationModel.FireOperationModelUpdated(new OperationModelUpdatedEventArgs());
                }
            }
        }

        private void OperationModel_OperationModelUpdated(object sender, OperationModelUpdatedEventArgs e)
        {
            OperationModel model = (OperationModel) sender;
            if (model is IFilterProviderOperationModel && 
                e is FilterOperationModelUpdatedEventArgs && 
                (e as FilterOperationModelUpdatedEventArgs).FilterOperationModelUpdatedEventType == FilterOperationModelUpdatedEventType.Links)
            {
                ((IFilterProviderOperationModel) model).ClearFilterModels();
            }

            if (!(e is FilterOperationModelUpdatedEventArgs) || (e is FilterOperationModelUpdatedEventArgs && 
                (e as FilterOperationModelUpdatedEventArgs).FilterOperationModelUpdatedEventType != FilterOperationModelUpdatedEventType.FilterModels))
            {
                model.SchemaModel.QueryExecuter.ExecuteOperationModel(model);
            }
        }
        

        void InkableSceneInkCollectedEvent(object sender, InkCollectedEventArgs e)
        {
            IList<IGesture> recognizedGestures = _gesturizer.Recognize(e.InkStroke.Clone());

            foreach (IGesture recognizedGesture in recognizedGestures.ToList())
            {
                if (recognizedGesture is ConnectGesture)
                {
                    ConnectGesture connect = recognizedGesture as ConnectGesture;
                    FilterLinkViewController.Instance.CreateFilterLinkViewModel(connect.FilterProviderOperationViewModel, connect.FilterConsumerOperationViewModel);
                }
                else if (recognizedGesture is HitGesture)
                {
                    HitGesture hitGesture = recognizedGesture as HitGesture;
                    foreach (IScribbable hitScribbable in hitGesture.HitScribbables)
                    {
                        if (hitScribbable is InkStroke)
                        {
                            _inkableScene.Remove(hitScribbable as InkStroke);
                        }
                        else if (hitScribbable is OperationContainerView)
                        {
                            RemoveOperationViewModel(hitScribbable as OperationContainerView);
                        }
                        else if (hitScribbable is FilterLinkView)
                        {
                            List<FilterLinkModel> models = (hitScribbable as FilterLinkView).GetLinkModelsToRemove(e.InkStroke.Geometry);
                            foreach (var model in models)
                            {
                                FilterLinkViewController.Instance.RemoveFilterLinkViewModel(model);
                            }
                        }
                        else if (hitScribbable is AttachmentItemView)
                        {
                            var model = ((hitScribbable as AttachmentItemView).DataContext as AttachmentItemViewModel);
                            if (model.AttachmentHeaderViewModel.RemovedTriggered != null)
                            {
                                model.AttachmentHeaderViewModel.RemovedTriggered(model);
                            }
                        }
                        else if (hitScribbable is MenuItemView)
                        {
                            var model = ((hitScribbable as MenuItemView).DataContext as MenuItemViewModel);
                            model.FireDeleted();
                        }
                    }
                }
            }


            if (!e.InkStroke.IsErase && !recognizedGestures.Any())
            {
                List<IScribbable> allScribbables = new List<IScribbable>();
                IScribbleHelpers.GetScribbablesRecursive(allScribbables, InkableScene.Elements.OfType<IScribbable>().ToList());
                var inkStroke = e.InkStroke.GetResampled(20);
                ILineString inkStrokeLine = inkStroke.GetLineString();

                bool consumed = false;
                foreach (IScribbable existingScribbable in allScribbables)
                {
                    IGeometry geom = existingScribbable.Geometry;
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
                if (!consumed)
                {
                    _inkableScene.Add(e.InkStroke);
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

                if (!MainPage.GetBounds().IntersectsWith(tt) && _mainModel.SchemaModel.QueryExecuter.IsJobRunning(current.OperationModel))
                {
                    _mainModel.SchemaModel.QueryExecuter.HaltJob(current.OperationModel);
                }
                else if (MainPage.GetBounds().IntersectsWith(tt) &&
                         !_mainModel.SchemaModel.QueryExecuter.IsJobRunning(current.OperationModel))
                {
                    _mainModel.SchemaModel.QueryExecuter.ResumeJob(current.OperationModel);
                }
            }
        }
    }
}
