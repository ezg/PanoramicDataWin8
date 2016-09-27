using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.data.progressive;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis;
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

            TaskModel.JobTypeViewModelDropped += TaskModelDropped;
            TaskModel.JobTypeViewModelMoved += TaskModelMoved;

            VisualizationTypeViewModel.VisualizationTypeViewModelDropped += VisualizationTypeViewModelDropped;
            VisualizationTypeViewModel.VisualizationTypeViewModelMoved += VisualizationTypeViewModelMoved;

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
            var backend = mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("backend"))
                .Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            var startDataSet = mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("startdataset"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            _mainModel.DatasetConfigurations.Clear();
            if (backend.ToLower() == "progressive")
            {
                try
                {
                    var ip = mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("ip"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                    var renderShadingIn1DHistograms = bool.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("rendershadingin1dhistograms"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
                    
                    MainModel.RenderShadingIn1DHistograms = renderShadingIn1DHistograms;

                    MainModel.Ip = ip;

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
            var installedLoc = Package.Current.InstalledLocation;
            
            string mainConifgContent = await installedLoc.GetFileAsync(@"Assets\data\main.ini").AsTask().ContinueWith(t => Windows.Storage.FileIO.ReadTextAsync(t.Result)).Result;

            var startDataSet = mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("startdataset"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            var throttle = double.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("throttle"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
            var sampleSize = double.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("samplesize"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

            CatalogCommand catalogCommand = new CatalogCommand();
            Catalog catalog = await catalogCommand.GetCatalog();
            
            _mainModel.DatasetConfigurations.Clear();
            foreach (var schema in catalog.Schemas)
            {
                var dataSetConfig = new DatasetConfiguration
                {
                    Schema = schema,
                    Backend = "progressive",
                    ThrottleInMillis = throttle,
                    SampleSize = sampleSize
                };
                _mainModel.DatasetConfigurations.Add(dataSetConfig);
            }

            if (_mainModel.DatasetConfigurations.Any(ds => ds.Schema.DisplayName.ToLower().Contains(startDataSet)))
            {
                LoadData(_mainModel.DatasetConfigurations.First(ds => ds.Schema.DisplayName.ToLower().Contains(startDataSet)));
            }
            else
            {
                LoadData(_mainModel.DatasetConfigurations.First());
            }
            
            /*TaskGroupModel parent = new TaskGroupModel();
            foreach (var child in catalog.SupportedOperations)
            {
                if (child.ToString() != "")
                    recursiveCreateAttributeModels(child, parent);
            }
            _mainModel.TaskModels = parent.TaskModels.ToList();*/
        }

        public void LoadData(DatasetConfiguration datasetConfiguration)
        {
            if (_mainModel.SchemaModel != null && _mainModel.SchemaModel.QueryExecuter != null)
            {
                _mainModel.SchemaModel.QueryExecuter.HaltAllJobs();
            }

            if (datasetConfiguration.Backend.ToLower() == "progressive")
            {
                _mainModel.SchemaModel = new ProgressiveSchemaModel();
                _mainModel.ThrottleInMillis = datasetConfiguration.ThrottleInMillis;
                _mainModel.SampleSize = datasetConfiguration.SampleSize;
                (_mainModel.SchemaModel as ProgressiveSchemaModel).QueryExecuter = new IDEAQueryExecuter();
                (_mainModel.SchemaModel as ProgressiveSchemaModel).RootOriginModel = new ProgressiveOriginModel(datasetConfiguration);
                (_mainModel.SchemaModel as ProgressiveSchemaModel).RootOriginModel.LoadInputFields();
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

        public HistogramOperationViewModel CreateDefaultHistogramOperationViewModel(AttributeModel attributeModel )
        {
            HistogramOperationViewModel visModel = OperationViewModelFactory.CreateDefaultHistogramOperationViewModel(_mainModel.SchemaModel, attributeModel);
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

            //newOperationViewModel.OperationModel.FireOperationModelUpdated(HistogramModelUpdatedEventType.Structure);
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
            _mainModel.SchemaModel.QueryExecuter.RemoveJob((operationContainerView.DataContext as OperationViewModel).OperationModel);
            _operationViewModels.Remove(operationContainerView.DataContext as HistogramOperationViewModel);
            //PhysicsController.Instance.RemovePhysicalObject(operationContainerView);
            MainViewController.Instance.InkableScene.Remove(operationContainerView);

            operationContainerView.Dispose();
            foreach (var attachmentView in MainViewController.Instance.InkableScene.Elements.Where(e => e is AttachmentView).ToList())
            {
                if ((attachmentView.DataContext as AttachmentViewModel).OperationViewModel == operationContainerView.DataContext as HistogramOperationViewModel)
                {
                    (attachmentView as AttachmentView).Dispose();
                    MainViewController.Instance.InkableScene.Remove(attachmentView);
                }
            }
            var qm = (operationContainerView.DataContext as HistogramOperationViewModel).HistogramOperationModel;
            foreach (var model in qm.LinkModels.ToArray())
            {
                ((IFilterConsumerOperationModel) model.FromOperationModel).LinkModels.Remove(model);
                ((IFilterConsumerOperationModel)model.ToOperationModel).LinkModels.Remove(model);
            }
        }

        void TaskModelMoved(object sender, TaskModelEventArgs e)
        {
            
        }

        void TaskModelDropped(object sender, TaskModelEventArgs e)
        {
            double width = HistogramOperationViewModel.WIDTH;
            double height = HistogramOperationViewModel.HEIGHT;
            Vec size = new Vec(width, height);
            Pt position = (Pt)new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size / 2.0;

            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<OperationContainerView> hits = new List<OperationContainerView>();
            foreach (var element in InkableScene.Elements.Where(ele => ele is OperationContainerView).Select(ele => ele as OperationContainerView))
            {
                var geom = element.GetBounds(InkableScene).GetPolygon();
                if (geom != null && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }

            bool found = false;
            foreach (var element in hits)
            {
                if ((element.DataContext as ClassificationOperationViewModel).ClassificationOperationModel.TaskModel != null)
                {
                    (element.DataContext as ClassificationOperationViewModel).ClassificationOperationModel.TaskModel = (sender as TaskModel);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                OperationContainerView operationContainerView = new OperationContainerView();
                //TODO
                /*HistogramOperationViewModel histogramOperationViewModel = CreateHistogramOperationViewModel((sender as TaskModel), null);
                histogramOperationViewModel.Position = position;
                histogramOperationViewModel.Size = size;
                operationContainerView.DataContext = histogramOperationViewModel;
                InkableScene.Add(operationContainerView);*/
            }
        }

        void VisualizationTypeViewModelMoved(object sender, VisualizationTypeViewModelEventArgs e)
        {
        }

        void VisualizationTypeViewModelDropped(object sender, VisualizationTypeViewModelEventArgs e)
        {
            double width = HistogramOperationViewModel.WIDTH;
            double height = HistogramOperationViewModel.HEIGHT;
            Vec size = new Vec(width, height);
            Pt position = (Pt)new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size / 2.0;

            OperationContainerView operationContainerView = new OperationContainerView();
            //TODO
                /*HistogramOperationViewModel histogramOperationViewModel = CreateHistogramOperationViewModel(null, (sender as VisualizationTypeViewModel).VisualizationType);
            histogramOperationViewModel.Position = position;
            histogramOperationViewModel.Size = size;
            operationContainerView.DataContext = histogramOperationViewModel;
            InkableScene.Add(operationContainerView);*/
        }

        void InputGroupViewModelMoved(object sender, InputGroupViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
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
            }
        }


        void InputGroupViewModelDropped(object sender, InputGroupViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
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
            }
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
                HistogramOperationViewModel histogramOperationViewModel = CreateDefaultHistogramOperationViewModel(e.AttributeTransformationModel.AttributeModel);
                histogramOperationViewModel.Position = position;
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
            OperationModel model = sender as OperationModel;
            if (e is FilterOperationModelUpdatedEventArgs && 
                (e as FilterOperationModelUpdatedEventArgs).FilterOperationModelUpdatedEventType == FilterOperationModelUpdatedEventType.Links)
            {
                ((HistogramOperationModel) model).ClearFilterModels();
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
                    FilterLinkViewController.Instance.CreateFilterLinkViewModel(connect.FromOperationViewModel.OperationModel, connect.ToOperationViewModel.OperationModel);
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
                    }
                }
            }

            if (recognizedGestures.Count == 0)// && e.InkStroke.IsErase)
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
                        /*Polygon p = new Polygon();
                        PointCollection pc = new PointCollection(existingScribbable.Geometry.Coordinates.Select(c => new System.Windows.Point(c.X, c.Y)));
                        p.Points = pc;
                        p.Stroke = Brushes.Blue;
                        p.StrokeThickness = 5;
                        _inkableScene.Add(p);*/

                        if (inkStrokeLine.Intersects(geom))
                        {
                            //existingScribbable.Consume(e.InkStroke);
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
