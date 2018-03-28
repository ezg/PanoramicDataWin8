using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.view;
using PanoramicDataWin8.view.vis;
using PanoramicDataWin8.view.vis.menu;
using System;
using System.Collections.Generic;
namespace PanoramicDataWin8.view.inq
{
    public interface IGesture
    {
        bool Recognize(InkStroke inkStroke);
    }

    public abstract class HitGesture : IGesture
    {
        protected IList<IScribbable> _hitScribbables;
        public IList<IScribbable> HitScribbables
        {
            get { return _hitScribbables; }
        }

        public void ProcessHit(InkStroke stroke)
        {
            foreach (var hitScribbable in HitScribbables)
            {
                if (hitScribbable is InkStroke)
                {
                    MainViewController.Instance.InkableScene.Remove(hitScribbable as InkStroke);
                }
                else if (hitScribbable is OperationContainerView)
                {
                    MainViewController.Instance.RemoveOperationViewModel(hitScribbable as OperationContainerView);
                }
                else if (hitScribbable is FilterLinkView)
                {
                    var models = (hitScribbable as FilterLinkView).GetLinkModelsToRemove(stroke.Geometry);
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

        public virtual bool Recognize(InkStroke inkStroke)
        {
            throw new System.NotImplementedException();
        }
    }
}
