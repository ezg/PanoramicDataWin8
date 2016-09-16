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
            ComparisonViews.CollectionChanged += ComparisonViews_CollectionChanged;

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
                (_mainModel.SchemaModel as ProgressiveSchemaModel).QueryExecuter = new ProgressiveQueryExecuter();
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

        public ObservableDictionary<ComparisonViewModel, ComparisonView> ComparisonViews = new ObservableDictionary<ComparisonViewModel, ComparisonView>();


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
                ((IFilterConsumer) model.FromOperationModel).LinkModels.Remove(model);
                ((IFilterConsumer)model.ToOperationModel).LinkModels.Remove(model);
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
                    (item as HistogramOperationViewModel).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                    (item as OperationViewModel).OperationModel.OperationModelUpdated -= OperationModel_OperationModelUpdated;
                    foreach (var link in (item as HistogramOperationViewModel).HistogramOperationModel.LinkModels)
                    {
                        FilterLinkViewController.Instance.RemoveFilterLinkViewModel(link);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as HistogramOperationViewModel).PropertyChanged += VisualizationViewModel_PropertyChanged;
                    (item as OperationViewModel).OperationModel.OperationModelUpdated += OperationModel_OperationModelUpdated;
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

        private void ComparisonViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = (((KeyValuePair<ComparisonViewModel, ComparisonView>)item).Key);
                    foreach (var visualizationViewModel in current.VisualizationViewModels)
                    {
                        visualizationViewModel.HistogramOperationModel.ComparisonViewModels.Remove(current);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var current = (((KeyValuePair<ComparisonViewModel, ComparisonView>)item).Key);
                    foreach (var visualizationViewModel in current.VisualizationViewModels)
                    {
                        visualizationViewModel.HistogramOperationModel.ComparisonViewModels.Add(current);
                    }
                }
            }
        }
        
        private Dictionary<HistogramOperationViewModel, DateTime> _lastMoved = new Dictionary<HistogramOperationViewModel, DateTime>(); 
        private void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            /*var current = sender as HistogramOperationViewModel;
            if (e.PropertyName == current.GetPropertyName(() => current.Position))
            {

                // update last moved time
                _lastMoved[current] = DateTime.Now;

                // check if we need to create new inputvisualization views
                foreach (var other in OperationViewModels.Select(c => c as HistogramOperationViewModel).Where(c => c != null))
                {
                    var diff = current.Position - other.Position;

                    bool areLinked = false;
                    foreach (var linkModel in current.HistogramOperationModel.LinkModels)
                    {
                        if ((linkModel.FromOperationModel == current.HistogramOperationModel && linkModel.ToOperationModel == other.HistogramOperationModel) ||
                            (linkModel.FromOperationModel == other.HistogramOperationModel && linkModel.ToOperationModel == current.HistogramOperationModel))
                        {
                            areLinked = true;
                        }
                    }
                    if (!areLinked)
                    {
                        // check for comparision views
                        if (Math.Abs(diff.Y) < 300 &&
                            boundHorizontalDistance(current.Bounds, other.Bounds) < 200 &&
                            _lastMoved.ContainsKey(other) &&
                            other != current &&
                            Math.Abs((_lastMoved[other] - _lastMoved[current]).TotalMilliseconds) < 400)
                        {
                            if (!ComparisonViews.Keys.Any(sov => sov.VisualizationViewModels.Contains(current) && sov.VisualizationViewModels.Contains(other)))
                            {
                                ComparisonViewModel comparisonViewModel = new ComparisonViewModel();
                                comparisonViewModel.VisualizationViewModels.Add(other);
                                comparisonViewModel.VisualizationViewModels.Add(current);
                                comparisonViewModel.Position =
                                    (((comparisonViewModel.VisualizationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec())) / 2.0) - comparisonViewModel.Size / 2.0).GetWindowsPoint();

                                comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Opening;
                                comparisonViewModel.DwellStartPosition = current.Position;
                                comparisonViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                ComparisonView view = new ComparisonView();
                                view.DataContext = comparisonViewModel;
                                InkableScene.Children.Add(view);
                                ComparisonViews.Add(comparisonViewModel, view);
                            }
                        }

                        // check for inputvisualization views
                        else if (Math.Abs(diff.Y) < 300 &&
                            boundHorizontalDistance(current.Bounds, other.Bounds) < 50)
                        {
                            if (!InputVisualizationViews.Keys.Any(sov => sov.VisualizationViewModels.Contains(current) && sov.VisualizationViewModels.Contains(other)))
                            {
                                List<BrushViewModel> inputCohorts = InputVisualizationViews.Keys.Where(icv => icv.To == other).ToList();

                                var allColorIndex = Enumerable.Range(0, BrushViewModel.ColorScheme1.Count);
                                allColorIndex = allColorIndex.Except(inputCohorts.Select(c => c.ColorIndex));
                                var colorIndex = inputCohorts.Count%BrushViewModel.ColorScheme1.Count;
                                if (allColorIndex.Any())
                                {
                                    colorIndex = allColorIndex.First();
                                }

                                BrushViewModel brushViewModel = new BrushViewModel();
                                brushViewModel.ColorIndex = colorIndex;
                                brushViewModel.Color = BrushViewModel.ColorScheme1[colorIndex];
                                brushViewModel.OperationViewModels.Add(other);
                                brushViewModel.OperationViewModels.Add(current);
                                brushViewModel.Position =
                                    (((brushViewModel.OperationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec()))/2.0) - brushViewModel.Size/2.0).GetWindowsPoint();

                                brushViewModel.BrushableOperationViewModelState = BrushableOperationViewModelState.Opening;
                                brushViewModel.DwellStartPosition = current.Position;
                                brushViewModel.From = current;
                                brushViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                BrushView view = new BrushView();
                                view.DataContext = brushViewModel;
                                InkableScene.Children.Add(view);
                                InputVisualizationViews.Add(brushViewModel, view);
                            }
                            else
                            {
                                var inputModel = InputVisualizationViews.Keys.First(sov => sov.VisualizationViewModels.Contains(current) && sov.VisualizationViewModels.Contains(other));
                                inputModel.From = current;
                            }
                        }
                    }
                }
            }*/
        }
        
        private void checkOpenOrCloseComparisionModels(bool dropped = false)
        {
            /*// views that need to be opened or closed
            foreach (var comparisonViewModel in ComparisonViews.Keys.ToList())
            {
                var model = comparisonViewModel;

                var diff = comparisonViewModel.VisualizationViewModels[0].Position - comparisonViewModel.VisualizationViewModels[1].Position;

                // views to open
                if (Math.Abs(diff.Y) < 300 &&
                    boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) < 300 &&
                    (dropped || DateTime.Now.Ticks > TimeSpan.TicksPerSecond*1 + model.TicksSinceDwellStart))
                {
                    comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Opened;
                }

                bool areLinked = false;
                foreach (var linkModel in comparisonViewModel.VisualizationViewModels.First().HistogramOperationModel.LinkModels)
                {
                    if ((linkModel.FromOperationModel == comparisonViewModel.VisualizationViewModels[0].HistogramOperationModel && linkModel.ToOperationModel == comparisonViewModel.VisualizationViewModels[1].HistogramOperationModel) ||
                        (linkModel.FromOperationModel == comparisonViewModel.VisualizationViewModels[1].HistogramOperationModel && linkModel.ToOperationModel == comparisonViewModel.VisualizationViewModels[0].HistogramOperationModel))
                    {
                        areLinked = true;
                    }
                }


                // Views to close
                if (areLinked ||
                    Math.Abs(diff.Y) >= 300 ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opening && boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) >= 300) ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opened && boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) >= 300) ||
                    comparisonViewModel.VisualizationViewModels.Any(c => !OperationViewModels.Contains(c)))
                {
                    comparisonViewModel.ComparisonViewModelState = ComparisonViewModelState.Closing;
                    var view = ComparisonViews[comparisonViewModel];
                    ComparisonViews.Remove(comparisonViewModel);

                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        InkableScene.Children.Remove(view);
                    });

                }
            }*/
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
