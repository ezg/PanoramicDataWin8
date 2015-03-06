using PanoramicData.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanoramicData.model.data
{
    public class LinkModel : ExtendedBindableBase
    {
        private QueryModel _fromQueryModel = null;
        public QueryModel FromQueryModel
        {
            get
            {
                return _fromQueryModel;
            }
            set
            {
                this.SetProperty(ref _fromQueryModel, value);
            }
        }

        private QueryModel _toQueryModel = null;
        public QueryModel ToQueryModel
        {
            get
            {
                return _toQueryModel;
            }
            set
            {
                this.SetProperty(ref _toQueryModel, value);
            }
        }

        private LinkType _linkType = LinkType.Filter;
        public LinkType LinkType
        {
            get
            {
                return _linkType;
            }
            set
            {
                this.SetProperty(ref _linkType, value);
            }
        }

        private bool _isInverted = false;
        public bool IsInverted
        {
            get
            {
                return _isInverted;
            }
            set
            {
                this.SetProperty(ref _isInverted, value);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is LinkModel)
            {
                var link = obj as LinkModel;
                return
                    link.FromQueryModel.Equals(this.FromQueryModel) &&
                    link.ToQueryModel.Equals(this.ToQueryModel) &&
                    link.LinkType.Equals(this.LinkType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.FromQueryModel.GetHashCode();
            code ^= this.ToQueryModel.GetHashCode();
            code ^= this.LinkType.GetHashCode();
            return code;
        }
    }
}
