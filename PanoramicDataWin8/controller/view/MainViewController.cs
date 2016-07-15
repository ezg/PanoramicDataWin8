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
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.data.progressive;
using PanoramicDataWin8.controller.data.sim;
using PanoramicDataWin8.controller.data.tuppleware;
using PanoramicDataWin8.controller.data.tuppleware.gateway;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.progressive;
using PanoramicDataWin8.model.data.sim;
using PanoramicDataWin8.model.data.tuppleware;
using PanoramicDataWin8.model.view;
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
        private DispatcherTimer _visualizationMovingTimer = new DispatcherTimer();

        private MainViewController(InkableScene root, MainPage mainPage)
        {
            _root = root;
            _mainPage = mainPage;

            _mainModel = new MainModel();
            
            InputFieldViewModel.InputFieldViewModelDropped += InputFieldViewModelDropped;
            InputFieldViewModel.InputFieldViewModelMoved += InputFieldViewModelMoved;

            InputGroupViewModel.InputGroupViewModelDropped += InputGroupViewModelDropped;
            InputGroupViewModel.InputGroupViewModelMoved += InputGroupViewModelMoved;

            TaskModel.JobTypeViewModelDropped += TaskModelDropped;
            TaskModel.JobTypeViewModelMoved += TaskModelMoved;

            VisualizationTypeViewModel.VisualizationTypeViewModelDropped += VisualizationTypeViewModel_VisualizationTypeViewModelDropped;
            VisualizationTypeViewModel.VisualizationTypeViewModelMoved += VisualizationTypeViewModel_VisualizationTypeViewModelMoved;

            _root.InkCollectedEvent += root_InkCollectedEvent;
            VisualizationViewModels.CollectionChanged += VisualizationViewModels_CollectionChanged;
            InputVisualizationViews.CollectionChanged += InputVisualizationViews_CollectionChanged;
            ComparisonViews.CollectionChanged += ComparisonViews_CollectionChanged;

            _gesturizer.AddGesture(new ConnectGesture(_root));
            _gesturizer.AddGesture(new EraseGesture(_root));
            //_gesturizer.AddGesture(new ScribbleGesture(_root));

            _visualizationMovingTimer.Interval = TimeSpan.FromMilliseconds(20);
            _visualizationMovingTimer.Tick += _visualizationMovingTimer_Tick;
            _visualizationMovingTimer.Start();
        }

        public async void LoadConfigs()
        {
            var installedLoc = Package.Current.InstalledLocation;
            var configLoc = await installedLoc.GetFolderAsync(@"Assets\data\config");
            string mainConifgContent = await installedLoc.GetFileAsync(@"Assets\data\main.ini").AsTask().ContinueWith(t => Windows.Storage.FileIO.ReadTextAsync(t.Result)).Result;
            var backend = mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("backend"))
                .Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            var startDataSet = mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("startdataset"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            _mainModel.DatasetConfigurations.Clear();
            if (backend.ToLower() == "sim")
            {
                var configs = await configLoc.GetFilesAsync();
                foreach (var file in configs)
                {
                    var content = await Windows.Storage.FileIO.ReadTextAsync(file);
                    _mainModel.DatasetConfigurations.Add(DatasetConfiguration.FromContent(content, file.Name));
                }
                if (_mainModel.DatasetConfigurations.Any(ds => ds.Name.ToLower().Contains(startDataSet)))
                {
                    LoadData(_mainModel.DatasetConfigurations.First(ds => ds.Name.ToLower().Contains(startDataSet)));
                }
                else
                {
                    LoadData(_mainModel.DatasetConfigurations.First(ds => ds.Name.ToLower().Contains("nba")));
                }
            }
            else if (backend.ToLower() == "tuppleware")
            {
                try
                {
                    var ip = mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("ip"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                    var throttle = double.Parse(mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("throttle"))
                        .Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
                    var nrRecords = int.Parse(mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("nrofrecords"))
                        .Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
                    var sampleSize = double.Parse(mainConifgContent.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("samplesize"))
                        .Split(new string[] {"="}, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

                    var showCodeGen = bool.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("showcodegen"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

                    var renderShadingIn1DHistograms = bool.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("rendershadingin1dhistograms"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

                    MainModel.ShowCodeGen = showCodeGen;
                    MainModel.RenderShadingIn1DHistograms = renderShadingIn1DHistograms;

                    MainModel.Ip = ip;
                    CatalogCommand catalogCommand = new CatalogCommand();
                    var loadedDatasetConfigs = await catalogCommand.GetCatalog(MainModel.Ip);
                    foreach (var ds in loadedDatasetConfigs)
                    {
                        ds.ThrottleInMillis = throttle;
                        ds.SampleSize = sampleSize;
                        ds.NrOfRecords = nrRecords;
                        _mainModel.DatasetConfigurations.Add(ds);
                    }
                    if (_mainModel.DatasetConfigurations.Any(ds => ds.Name.ToLower().Contains(startDataSet)))
                    {
                        LoadData(_mainModel.DatasetConfigurations.First(ds => ds.Name.ToLower().Contains(startDataSet)));
                    }
                    else
                    {
                        LoadData(_mainModel.DatasetConfigurations.First());
                    }

                    TasksCommand tasksCommand = new TasksCommand();
                    var loadedTasks = await tasksCommand.GetTasks(backend);
                    _mainModel.TaskModels = loadedTasks;
                }
                catch (Exception exc)
                {
                    ErrorHandler.HandleError(exc.Message);
                }
            }
            else if (backend.ToLower() == "progressive")
            {
                try
                {
                    var ip = mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("ip"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                    var showCodeGen = bool.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("showcodegen"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

                    var renderShadingIn1DHistograms = bool.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .First(l => l.ToLower().StartsWith("rendershadingin1dhistograms"))
                        .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

                    MainModel.ShowCodeGen = showCodeGen;
                    MainModel.RenderShadingIn1DHistograms = renderShadingIn1DHistograms;

                    MainModel.Ip = ip;

                    LoadCatalog();

                    string message = await ProgressiveGateway.Request(new JObject(new JProperty("type", "tasks")));
                    parsePgTaskResponse(message);

                }
                catch (Exception exc)
                {
                    ErrorHandler.HandleError(exc.Message);
                }
            }
        }

        private void parsePgTaskResponse(string e)
        {
            JToken jToken = JToken.Parse(e);

            List<TaskModel> tasks = new List<TaskModel>();
            TaskGroupModel parent = new TaskGroupModel();
            foreach (var child in jToken as JArray)
            {
                if (child.ToString() != "")
                    recursiveCreateAttributeModels(child, parent);
            }
            _mainModel.TaskModels = parent.TaskModels.ToList();
        }

        private void recursiveCreateAttributeModels(JToken token, TaskGroupModel parent)
        {
            if (token is JArray)
            {
                if (token[0] is JValue)
                {
                    TaskGroupModel groupModel = new TaskGroupModel() { Name = token[0].ToString() };
                    parent.TaskModels.Add(groupModel);
                    foreach (var child in token[1])
                    {
                        recursiveCreateAttributeModels(child, groupModel);
                    }
                }
            }
            if (token is JValue)
            {
                TaskModel taskModel = new TaskModel() { Name = token.ToString() };
                parent.TaskModels.Add(taskModel);
            }
        }

        public async void LoadCatalog()
        {
            string message = await ProgressiveGateway.Request(new JObject(new JProperty("type", "catalog")));
            parsePgCatalogResponse(message);
        }

        private async void parsePgCatalogResponse(string e)
        {
            var installedLoc = Package.Current.InstalledLocation;
            var configLoc = await installedLoc.GetFolderAsync(@"Assets\data\config");
            string mainConifgContent = await installedLoc.GetFileAsync(@"Assets\data\main.ini").AsTask().ContinueWith(t => Windows.Storage.FileIO.ReadTextAsync(t.Result)).Result;
          
            var startDataSet = mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("startdataset"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            var throttle = double.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("throttle"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
            var nrRecords = int.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("nrofrecords"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
            var sampleSize = double.Parse(mainConifgContent.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First(l => l.ToLower().StartsWith("samplesize"))
                .Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());

            JToken jToken = JToken.Parse(e );

            List<DatasetConfiguration> dataSets = new List<DatasetConfiguration>();
            foreach (var child in jToken)
            {
                var schemaName = ((JProperty)child).Name;
                var schemaJson = ((JProperty)child).Value;
                var dataSetConfig = new DatasetConfiguration
                {
                    Name = schemaName,
                    SchemaJson = schemaJson,
                    Backend = "progressive",
                    BaseUUID = ((JProperty)child).Value["uuid"].Value<string>()
                };
                dataSets.Add(dataSetConfig);
            }
            _mainModel.DatasetConfigurations.Clear();
            foreach (var ds in dataSets)
            {
                ds.ThrottleInMillis = throttle;
                ds.SampleSize = sampleSize;
                ds.NrOfRecords = nrRecords;
                _mainModel.DatasetConfigurations.Add(ds);
            }
            if (_mainModel.DatasetConfigurations.Any(ds => ds.Name.ToLower().Contains(startDataSet)))
            {
                LoadData(_mainModel.DatasetConfigurations.First(ds => ds.Name.ToLower().Contains(startDataSet)));
            }
            else
            {
                LoadData(_mainModel.DatasetConfigurations.First());
            }
        }

        public static void CreateInstance(InkableScene root, MainPage mainPage)
        {
            _instance = new MainViewController(root, mainPage);
            _instance.LoadConfigs();
        }
        
        public static MainViewController Instance
        {
            get
            {
                return _instance;
            }
        }

        private InkableScene _root;
        public InkableScene InkableScene
        {
            get
            {
                return _root;
            }
        }

        public ObservableDictionary<InputVisualizationViewModel, InputVisualizationView> InputVisualizationViews = new ObservableDictionary<InputVisualizationViewModel, InputVisualizationView>();

        public ObservableDictionary<ComparisonViewModel, ComparisonView> ComparisonViews = new ObservableDictionary<ComparisonViewModel, ComparisonView>();


        private ObservableCollection<VisualizationViewModel> _visualizationViewModels = new ObservableCollection<VisualizationViewModel>();
        public ObservableCollection<VisualizationViewModel> VisualizationViewModels
        {
            get
            {
                return _visualizationViewModels;
            }
        }

        private ObservableCollection<LinkViewModel> _linkViewModels = new ObservableCollection<LinkViewModel>();
        public ObservableCollection<LinkViewModel> LinkViewModels
        {
            get
            {
                return _linkViewModels;
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

        public void LoadData(DatasetConfiguration datasetConfiguration)
        {
            if (_mainModel.SchemaModel != null && _mainModel.SchemaModel.QueryExecuter != null)
            {
                _mainModel.SchemaModel.QueryExecuter.HaltAllJobs();
            }

            if (datasetConfiguration.Backend.ToLower() == "mssql")
            {
                _mainModel.SchemaModel = null; //new MSSQLSchemaModel(datasetConfiguration);
            }
            else if (datasetConfiguration.Backend.ToLower() == "sim")
            {
                _mainModel.SchemaModel = new SimSchemaModel();
                _mainModel.ThrottleInMillis = datasetConfiguration.ThrottleInMillis;
                _mainModel.SampleSize = datasetConfiguration.SampleSize;
                (_mainModel.SchemaModel as SimSchemaModel).QueryExecuter = new SimQueryExecuter();
                (_mainModel.SchemaModel as SimSchemaModel).RootOriginModel = new SimOriginModel(datasetConfiguration);
                (_mainModel.SchemaModel as SimSchemaModel).RootOriginModel.LoadInputFields();
            }
            else if (datasetConfiguration.Backend.ToLower() == "tuppleware")
            {
                _mainModel.SchemaModel = new TuppleWareSchemaModel();
                _mainModel.ThrottleInMillis = datasetConfiguration.ThrottleInMillis;
                _mainModel.SampleSize = datasetConfiguration.SampleSize;
                (_mainModel.SchemaModel as TuppleWareSchemaModel).QueryExecuter = new TuppleWareQueryExecuter();
                (_mainModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel = new TuppleWareOriginModel(datasetConfiguration);
                (_mainModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel.LoadInputFields();
            }
            else if (datasetConfiguration.Backend.ToLower() == "progressive")
            {
                _mainModel.SchemaModel = new ProgressiveSchemaModel();
                _mainModel.ThrottleInMillis = datasetConfiguration.ThrottleInMillis;
                _mainModel.SampleSize = datasetConfiguration.SampleSize;
                (_mainModel.SchemaModel as ProgressiveSchemaModel).QueryExecuter = new ProgressiveQueryExecuter();
                (_mainModel.SchemaModel as ProgressiveSchemaModel).RootOriginModel = new ProgressiveOriginModel(datasetConfiguration);
                (_mainModel.SchemaModel as ProgressiveSchemaModel).RootOriginModel.LoadInputFields();
            }
        }
        public VisualizationViewModel CreateVisualizationViewModel(TaskModel taskModel, InputOperationModel inputOperationModel)
        {
            VisualizationViewModel visModel = VisualizationViewModelFactory.CreateDefault(_mainModel.SchemaModel, taskModel, inputOperationModel != null ? inputOperationModel.InputModel : null);
            addAttachmentViews(visModel);
            _visualizationViewModels.Add(visModel);
            return visModel;
        }

        public VisualizationViewModel CreateVisualizationViewModel(TaskModel taskModel, VisualizationType visualizationType)
        {
            VisualizationViewModel visModel = VisualizationViewModelFactory.CreateDefault(_mainModel.SchemaModel, taskModel, visualizationType);
            addAttachmentViews(visModel);
            _visualizationViewModels.Add(visModel);
            return visModel;
        }

        public void CopyVisualisationViewModel(VisualizationViewModel visualizationViewModel, Pt centerPoint)
        {
            VisualizationContainerView visualizationContainerView = new VisualizationContainerView();
            VisualizationViewModel newVisualizationViewModel = CreateVisualizationViewModel(visualizationViewModel.QueryModel.TaskModel, null);
            
            newVisualizationViewModel.Position = centerPoint - (visualizationViewModel.Size / 2.0);
            newVisualizationViewModel.Size = visualizationViewModel.Size;
            foreach (var usage in visualizationViewModel.QueryModel.UsageInputOperationModels.Keys)
            {
                foreach (var inputOperationModel in visualizationViewModel.QueryModel.UsageInputOperationModels[usage])
                {
                    newVisualizationViewModel.QueryModel.AddUsageInputOperationModel(usage, 
                        new InputOperationModel(inputOperationModel.InputModel)
                        {
                            AggregateFunction = inputOperationModel.AggregateFunction
                        });
                }
            }
            newVisualizationViewModel.Size = visualizationViewModel.Size;
            newVisualizationViewModel.QueryModel.VisualizationType = visualizationViewModel.QueryModel.VisualizationType;

            visualizationContainerView.DataContext = newVisualizationViewModel;
            InkableScene.Add(visualizationContainerView);

            newVisualizationViewModel.QueryModel.FireQueryModelUpdated(QueryModelUpdatedEventType.Structure);
        }

        private void addAttachmentViews(VisualizationViewModel visModel)
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


        public void RemoveVisualizationViewModel(VisualizationContainerView visualizationContainerView)
        {
            _mainModel.SchemaModel.QueryExecuter.RemoveJob((visualizationContainerView.DataContext as VisualizationViewModel).QueryModel);
            _visualizationViewModels.Remove(visualizationContainerView.DataContext as VisualizationViewModel);
            //PhysicsController.Instance.RemovePhysicalObject(visualizationContainerView);
            MainViewController.Instance.InkableScene.Remove(visualizationContainerView);

            visualizationContainerView.Dispose();
            foreach (var attachmentView in MainViewController.Instance.InkableScene.Elements.Where(e => e is AttachmentView).ToList())
            {
                if ((attachmentView.DataContext as AttachmentViewModel).VisualizationViewModel == visualizationContainerView.DataContext as VisualizationViewModel)
                {
                    (attachmentView as AttachmentView).Dispose();
                    MainViewController.Instance.InkableScene.Remove(attachmentView);
                }
            }
            var qm = (visualizationContainerView.DataContext as VisualizationViewModel).QueryModel;
            foreach (var model in qm.LinkModels.ToArray())
            {
                model.FromQueryModel.LinkModels.Remove(model);
                model.ToQueryModel.LinkModels.Remove(model);
            }
        }

        public LinkViewModel CreateLinkViewModel(LinkModel linkModel)
        {
            LinkViewModel linkViewModel = LinkViewModels.FirstOrDefault(lvm => lvm.ToVisualizationViewModel == VisualizationViewModels.Where(vvm => vvm.QueryModel == linkModel.ToQueryModel).First());
            if (linkViewModel == null)
            {
                linkViewModel = new LinkViewModel()
                {
                    ToVisualizationViewModel = VisualizationViewModels.Where(vvm => vvm.QueryModel == linkModel.ToQueryModel).First(),
                };
                _linkViewModels.Add(linkViewModel);
                LinkView linkView = new LinkView();
                linkView.DataContext = linkViewModel;
                _root.AddToBack(linkView);
            }
            if (!linkViewModel.LinkModels.Contains(linkModel))
            {
                linkViewModel.LinkModels.Add(linkModel);
                linkViewModel.FromVisualizationViewModels.Add(VisualizationViewModels.Where(vvm => vvm.QueryModel == linkModel.FromQueryModel).First());
            }

            return linkViewModel;
        }

        private bool isLinkAllowed(LinkModel linkModel)
        {
            List<LinkModel> linkModels = linkModel.FromQueryModel.LinkModels.Where(lm => lm.FromQueryModel == linkModel.FromQueryModel).ToList();
            linkModels.Add(linkModel);
            return !recursiveCheckForCiruclarLinking(linkModels, linkModel.FromQueryModel, new HashSet<QueryModel>());
        } 

        private bool recursiveCheckForCiruclarLinking(List<LinkModel> links, QueryModel current, HashSet<QueryModel> chain)
        {
            if (!chain.Contains(current))
            {
                chain.Add(current);
                bool ret = false;
                foreach (var link in links)
                {
                    ret = ret || recursiveCheckForCiruclarLinking(link.ToQueryModel.LinkModels.Where(lm => lm.FromQueryModel == link.ToQueryModel).ToList(), link.ToQueryModel, chain);
                }
                return ret;
            }
            else
            {
                return true;
            }
        }

        public void RemoveLinkViewModel(LinkModel linkModel)
        {
            foreach (var linkViewModel in LinkViewModels.ToArray()) 
            {
                if (linkViewModel.LinkModels.Contains(linkModel))
                {
                    linkViewModel.LinkModels.Remove(linkModel);
                }
                if (linkViewModel.LinkModels.Count == 0)
                {
                    LinkViewModels.Remove(linkViewModel);
                    _root.Remove(_root.Elements.First(e => e is LinkView && (e as LinkView).DataContext == linkViewModel));
                }
            }
        }

        void TaskModelMoved(object sender, TaskModelEventArgs e)
        {
            
        }

        void TaskModelDropped(object sender, TaskModelEventArgs e)
        {
            double width = VisualizationViewModel.WIDTH;
            double height = VisualizationViewModel.HEIGHT;
            Vec size = new Vec(width, height);
            Pt position = (Pt)new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size / 2.0;

            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<VisualizationContainerView> hits = new List<VisualizationContainerView>();
            foreach (var element in InkableScene.Elements.Where(ele => ele is VisualizationContainerView).Select(ele => ele as VisualizationContainerView))
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
                if ((element.DataContext as VisualizationViewModel).QueryModel.TaskModel != null)
                {
                    (element.DataContext as VisualizationViewModel).QueryModel.TaskModel = (sender as TaskModel);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                VisualizationContainerView visualizationContainerView = new VisualizationContainerView();
                VisualizationViewModel visualizationViewModel = CreateVisualizationViewModel((sender as TaskModel), null);
                visualizationViewModel.Position = position;
                visualizationViewModel.Size = size;
                visualizationContainerView.DataContext = visualizationViewModel;
                InkableScene.Add(visualizationContainerView);
            }
        }

        void VisualizationTypeViewModel_VisualizationTypeViewModelMoved(object sender, VisualizationTypeViewModelEventArgs e)
        {
        }

        void VisualizationTypeViewModel_VisualizationTypeViewModelDropped(object sender, VisualizationTypeViewModelEventArgs e)
        {
            double width = VisualizationViewModel.WIDTH;
            double height = VisualizationViewModel.HEIGHT;
            Vec size = new Vec(width, height);
            Pt position = (Pt)new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size / 2.0;

            VisualizationContainerView visualizationContainerView = new VisualizationContainerView();
            VisualizationViewModel visualizationViewModel = CreateVisualizationViewModel(null, (sender as VisualizationTypeViewModel).VisualizationType);
            visualizationViewModel.Position = position;
            visualizationViewModel.Size = size;
            visualizationContainerView.DataContext = visualizationViewModel;
            InkableScene.Add(visualizationContainerView);
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
        
        void InputFieldViewModelMoved(object sender, InputFieldViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<InputFieldViewModelEventHandler> hits = new List<InputFieldViewModelEventHandler>();
            var tt = InkableScene.GetDescendants().OfType<InputFieldViewModelEventHandler>().ToList();
            foreach (var element in tt)
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom)) 
                {
                    hits.Add(element);
                }
            }
            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();

            foreach (var element in InkableScene.GetDescendants().OfType<InputFieldViewModelEventHandler>())
            {
                element.InputFieldViewModelMoved(
                        sender as InputFieldViewModel, e,
                        hits.Count() > 0 && orderderHits[0] == element);
            }
        }

        void InputFieldViewModelDropped(object sender, InputFieldViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<InputFieldViewModelEventHandler> hits = new List<InputFieldViewModelEventHandler>();
            foreach (var element in InkableScene.GetDescendants().OfType<InputFieldViewModelEventHandler>())
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }

            double width = e.UseDefaultSize ? VisualizationViewModel.WIDTH : e.Bounds.Width;
            double height = e.UseDefaultSize ? VisualizationViewModel.HEIGHT : e.Bounds.Height;
            Vec size = new Vec(width, height);
            Pt position = (Pt) new Vec(e.Bounds.Center.X, e.Bounds.Center.Y) - size / 2.0;

            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();
            foreach (var element in InkableScene.GetDescendants().OfType<InputFieldViewModelEventHandler>())
            {
                element.InputFieldViewModelDropped(
                        sender as InputFieldViewModel, e,
                        hits.Count() > 0 && orderderHits[0] == element);
            }

            if (!hits.Any())
            {
                VisualizationContainerView visualizationContainerView = new VisualizationContainerView();
                VisualizationViewModel visualizationViewModel = CreateVisualizationViewModel(null, e.InputOperationModel);
                visualizationViewModel.Position = position;
                visualizationViewModel.Size = size;
                visualizationContainerView.DataContext = visualizationViewModel;
                InkableScene.Add(visualizationContainerView);
            }
        }

        void VisualizationViewModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    (item as VisualizationViewModel).QueryModel.LinkModels.CollectionChanged -= LinkModels_CollectionChanged;
                    (item as VisualizationViewModel).PropertyChanged -= VisualizationViewModel_PropertyChanged;
                    foreach (var link in (item as VisualizationViewModel).QueryModel.LinkModels)
                    {
                        RemoveLinkViewModel(link);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    (item as VisualizationViewModel).PropertyChanged += VisualizationViewModel_PropertyChanged;
                    (item as VisualizationViewModel).QueryModel.LinkModels.CollectionChanged += LinkModels_CollectionChanged;
                }
            }
        }

        public void VisualizationViewTapped(VisualizationViewModel visModel)
        {
            foreach (var inputVisualizationView in InputVisualizationViews)
            {
                if (visModel.QueryModel.TaskModel == null && inputVisualizationView.Key.VisualizationViewModels.Contains(visModel))
                {
                    inputVisualizationView.Key.From = visModel;
                }
            }
        }
        
        private void InputVisualizationViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var current = (((KeyValuePair<InputVisualizationViewModel, InputVisualizationView>)item).Key);
                    foreach (var visualizationViewModel in current.VisualizationViewModels)
                    {
                        visualizationViewModel.QueryModel.InputVisualizationViewModels.Remove(current);
                    }
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var current = (((KeyValuePair< InputVisualizationViewModel, InputVisualizationView>)item).Key);
                    foreach (var visualizationViewModel in current.VisualizationViewModels)
                    {
                        visualizationViewModel.QueryModel.InputVisualizationViewModels.Add(current);
                    }
                }
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
                        visualizationViewModel.QueryModel.ComparisonViewModels.Remove(current);
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
                        visualizationViewModel.QueryModel.ComparisonViewModels.Add(current);
                    }
                }
            }
        }

        private double boundHorizontalDistance(Rct b1, Rct b2)
        {
            return Math.Min(Math.Abs(b1.Right - b2.Left), Math.Abs(b1.Left - b2.Right));
        }

        
        private Dictionary<VisualizationViewModel, DateTime> _lastMoved = new Dictionary<VisualizationViewModel, DateTime>(); 
        private void VisualizationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var current = sender as VisualizationViewModel;
            if (e.PropertyName == current.GetPropertyName(() => current.Position))
            {

                // update last moved time
                _lastMoved[current] = DateTime.Now;

                // check if we need to create new inputvisualization views
                foreach (var other in VisualizationViewModels.Where(c => c != null))
                {
                    var diff = current.Position - other.Position;

                    bool areLinked = false;
                    foreach (var linkModel in current.QueryModel.LinkModels)
                    {
                        if ((linkModel.FromQueryModel == current.QueryModel && linkModel.ToQueryModel == other.QueryModel) ||
                            (linkModel.FromQueryModel == other.QueryModel && linkModel.ToQueryModel == current.QueryModel))
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
                                List<InputVisualizationViewModel> inputCohorts = InputVisualizationViews.Keys.Where(icv => icv.To == other).ToList();

                                var allColorIndex = Enumerable.Range(0, InputVisualizationViewModel.ColorScheme1.Count);
                                allColorIndex = allColorIndex.Except(inputCohorts.Select(c => c.ColorIndex));
                                var colorIndex = inputCohorts.Count%InputVisualizationViewModel.ColorScheme1.Count;
                                if (allColorIndex.Any())
                                {
                                    colorIndex = allColorIndex.First();
                                }

                                InputVisualizationViewModel inputVisualizationViewModel = new InputVisualizationViewModel();
                                inputVisualizationViewModel.ColorIndex = colorIndex;
                                inputVisualizationViewModel.Color = InputVisualizationViewModel.ColorScheme1[colorIndex];
                                inputVisualizationViewModel.VisualizationViewModels.Add(other);
                                inputVisualizationViewModel.VisualizationViewModels.Add(current);
                                inputVisualizationViewModel.Position =
                                    (((inputVisualizationViewModel.VisualizationViewModels.Aggregate(new Vec(), (a, b) => a + b.Bounds.Center.GetVec()))/2.0) - inputVisualizationViewModel.Size/2.0).GetWindowsPoint();

                                inputVisualizationViewModel.InputVisualizationViewModelState = InputVisualizationViewModelState.Opening;
                                inputVisualizationViewModel.DwellStartPosition = current.Position;
                                inputVisualizationViewModel.From = current;
                                inputVisualizationViewModel.TicksSinceDwellStart = DateTime.Now.Ticks;

                                InputVisualizationView view = new InputVisualizationView();
                                view.DataContext = inputVisualizationViewModel;
                                InkableScene.Children.Add(view);
                                InputVisualizationViews.Add(inputVisualizationViewModel, view);
                            }
                            else
                            {
                                var inputModel = InputVisualizationViews.Keys.First(sov => sov.VisualizationViewModels.Contains(current) && sov.VisualizationViewModels.Contains(other));
                                inputModel.From = current;
                            }
                        }
                    }
                }
            }
        }

        void _visualizationMovingTimer_Tick(object sender, object e)
        {
            checkOpenOrCloseInputVisualizationModels();
            checkOpenOrCloseComparisionModels();
        }

        private void checkOpenOrCloseComparisionModels(bool dropped = false)
        {
            // views that need to be opened or closed
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
                foreach (var linkModel in comparisonViewModel.VisualizationViewModels.First().QueryModel.LinkModels)
                {
                    if ((linkModel.FromQueryModel == comparisonViewModel.VisualizationViewModels[0].QueryModel && linkModel.ToQueryModel == comparisonViewModel.VisualizationViewModels[1].QueryModel) ||
                        (linkModel.FromQueryModel == comparisonViewModel.VisualizationViewModels[1].QueryModel && linkModel.ToQueryModel == comparisonViewModel.VisualizationViewModels[0].QueryModel))
                    {
                        areLinked = true;
                    }
                }


                // Views to close
                if (areLinked ||
                    Math.Abs(diff.Y) >= 300 ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opening && boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) >= 300) ||
                    (comparisonViewModel.ComparisonViewModelState == ComparisonViewModelState.Opened && boundHorizontalDistance(comparisonViewModel.VisualizationViewModels[0].Bounds, comparisonViewModel.VisualizationViewModels[1].Bounds) >= 300) ||
                    comparisonViewModel.VisualizationViewModels.Any(c => !VisualizationViewModels.Contains(c)))
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
            }
        }

        private void checkOpenOrCloseInputVisualizationModels(bool dropped = false)
        {
            // views that need to be opened or closed
            foreach (var inputVisualizationViewModel in InputVisualizationViews.Keys.ToList())
            {
                var model = inputVisualizationViewModel;

                var diff = inputVisualizationViewModel.VisualizationViewModels[0].Position - inputVisualizationViewModel.VisualizationViewModels[1].Position;

                // views to open
                if (Math.Abs(diff.Y) < 300 &&
                    boundHorizontalDistance(inputVisualizationViewModel.VisualizationViewModels[0].Bounds, inputVisualizationViewModel.VisualizationViewModels[1].Bounds) < 50 &&
                    (dropped || DateTime.Now.Ticks > TimeSpan.TicksPerSecond * 1 + model.TicksSinceDwellStart))
                {
                    inputVisualizationViewModel.InputVisualizationViewModelState = InputVisualizationViewModelState.Opened;
                }

                bool areLinked = false;
                foreach (var linkModel in inputVisualizationViewModel.From.QueryModel.LinkModels)
                {
                    if ((linkModel.FromQueryModel == inputVisualizationViewModel.VisualizationViewModels[0].QueryModel && linkModel.ToQueryModel == inputVisualizationViewModel.VisualizationViewModels[1].QueryModel) ||
                        (linkModel.FromQueryModel == inputVisualizationViewModel.VisualizationViewModels[1].QueryModel && linkModel.ToQueryModel == inputVisualizationViewModel.VisualizationViewModels[0].QueryModel))
                    {
                        areLinked = true;
                    }
                }


                // Views to close
                if (areLinked ||
                    Math.Abs(diff.Y) >= 300 ||
                    (inputVisualizationViewModel.InputVisualizationViewModelState == InputVisualizationViewModelState.Opening && boundHorizontalDistance(inputVisualizationViewModel.VisualizationViewModels[0].Bounds, inputVisualizationViewModel.VisualizationViewModels[1].Bounds) >= 50) ||
                    (inputVisualizationViewModel.InputVisualizationViewModelState == InputVisualizationViewModelState.Opened && boundHorizontalDistance(inputVisualizationViewModel.VisualizationViewModels[0].Bounds, inputVisualizationViewModel.VisualizationViewModels[1].Bounds) >= 50) ||
                    inputVisualizationViewModel.VisualizationViewModels.Any(c => !VisualizationViewModels.Contains(c)))
                {
                    inputVisualizationViewModel.InputVisualizationViewModelState = InputVisualizationViewModelState.Closing;
                    var view = InputVisualizationViews[inputVisualizationViewModel];
                    InputVisualizationViews.Remove(inputVisualizationViewModel);

                    var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
                    dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        InkableScene.Children.Remove(view);
                    });

                }
            }
        }

        void LinkModels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    RemoveLinkViewModel(item as LinkModel);
                }
            }
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    CreateLinkViewModel(item as LinkModel);
                }
            }
        }

        void root_InkCollectedEvent(object sender, InkCollectedEventArgs e)
        {
            IList<IGesture> recognizedGestures = _gesturizer.Recognize(e.InkStroke.Clone());

            foreach (IGesture recognizedGesture in recognizedGestures.ToList())
            {
                if (recognizedGesture is ConnectGesture)
                {
                    ConnectGesture connect = recognizedGesture as ConnectGesture;

                    LinkModel linkModel = new LinkModel()
                    {
                        FromQueryModel = connect.FromVisualizationViewModel.QueryModel,
                        ToQueryModel = connect.ToVisualizationViewModel.QueryModel
                    };
                    if (isLinkAllowed(linkModel))
                    {
                        if (!linkModel.FromQueryModel.LinkModels.Contains(linkModel) &&
                            !linkModel.ToQueryModel.LinkModels.Contains(linkModel))
                        {
                            linkModel.FromQueryModel.LinkModels.Add(linkModel);
                            linkModel.ToQueryModel.LinkModels.Add(linkModel);
                        }
                    }
                    else
                    {
                       ErrorHandler.HandleError("Link cycles are not supported."); 
                    }
                }
                else if (recognizedGesture is HitGesture)
                {
                    HitGesture hitGesture = recognizedGesture as HitGesture;
                    foreach (IScribbable hitScribbable in hitGesture.HitScribbables)
                    {
                        if (hitScribbable is InkStroke)
                        {
                            _root.Remove(hitScribbable as InkStroke);
                        }
                        else if (hitScribbable is VisualizationContainerView)
                        {
                            RemoveVisualizationViewModel(hitScribbable as VisualizationContainerView);
                        }
                        else if (hitScribbable is LinkView)
                        {
                            List<LinkModel> models = (hitScribbable as LinkView).GetLinkModelsToRemove(e.InkStroke.Geometry);
                            foreach (var model in models)
                            {
                                model.FromQueryModel.LinkModels.Remove(model);
                                model.ToQueryModel.LinkModels.Remove(model);
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
                    _root.Add(e.InkStroke);
                }
            }
        }

        public void UpdateJobStatus()
        {
            foreach (var current in VisualizationViewModels.ToArray())
            {
                // check if we need to halt or resume the job
                var tg = InkableScene.TransformToVisual(MainPage);
                var tt = tg.TransformBounds(current.Bounds);

                if (!MainPage.GetBounds().IntersectsWith(tt) && _mainModel.SchemaModel.QueryExecuter.IsJobRunning(current.QueryModel))
                {
                    _mainModel.SchemaModel.QueryExecuter.HaltJob(current.QueryModel);
                }
                else if (MainPage.GetBounds().IntersectsWith(tt) &&
                         !_mainModel.SchemaModel.QueryExecuter.IsJobRunning(current.QueryModel))
                {
                    _mainModel.SchemaModel.QueryExecuter.ResumeJob(current.QueryModel);
                }
            }
        }
    }
}
