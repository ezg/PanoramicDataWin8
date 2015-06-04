using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.ApplicationModel;
using GeoAPI.Geometries;
using PanoramicDataWin8.controller.data.sim;
using PanoramicDataWin8.controller.data.tuppleware;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.model.data;
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

        private MainViewController(InkableScene root, MainPage mainPage)
        {
            _root = root;
            _mainPage = mainPage;

            _mainModel = new MainModel();

            InputFieldViewModel.InputFieldViewModelDropped += InputFieldViewModelDropped;
            InputFieldViewModel.InputFieldViewModelMoved += InputFieldViewModelMoved;

            InputGroupViewModel.InputGroupViewModelDropped += InputGroupViewModelDropped;
            InputGroupViewModel.InputGroupViewModelMoved += InputGroupViewModelMoved;

            JobTypeViewModel.JobTypeViewModelDropped += JobTypeViewModelDropped;
            JobTypeViewModel.JobTypeViewModelMoved += JobTypeViewModelMoved;

            VisualizationTypeViewModel.VisualizationTypeViewModelDropped += VisualizationTypeViewModel_VisualizationTypeViewModelDropped;
            VisualizationTypeViewModel.VisualizationTypeViewModelMoved += VisualizationTypeViewModel_VisualizationTypeViewModelMoved;

            _root.InkCollectedEvent += root_InkCollectedEvent;
            VisualizationViewModels.CollectionChanged += VisualizationViewModels_CollectionChanged;

            _gesturizer.AddGesture(new ConnectGesture(_root));
            _gesturizer.AddGesture(new EraseGesture(_root));
            _gesturizer.AddGesture(new ScribbleGesture(_root));

            loadConfigs();
        }

        private async void loadConfigs()
        {
            var installedLoc = Package.Current.InstalledLocation;
            var configLoc = await installedLoc.GetFolderAsync(@"Assets\data\config");
            var configs = await configLoc.GetFilesAsync();
            foreach (var file in configs)
            {
                var content = await Windows.Storage.FileIO.ReadTextAsync(file);
                _mainModel.DatasetConfigurations.Add(DatasetConfiguration.FromContent(content, file.Name));
            }
            LoadData(_mainModel.DatasetConfigurations.Where(ds => ds.Name.ToLower().Contains("nba")).First());
            //LoadData(_mainModel.DatasetConfigurations.Where(ds => ds.Name.ToLower().Contains("mimic")).First());
            //LoadData(_mainModel.DatasetConfigurations.First());
        }

        public static void CreateInstance(InkableScene root, MainPage mainPage)
        {
            _instance = new MainViewController(root, mainPage);
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
                TuppleWareGateway.PopulateSchema((_mainModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel);
                //((_mainModel.SchemaModel as TuppleWareSchemaModel).QueryExecuter as TuppleWareQueryExecuter).LoadFileDescription((_mainModel.SchemaModel as TuppleWareSchemaModel).RootOriginModel);
            }
        }
        public VisualizationViewModel CreateVisualizationViewModel(JobType jobType, InputOperationModel inputOperationModel)
        {
            VisualizationViewModel visModel = VisualizationViewModelFactory.CreateDefault(_mainModel.SchemaModel, jobType, inputOperationModel != null ? inputOperationModel.InputModel : null);
            addAttachmentViews(visModel);
            _visualizationViewModels.Add(visModel);
            return visModel;
        }

        public VisualizationViewModel CreateVisualizationViewModel(JobType jobType, VisualizationType visualizationType)
        {
            VisualizationViewModel visModel = VisualizationViewModelFactory.CreateDefault(_mainModel.SchemaModel, jobType, visualizationType);
            addAttachmentViews(visModel);
            _visualizationViewModels.Add(visModel);
            return visModel;
        }

        public void CopyVisualisationViewModel(VisualizationViewModel visualizationViewModel, Pt centerPoint)
        {
            VisualizationContainerView visualizationContainerView = new VisualizationContainerView();
            VisualizationViewModel newVisualizationViewModel = CreateVisualizationViewModel(visualizationViewModel.QueryModel.JobType, null);

            newVisualizationViewModel.Position = centerPoint - (visualizationViewModel.Size / 2.0);
            newVisualizationViewModel.Size = visualizationViewModel.Size;
            foreach (var usage in visualizationViewModel.QueryModel.UsageInputOperationModels.Keys)
            {
                foreach (var inputOperationModel in visualizationViewModel.QueryModel.UsageInputOperationModels[usage])
                {
                    newVisualizationViewModel.QueryModel.AddUsageInputOperationModel(usage, new InputOperationModel(inputOperationModel.InputModel));
                }
            }
            newVisualizationViewModel.Size = visualizationViewModel.Size;

            visualizationContainerView.DataContext = newVisualizationViewModel;
            InkableScene.Add(visualizationContainerView);
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
            _visualizationViewModels.Remove(visualizationContainerView.DataContext as VisualizationViewModel);
            //PhysicsController.Instance.RemovePhysicalObject(visualizationContainerView);
            MainViewController.Instance.InkableScene.Remove(visualizationContainerView);

            foreach (var attachmentView in MainViewController.Instance.InkableScene.Elements.Where(e => e is AttachmentView).ToList())
            {
                if ((attachmentView.DataContext as AttachmentViewModel).VisualizationViewModel == visualizationContainerView.DataContext as VisualizationViewModel)
                {
                    MainViewController.Instance.InkableScene.Remove(attachmentView);
                }
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

        void JobTypeViewModelMoved(object sender, JobTypeViewModelEventArgs e)
        {
            
        }

        void JobTypeViewModelDropped(object sender, JobTypeViewModelEventArgs e)
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
                if ((element.DataContext as VisualizationViewModel).QueryModel.JobType != JobType.DB)
                {
                    (element.DataContext as VisualizationViewModel).QueryModel.JobType = (sender as JobTypeViewModel).JobType;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                VisualizationContainerView visualizationContainerView = new VisualizationContainerView();
                VisualizationViewModel visualizationViewModel = CreateVisualizationViewModel((sender as JobTypeViewModel).JobType, null);
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
            VisualizationViewModel visualizationViewModel = CreateVisualizationViewModel(JobType.DB, (sender as VisualizationTypeViewModel).VisualizationType);
            visualizationViewModel.Position = position;
            visualizationViewModel.Size = size;
            visualizationContainerView.DataContext = visualizationViewModel;
            InkableScene.Add(visualizationContainerView);
        }

        void InputGroupViewModelMoved(object sender, InputGroupViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<InputGroupViewModelEventHandler> hits = new List<InputGroupViewModelEventHandler>();
            foreach (var element in InkableScene.Elements.Where(ele => ele is InputGroupViewModelEventHandler).Select(ele => ele as InputGroupViewModelEventHandler))
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }
            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();

            foreach (var element in InkableScene.Elements.Where(ele => ele is InputGroupViewModelEventHandler).Select(ele => ele as InputGroupViewModelEventHandler))
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
            foreach (var element in InkableScene.Elements.Where(ele => ele is InputGroupViewModelEventHandler).Select(ele => ele as InputGroupViewModelEventHandler))
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom))
                {
                    hits.Add(element);
                }
            }

            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();
            foreach (var element in InkableScene.Elements.Where(ele => ele is InputGroupViewModelEventHandler).Select(ele => ele as InputGroupViewModelEventHandler))
            {
                element.InputGroupViewModelDropped(
                        sender as InputGroupViewModel, e,
                        hits.Count() > 0 ? orderderHits[0] == element : false);
            }
        }
        
        void InputFieldViewModelMoved(object sender, InputFieldViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<InputFieldViewModelEventHandler> hits = new List<InputFieldViewModelEventHandler>();
            foreach (var element in InkableScene.Elements.Where(ele => ele is InputFieldViewModelEventHandler).Select(ele => ele as InputFieldViewModelEventHandler))
            {
                var geom = element.BoundsGeometry;
                if (geom != null && mainPageBounds.Intersects(geom)) 
                {
                    hits.Add(element);
                }
            }
            var orderderHits = hits.OrderBy(fe => (fe.BoundsGeometry.Centroid.GetVec() - e.Bounds.Center.GetVec()).LengthSquared).ToList();

            foreach (var element in InkableScene.Elements.Where(ele => ele is InputFieldViewModelEventHandler).Select(ele => ele as InputFieldViewModelEventHandler))
            {
                element.InputFieldViewModelMoved(
                        sender as InputFieldViewModel, e,
                        hits.Count() > 0 ? orderderHits[0] == element : false);
            }
        }

        void InputFieldViewModelDropped(object sender, InputFieldViewModelEventArgs e)
        {
            IGeometry mainPageBounds = e.Bounds.GetPolygon();
            List<InputFieldViewModelEventHandler> hits = new List<InputFieldViewModelEventHandler>();
            foreach (var element in InkableScene.Elements.Where(ele => ele is InputFieldViewModelEventHandler).Select(ele => ele as InputFieldViewModelEventHandler))
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
            foreach (var element in InkableScene.Elements.Where(ele => ele is InputFieldViewModelEventHandler).Select(ele => ele as InputFieldViewModelEventHandler))
            {
                element.InputFieldViewModelDropped(
                        sender as InputFieldViewModel, e,
                        hits.Count() > 0 ? orderderHits[0] == element : false);
            }

            if (hits.Count() == 0)
            {
                VisualizationContainerView visualizationContainerView = new VisualizationContainerView();
                VisualizationViewModel visualizationViewModel = CreateVisualizationViewModel(JobType.DB, e.InputOperationModel);
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
                    (item as VisualizationViewModel).QueryModel.LinkModels.CollectionChanged += LinkModels_CollectionChanged;
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
                    if (!linkModel.FromQueryModel.LinkModels.Contains(linkModel) &&
                        !linkModel.ToQueryModel.LinkModels.Contains(linkModel))
                    {
                        linkModel.FromQueryModel.LinkModels.Add(linkModel);
                        linkModel.ToQueryModel.LinkModels.Add(linkModel);
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

            if (recognizedGestures.Count == 0)
            {
                _root.Add(e.InkStroke);
            }
        }
    }
}
