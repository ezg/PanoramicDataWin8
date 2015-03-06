﻿using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.model.data;
using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.view
{
    public class SchemaViewModel : BindableBase
    {
        public delegate void SchemaViewModelUpdatedHandler(object sender, SchemaViewModelUpdatedEventArgs e);
        public event SchemaViewModelUpdatedHandler SchemaViewModelUpdated;

        private Vec _size = new Vec(180, 300);

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

        private Pt _postion;

        public Pt Position
        {
            get
            {
                return _postion;
            }
            set
            {
                this.SetProperty(ref _postion, value);
            }
        }

        private SchemaModel _schemaModel;

        public SchemaModel SchemaModel
        {
            get
            {
                return _schemaModel;
            }
            set
            {
                this.SetProperty(ref _schemaModel, value);
            }
        }

        protected void fireTableModelUpdated()
        {
            if (SchemaViewModelUpdated != null)
            {
                SchemaViewModelUpdated(this, new SchemaViewModelUpdatedEventArgs());
            }
        }
    }

    public class SchemaViewModelUpdatedEventArgs : EventArgs
    {
        public SchemaViewModelUpdatedEventArgs()
            : base()
        {
        }
    }
}
