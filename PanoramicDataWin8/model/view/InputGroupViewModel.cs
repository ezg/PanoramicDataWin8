﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class InputGroupViewModel : BindableBase
    {
        public static event EventHandler<InputGroupViewModelEventArgs> InputGroupViewModelMoved;
        public static event EventHandler<InputGroupViewModelEventArgs> InputGroupViewModelDropped;

        public InputGroupViewModel()
        {
        }

        public InputGroupViewModel(VisualizationViewModel visualizationViewModel, InputGroupModel inputGroupModel)
        {
            VisualizationViewModel = visualizationViewModel;
            InputOperationModel = inputGroupModel;
        }

        private string _mainLabel = null;
        public string MainLabel
        {
            get
            {
                return _mainLabel;
            }
            set
            {
                this.SetProperty(ref _mainLabel, value);
            }
        }

        private VisualizationViewModel _visualizationViewModel = null;
        public VisualizationViewModel VisualizationViewModel
        {
            get
            {
                return _visualizationViewModel;
            }
            set
            {
                this.SetProperty(ref _visualizationViewModel, value);
            }
        }

        private InputGroupModel _inpuGroupModel = null;
        public InputGroupModel InputOperationModel
        {
            get
            {
                return _inpuGroupModel;
            }
            set
            {
                this.SetProperty(ref _inpuGroupModel, value);
                if (_inpuGroupModel != null)
                {
                    updateLabels();
                }
            }
        }

        private bool _isShadow = false;
        public bool IsShadow
        {
            get
            {
                return _isShadow;
            }
            set
            {
                this.SetProperty(ref _isShadow, value);
            }
        }

        private Vec _size = new Vec(50, 50);
        public Vec Size
        {
            get
            {
                return _size;
            }
            set
            {
                this.SetProperty(ref _size, value);
            }
        }


        public void FireMoved(Rct bounds, InputGroupModel inputGroupModel)
        {
            if (InputGroupViewModelMoved != null)
            {
                InputGroupViewModelMoved(this, new InputGroupViewModelEventArgs(bounds, inputGroupModel));
            }
        }

        public void FireDropped(Rct bounds, InputGroupModel inputGroupModel)
        {
            if (InputGroupViewModelDropped != null)
            {
                InputGroupViewModelDropped(this, new InputGroupViewModelEventArgs(bounds, inputGroupModel));
            }
        }

        private void updateLabels()
        {
            MainLabel = _inpuGroupModel.RawName.Replace("_", "");
        }
    }



    public interface InputGroupViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void InputGroupViewModelMoved(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement);
        void InputGroupViewModelDropped(InputGroupViewModel sender, InputGroupViewModelEventArgs e, bool overElement);
    }


    public class InputGroupViewModelEventArgs : EventArgs
    {
        public InputGroupModel InputGroupModel { get; set; }
        public Rct Bounds { get; set; }
        public InputGroupViewModelEventArgs(Rct bounds, InputGroupModel inputGroupModel)
        {
            Bounds = bounds;
            InputGroupModel = inputGroupModel;
        }
    }
}
