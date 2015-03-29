using GeoAPI.Geometries;
using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.controller.view;
using PanoramicData.model.data;
using PanoramicData.utils;
using PanoramicDataWin8.model.view;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.view
{
    public class VisualizationTypeViewModel : BindableBase
    {
        public static event EventHandler<VisualizationTypeViewModelEventArgs> VisualizationTypeViewModelMoved;
        public static event EventHandler<VisualizationTypeViewModelEventArgs> VisualizationTypeViewModelDropped;

        public VisualizationTypeViewModel() { }


        public void FireMoved(Rct bounds)
        {
            if (VisualizationTypeViewModelMoved != null)
            {
                VisualizationTypeViewModelMoved(this, new VisualizationTypeViewModelEventArgs(bounds));
            }
        }

        public void FireDropped(Rct bounds)
        {
            if (VisualizationTypeViewModelDropped != null)
            {
                VisualizationTypeViewModelDropped(this, new VisualizationTypeViewModelEventArgs(bounds));
            }
        }

        private VisualizationType _visualizationType;
        public VisualizationType VisualizationType
        {
            get
            {
                return _visualizationType;
            }
            set
            {
                _mainLabel = value.ToString();
                this.SetProperty(ref _visualizationType, value);
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

        private string _sublabel = null;
        public string SubLabel
        {
            get
            {
                return _sublabel;
            }
            set
            {
                this.SetProperty(ref _sublabel, value);
            }
        }
    }



    public interface VisualizationTypeViewModelEventHandler
    {
        IGeometry BoundsGeometry { get; }
        void VisualizationTypeViewModelMoved(VisualizationTypeViewModel sender, VisualizationTypeViewModelEventArgs e, bool overElement);
        void VisualizationTypeViewModelDropped(VisualizationTypeViewModel sender, VisualizationTypeViewModelEventArgs e, bool overElement);
    }


    public class VisualizationTypeViewModelEventArgs : EventArgs
    {
        public Rct Bounds { get; set; }
        public VisualizationTypeViewModelEventArgs(Rct bounds)
        {
            Bounds = bounds;
        }
    }
}
