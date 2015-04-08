using Microsoft.Practices.Prism.Mvvm;
using PanoramicData.controller.data;
using PanoramicData.utils;
using PanoramicDataWin8.model.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class QueryResultItemModel : ExtendedBindableBase
    {
        public QueryResultItemModel()
        {
        }

        private Dictionary<JobResult, QueryResultItemValueModel> _jobResultValues = new Dictionary<JobResult, QueryResultItemValueModel>();
        public Dictionary<JobResult, QueryResultItemValueModel> JobResultValues
        {
            get
            {
                return _jobResultValues;
            }
            set
            {
                this.SetProperty(ref _jobResultValues, value);
            }
        }

        private Dictionary<AttributeOperationModel, QueryResultItemValueModel> _attributeValues = new Dictionary<AttributeOperationModel, QueryResultItemValueModel>();
        public Dictionary<AttributeOperationModel, QueryResultItemValueModel> AttributeValues
        {
            get
            {
                return _attributeValues;
            }
            set
            {
                this.SetProperty(ref _attributeValues, value);
            }
        }

        private Bin _bin = null;
        public Bin Bin
        {
            get
            {
                return _bin;
            }
            set
            {
                this.SetProperty(ref _bin, value);
            }
        }

        private Dictionary<AttributeOperationModel, double> _partitions = null;
        public Dictionary<AttributeOperationModel, double> Partitions
        {
            get
            {
                return _partitions;
            }
            set
            {
                this.SetProperty(ref _partitions, value);
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                this.SetProperty(ref _isSelected, value);
            }
        }

        private int _rowNumber = -1;
        public int RowNumber
        {
            get
            {
                return _rowNumber;
            }
            set
            {
                this.SetProperty(ref _rowNumber, value);
            }
        }

        public void Update(QueryResultItemModel updateTo)
        {
            this.JobResultValues = updateTo.JobResultValues;
            this.IsSelected = updateTo.IsSelected;
            this.AttributeValues = updateTo.AttributeValues;
            this.RowNumber = updateTo.RowNumber;
            this.Bin = updateTo.Bin;
        }
    }

    
}
