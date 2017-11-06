using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using IDEA_common.aggregates;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.attribute;
using PanoramicDataWin8.model.view.operation;
using PanoramicDataWin8.utils;
using Windows.UI.Xaml.Media;

namespace PanoramicDataWin8.model.view
{
    public class AttributeViewModel : MenuItemViewModel
    {
        private AttachmentOrientation _attachmentOrientation;

        private AttributeTransformationModel _attributeTransformationModel;

        private AttributeModel _attributeModel;

        private Thickness _borderThicknes;

        private bool _isFiltered;

        private bool _isGestureEnabled;

        private bool _isHighlighted;

        private bool _isMenuEnabled = true;

        private bool _isNoChrome;

        private bool _isRemoveEnabled;

        private bool _isScaleFunctionEnabled = true;

        private bool _isShadow;

        private string _mainLabel;

        private OperationViewModel _operationViewModel;

        private Vec _size = new Vec(50, 50);

        private string _sublabel;

        private double _textAngle;

        public AttributeViewModel() { }

        public AttributeViewModel(OperationViewModel operationViewModel, AttributeModel attributeModel)
        {
            OperationViewModel = operationViewModel;
            _attributeModel= attributeModel;
            _attributeTransformationModel = new AttributeTransformationModel(attributeModel);
            updateLabels();
        }
        public AttributeViewModel(OperationViewModel operationViewModel, AttributeTransformationModel attributeTransformationModel)
        {
            OperationViewModel = operationViewModel;
            _attributeModel = attributeTransformationModel?.AttributeModel;
            _attributeTransformationModel = attributeTransformationModel;
            updateLabels();
        }

        public AttributeTransformationModel AttributeTransformationModel
        {
            get { return _attributeTransformationModel; }
            set
            {
                if (_attributeTransformationModel != null)
                    _attributeTransformationModel.PropertyChanged -= (sender, e) => updateLabels();
                SetProperty(ref _attributeTransformationModel, value);
                if (_attributeTransformationModel != null)
                {
                    _attributeTransformationModel.PropertyChanged += (sender, e) => updateLabels();
                    updateLabels();
                }
            }
        }

        public virtual AttributeViewModel Clone()
        {
            return new AttributeViewModel(OperationViewModel, AttributeTransformationModel);
        }
 
        public AttachmentOrientation AttachmentOrientation
        {
            get { return _attachmentOrientation; }
            set { SetProperty(ref _attachmentOrientation, value); }
        }

        public virtual Brush HighlightBrush => Application.Current.Resources.MergedDictionaries[0]["backgroundBrush"] as SolidColorBrush;

        public virtual Brush NormalBrush => Application.Current.Resources.MergedDictionaries[0]["highlightBrush"] as SolidColorBrush;

        public double TextAngle
        {
            get { return _textAngle; }
            set { SetProperty(ref _textAngle, value); }
        }

        public bool IsShadow
        {
            get { return _isShadow; }
            set { SetProperty(ref _isShadow, value); }
        }

        public Thickness BorderThicknes
        {
            get { return _borderThicknes; }
            set { SetProperty(ref _borderThicknes, value); }
        }

        public bool IsNoChrome
        {
            get { return _isNoChrome; }
            set { SetProperty(ref _isNoChrome, value); }
        }

        public bool IsFiltered
        {
            get { return _isFiltered; }
            set { SetProperty(ref _isFiltered, value); }
        }

        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { SetProperty(ref _isHighlighted, value); }
        }

        public bool IsMenuEnabled
        {
            get { return _isMenuEnabled; }
            set { SetProperty(ref _isMenuEnabled, value); }
        }

        public bool IsScaleFunctionEnabled
        {
            get { return _isScaleFunctionEnabled; }
            set { SetProperty(ref _isScaleFunctionEnabled, value); }
        }

        public bool IsRemoveEnabled
        {
            get { return _isRemoveEnabled; }
            set { SetProperty(ref _isRemoveEnabled, value); }
        }

        public Vec Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public bool IsGestureEnabled
        {
            get { return _isGestureEnabled; }
            set { SetProperty(ref _isGestureEnabled, value); }
        }

        public string MainLabel
        {
            get { return _mainLabel; }
            set { SetProperty(ref _mainLabel, value); }
        }

        public string SubLabel
        {
            get { return _sublabel; }
            set { SetProperty(ref _sublabel, value); }
        }

        public OperationViewModel OperationViewModel
        {
            get { return _operationViewModel; }
            set { SetProperty(ref _operationViewModel, value); }
        }

        public AttributeModel AttributeModel
        {
            get { return _attributeModel; }
            set
            {
                if (_attributeModel != null)
                    _attributeModel.PropertyChanged -= (sender, e) => updateLabels();
                SetProperty(ref _attributeModel, value);
                if (_attributeModel != null)
                {
                    _attributeModel.PropertyChanged += (sender, e) => updateLabels();
                    updateLabels();
                }
            }
        }

        public static event EventHandler<AttributeViewModelEventArgs> AttributeViewModelMoved;
        public static event EventHandler<AttributeViewModelEventArgs> AttributeViewModelDropped;

        public void FireMoved(Rct bounds, AttributeViewModel attributeViewModel)
        {
            AttributeViewModelMoved?.Invoke(this, new AttributeViewModelEventArgs(attributeViewModel, bounds));
        }

        public void FireDropped(Rct bounds, AttributeViewModel attributeViewModel)
        {
            AttributeViewModelDropped?.Invoke(this, new AttributeViewModelEventArgs(attributeViewModel, bounds));
        }
        protected virtual void updateLabels()
        {
            if (_attributeModel != null)
            {
                if (AttributeTransformationModel != null)
                     MainLabel = AttributeTransformationModel.GetLabel();
                else MainLabel = AttributeModel.DisplayName; 
            }
        }
    }
    public class AttributeGroupViewModel : AttributeViewModel
    {
        public AttributeGroupViewModel(OperationViewModel operationViewModel, AttributeModel attributeGroupModel) :
            base(operationViewModel, attributeGroupModel)
        {
        }

        public override Brush HighlightBrush => Application.Current.Resources.MergedDictionaries[0]["darkBrush"] as SolidColorBrush;

        public override Brush NormalBrush => Application.Current.Resources.MergedDictionaries[0]["darkBrush"] as SolidColorBrush;

        public override AttributeViewModel Clone()
        {
            return new AttributeGroupViewModel(OperationViewModel, AttributeModel);
        }
    }
}