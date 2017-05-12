using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoAPI.Geometries;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using PanoramicDataWin8.view.inq;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class FilterLinkViewController
    {
        private static FilterLinkViewController _instance;

        private FilterLinkViewController()
        {
        }

        public static FilterLinkViewController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FilterLinkViewController();
                return _instance;
            }
        }

        public ObservableCollection<FilterLinkViewModel> FilterLinkViewModels { get; } = new ObservableCollection<FilterLinkViewModel>();

        private FilterLinkModel createLinkModel(IOperationModel from, IOperationModel to)
        {
            FilterLinkModel filterLinkModel = null;
            if (from is IFilterProviderOperationModel && to is IFilterConsumerOperationModel)
            {
                filterLinkModel = new FilterLinkModel
                {
                    FromOperationModel = (IFilterProviderOperationModel) from,
                    ToOperationModel = (IFilterConsumerOperationModel) to
                };
                if (isLinkAllowed(filterLinkModel))
                {
                    if (!filterLinkModel.FromOperationModel.ProviderLinkModels.Contains(filterLinkModel))
                    {
                        filterLinkModel.FromOperationModel.ProviderLinkModels.Add(filterLinkModel);
                    }
                    if (!filterLinkModel.ToOperationModel.ConsumerLinkModels.Contains(filterLinkModel))
                    {
                        filterLinkModel.ToOperationModel.ConsumerLinkModels.Add(filterLinkModel);
                    }
                    return filterLinkModel;
                }
                ErrorHandler.HandleError("Link cycles are not supported.");
            }
            return null;
        }

        public bool AreOperationViewModelsLinked(OperationViewModel current, OperationViewModel other)
        {
            var areLinked = false;
            if (current.OperationModel is IFilterConsumerOperationModel && other.OperationModel is IFilterConsumerOperationModel)
            {
                foreach (var linkModel in (current.OperationModel as IFilterConsumerOperationModel).ConsumerLinkModels)
                {
                    if (((linkModel.FromOperationModel == current.OperationModel) && (linkModel.ToOperationModel == other.OperationModel)) ||
                        ((linkModel.FromOperationModel == other.OperationModel) && (linkModel.ToOperationModel == current.OperationModel)))
                    {
                        areLinked = true;
                    }
                }
            }
            return areLinked;
        }

        public FilterLinkViewModel CreateFilterLinkViewModel(OperationViewModel from, Rct bounds)
        {
            IGeometry inputBounds = bounds.GetPolygon();
            var hits = new List<OperationContainerView>();
            var tt = MainViewController.Instance.InkableScene.GetDescendants().OfType<OperationContainerView>().ToList();
            foreach (var element in tt)
            {
                var geom = element.Geometry;
                if ((geom != null) && geom.Intersects(inputBounds))
                {
                    hits.Add(element);
                }
            }

            if (hits.Any())
            {
                return CreateFilterLinkViewModel(from.OperationModel, ((OperationViewModel)hits.First().DataContext).OperationModel);
            }
            else
            {
                var copyContainer = PanoramicDataWin8.controller.view.MainViewController.Instance.CopyOperationViewModel(from, bounds.Center);
                return CreateFilterLinkViewModel(from.OperationModel, (copyContainer.DataContext as OperationViewModel).OperationModel);
            }
        }

        public FilterLinkViewModel CreateFilterLinkViewModel(IOperationModel from, IOperationModel to)
        {
            var filterLinkModel = createLinkModel(from, to);
            FilterLinkViewModel filterLinkViewModel = null;
            if (filterLinkModel != null)
            {
                filterLinkViewModel =
                    FilterLinkViewModels.FirstOrDefault(
                        lvm => lvm.ToOperationViewModel == MainViewController.Instance.OperationViewModels.First(vvm => vvm.OperationModel == filterLinkModel.ToOperationModel));
                if (filterLinkViewModel == null)
                {
                    filterLinkViewModel = new FilterLinkViewModel
                    {
                        ToOperationViewModel =
                            MainViewController.Instance.OperationViewModels.Where(ovm => ovm.OperationModel is IFilterConsumerOperationModel)
                                .First(vvm => vvm.OperationModel == filterLinkModel.ToOperationModel)
                    };
                    FilterLinkViewModels.Add(filterLinkViewModel);
                    var filterLinkView = new FilterLinkView
                    {
                        DataContext = filterLinkViewModel
                    };
                    MainViewController.Instance.InkableScene.Add(filterLinkView);
                }
                if (!filterLinkViewModel.FilterLinkModels.Contains(filterLinkModel))
                {
                    filterLinkViewModel.FilterLinkModels.Add(filterLinkModel);
                    filterLinkViewModel.FromOperationViewModels.Add(MainViewController.Instance.OperationViewModels.First(vvm => vvm.OperationModel == filterLinkModel.FromOperationModel));
                }
            }
            return filterLinkViewModel;
        }

        private bool isLinkAllowed(FilterLinkModel filterLinkModel)
        {
            if (!(filterLinkModel.FromOperationModel is IFilterConsumerOperationModel)) // bcz: if source is a filter not a histogram then we can add it
                return true;
            var linkModels = ((IFilterConsumerOperationModel)filterLinkModel.FromOperationModel).ConsumerLinkModels.Where(lm => lm.FromOperationModel == filterLinkModel.FromOperationModel).ToList();
            linkModels.Add(filterLinkModel);
            var chain = new HashSet<IFilterConsumerOperationModel>();
            recursiveCheckForCiruclarLinking(linkModels, chain);

            if (chain.Contains(filterLinkModel.FromOperationModel as IFilterConsumerOperationModel))
                return false;

            if (filterLinkModel.FromOperationModel is IBrushableOperationModel) {
                var brushModels = ((IBrushableOperationModel)filterLinkModel.FromOperationModel).BrushOperationModels.ToList();
                foreach (var brushableOperationModel in brushModels)
                {
                    foreach (var linkModel in linkModels)
                    {
                        if (brushableOperationModel == linkModel.ToOperationModel)
                        {
                            BrushableViewController.Instance.Remove(brushableOperationModel);
                            return true;
                        }
                    }
                }
            }
            return true;
        }

        private void recursiveCheckForCiruclarLinking(List<FilterLinkModel> links, HashSet<IFilterConsumerOperationModel> chain)
        {
            foreach (var link in links)
            {
                chain.Add(link.ToOperationModel);
                recursiveCheckForCiruclarLinking(link.ToOperationModel.ConsumerLinkModels.Where(lm => lm.FromOperationModel == link.ToOperationModel).ToList(), chain);
            }
        }


        public void RemoveFilterLinkViewModel(FilterLinkModel filterLinkModel)
        {
            if (filterLinkModel is IFilterConsumerOperationModel)  // bcz: if FromOperationModel is a consumer, then remove this from it.
                (filterLinkModel.FromOperationModel as IFilterConsumerOperationModel).ConsumerLinkModels.Remove(filterLinkModel);
            filterLinkModel.ToOperationModel.ConsumerLinkModels.Remove(filterLinkModel);
            foreach (var linkViewModel in FilterLinkViewModels.ToArray())
            {
                if (linkViewModel.FilterLinkModels.Contains(filterLinkModel))
                    linkViewModel.FilterLinkModels.Remove(filterLinkModel);
                if (linkViewModel.FilterLinkModels.Count == 0)
                {
                    FilterLinkViewModels.Remove(linkViewModel);
                    var view = MainViewController.Instance.InkableScene.Elements.First(e => e is FilterLinkView && ((e as FilterLinkView).DataContext == linkViewModel));
                    view.DataContext = null;
                    MainViewController.Instance.InkableScene.Remove(view);
                }
            }
        }
    }
}