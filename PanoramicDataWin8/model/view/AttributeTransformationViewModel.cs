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

namespace PanoramicDataWin8.model.view
{
    public class AttributeTransformationViewModel : ExtendedBindableBase
    {
        private AttachmentOrientation _attachmentOrientation;

        private AttributeTransformationModel _attributeTransformationModel;

        private Thickness _borderThicknes;

        private bool _hideAggregationFunction;

        private bool _isDraggable = true;

        private bool _isDraggableByPen;

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

        public AttributeTransformationViewModel()
        {
        }

        public AttributeTransformationViewModel(OperationViewModel operationViewModel, AttributeTransformationModel attributeTransformationModel)
        {
            OperationViewModel = operationViewModel;
            AttributeTransformationModel = attributeTransformationModel;
        }

        public AttachmentOrientation AttachmentOrientation
        {
            get { return _attachmentOrientation; }
            set { SetProperty(ref _attachmentOrientation, value); }
        }

        public double TextAngle
        {
            get { return _textAngle; }
            set { SetProperty(ref _textAngle, value); }
        }

        public bool HideAggregationFunction
        {
            get { return _hideAggregationFunction; }
            set
            {
                SetProperty(ref _hideAggregationFunction, value);
                updateLabels();
            }
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

        public bool IsDraggable
        {
            get { return _isDraggable; }
            set { SetProperty(ref _isDraggable, value); }
        }

        public bool IsDraggableByPen
        {
            get { return _isDraggableByPen; }
            set { SetProperty(ref _isDraggableByPen, value); }
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

        public AttributeTransformationModel AttributeTransformationModel
        {
            get { return _attributeTransformationModel; }
            set
            {
                if (_attributeTransformationModel != null)
                    _attributeTransformationModel.PropertyChanged -= AttributeTransformationModelPropertyChanged;
                SetProperty(ref _attributeTransformationModel, value);
                if (_attributeTransformationModel != null)
                {
                    _attributeTransformationModel.PropertyChanged += AttributeTransformationModelPropertyChanged;
                    updateLabels();
                }
            }
        }

        public static event EventHandler<AttributeTransformationViewModelEventArgs> AttributeTransformationViewModelMoved;
        public static event EventHandler<AttributeTransformationViewModelEventArgs> AttributeTransformationViewModelDropped;

        public void FireMoved(Rct bounds, AttributeTransformationModel attributeTransformationModel)
        {
            AttributeTransformationViewModelMoved?.Invoke(this, new AttributeTransformationViewModelEventArgs(attributeTransformationModel, bounds));
        }

        public void FireDropped(Rct bounds, AttributeTransformationModel attributeTransformationModel)
        {
            AttributeTransformationViewModelDropped?.Invoke(this, new AttributeTransformationViewModelEventArgs(attributeTransformationModel, bounds));
        }

        private void AttributeTransformationModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateLabels();
        }

        private void updateLabels()
        {
            if (_attributeTransformationModel != null)
            {
                MainLabel = _attributeTransformationModel.AttributeModel.DisplayName; //columnDescriptor.GetLabels(out mainLabel, out subLabel);

                var mainLabel = _attributeTransformationModel.AttributeModel.DisplayName;
                var subLabel = "";

                if (!HideAggregationFunction)
                {
                    mainLabel = addDetailToLabel(mainLabel);
                    MainLabel = mainLabel?.Replace("_", " ");
                }
                else
                {
                    mainLabel = mainLabel;
                }
                SubLabel = subLabel;
            }
        }

        private string addDetailToLabel(string name)
        {
            if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Avg)
                name = "avg(" + name + ")";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Count)
                name = "count";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Max)
                name = "max(" + name + ")";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Min)
                name = "min(" + name + ")";
            else if (AttributeTransformationModel.AggregateFunction == AggregateFunction.Sum)
                name = "sum(" + name + ")";
            /*else if (AttributeTransformationViewModel.AggregateFunction == AggregateFunction.Bin)
            {
                name = "Bin Range(" + name + ")";
            }*/

            if (AttributeTransformationModel.ScaleFunction != ScaleFunction.None)
                if (AttributeTransformationModel.ScaleFunction == ScaleFunction.Log)
                    name += " [Log]";
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.Normalize)
                    name += " [Normalize]";
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.RunningTotal)
                    name += " [RT]";
                else if (AttributeTransformationModel.ScaleFunction == ScaleFunction.RunningTotalNormalized)
                    name += " [RT Norm]";
            return name;
        }
    }
}