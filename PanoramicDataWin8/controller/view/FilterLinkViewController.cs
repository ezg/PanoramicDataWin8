using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.operation;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.view.vis;

namespace PanoramicDataWin8.controller.view
{
    public class FilterLinkViewController
    {
        private static FilterLinkViewController _instance = null;
        private FilterLinkViewController() { }
        public static FilterLinkViewController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FilterLinkViewController();
                }
                return _instance;
            }
        }
        private ObservableCollection<FilterLinkViewModel> _filterLinkViewModels = new ObservableCollection<FilterLinkViewModel>();
        public ObservableCollection<FilterLinkViewModel> FilterLinkViewModels
        {
            get
            {
                return _filterLinkViewModels;
            }
        }

        private FilterLinkModel createLinkModel(IOperationModel from, IOperationModel to)
        {
            FilterLinkModel filterLinkModel = null;
            if (from is IFilterProvider && to is IFilterConsumer)
            {
                filterLinkModel = new FilterLinkModel()
                {
                    FromOperationModel = (IFilterProvider) @from,
                    ToOperationModel = (IFilterConsumer) to
                };
                if (isLinkAllowed(filterLinkModel))
                {
                    if (!((IFilterConsumer) filterLinkModel.FromOperationModel).LinkModels.Contains(filterLinkModel) &&
                        !((IFilterConsumer) filterLinkModel.ToOperationModel).LinkModels.Contains(filterLinkModel))
                    {
                        ((IFilterConsumer) filterLinkModel.FromOperationModel).LinkModels.Add(filterLinkModel);
                        ((IFilterConsumer) filterLinkModel.ToOperationModel).LinkModels.Add(filterLinkModel);
                    }
                    return filterLinkModel;
                }
                else
                {
                    ErrorHandler.HandleError("Link cycles are not supported.");
                }
            }
            return null;
        }

        public bool AreOperationViewModelsLinked(OperationViewModel current, OperationViewModel other)
        {
            bool areLinked = false;
            if (current.OperationModel is IFilterConsumer && other.OperationModel is IFilterConsumer)
            {
                foreach (var linkModel in (current.OperationModel as IFilterConsumer).LinkModels)
                {
                    if ((linkModel.FromOperationModel == current.OperationModel && linkModel.ToOperationModel == other.OperationModel) ||
                        (linkModel.FromOperationModel == other.OperationModel && linkModel.ToOperationModel == current.OperationModel))
                    {
                        areLinked = true;
                    }
                }
            }
            return areLinked;
        }


        public FilterLinkViewModel CreateFilterLinkViewModel(IOperationModel from, IOperationModel to)
        {
            FilterLinkModel filterLinkModel = createLinkModel(from, to);
            FilterLinkViewModel filterLinkViewModel = null;
            if (filterLinkModel != null)
            {
                filterLinkViewModel = FilterLinkViewModels.FirstOrDefault(lvm => lvm.ToOperationViewModel == MainViewController.Instance.OperationViewModels.First(vvm => vvm.OperationModel == filterLinkModel.ToOperationModel));
                if (filterLinkViewModel == null)
                {
                    filterLinkViewModel = new FilterLinkViewModel()
                    {
                        ToOperationViewModel = MainViewController.Instance.OperationViewModels.Where(ovm => ovm.OperationModel is IFilterConsumer).First(vvm => vvm.OperationModel == filterLinkModel.ToOperationModel),
                    };
                    _filterLinkViewModels.Add(filterLinkViewModel);
                    FilterLinkView filterLinkView = new FilterLinkView
                    {
                        DataContext = filterLinkViewModel
                    };
                    MainViewController.Instance.InkableScene.AddToBack(filterLinkView);
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
            List<FilterLinkModel> linkModels = ((IFilterConsumer)filterLinkModel.FromOperationModel).LinkModels.Where(lm => lm.FromOperationModel == filterLinkModel.FromOperationModel).ToList();
            linkModels.Add(filterLinkModel);
            return !recursiveCheckForCiruclarLinking(linkModels, (IFilterConsumer)filterLinkModel.FromOperationModel, new HashSet<IFilterConsumer>());
        }

        private bool recursiveCheckForCiruclarLinking(List<FilterLinkModel> links, IFilterConsumer current, HashSet<IFilterConsumer> chain)
        {
            if (!chain.Contains(current))
            {
                chain.Add(current);
                bool ret = false;
                foreach (var link in links)
                {
                    ret = ret || recursiveCheckForCiruclarLinking(((IFilterConsumer)link.ToOperationModel).LinkModels.Where(lm => lm.FromOperationModel == link.ToOperationModel).ToList(),
                        (IFilterConsumer)link.ToOperationModel, chain);
                }
                return ret;
            }
            else
            {
                return true;
            }
        }

        public void RemoveFilterLinkViewModel(FilterLinkModel filterLinkModel)
        {
            (filterLinkModel.FromOperationModel as IFilterConsumer).LinkModels.Remove(filterLinkModel);
            (filterLinkModel.ToOperationModel as IFilterConsumer).LinkModels.Remove(filterLinkModel);
            foreach (var linkViewModel in FilterLinkViewModels.ToArray())
            {
                if (linkViewModel.FilterLinkModels.Contains(filterLinkModel))
                {
                    linkViewModel.FilterLinkModels.Remove(filterLinkModel);
                }
                if (linkViewModel.FilterLinkModels.Count == 0)
                {
                    FilterLinkViewModels.Remove(linkViewModel);
                    MainViewController.Instance.InkableScene.Remove(MainViewController.Instance.InkableScene.Elements.First(e => e is FilterLinkView && (e as FilterLinkView).DataContext == linkViewModel));
                }
            }
        }
    }
}
