using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public class FilterLinkModel : ExtendedBindableBase
    {
        private IFilterProviderOperationModel _fromOperationModel = null;
        public IFilterProviderOperationModel FromOperationModel
        {
            get
            {
                return _fromOperationModel;
            }
            set
            {
                this.SetProperty(ref _fromOperationModel, value);
            }
        }

        private IFilterConsumerOperationModel _toOperationModel = null;
        public IFilterConsumerOperationModel ToOperationModel
        {
            get
            {
                return _toOperationModel;
            }
            set
            {
                this.SetProperty(ref _toOperationModel, value);
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
            if (obj is FilterLinkModel)
            {
                var link = obj as FilterLinkModel;
                return
                    link.FromOperationModel.Equals(this.FromOperationModel) &&
                    link.ToOperationModel.Equals(this.ToOperationModel) &&
                    link.LinkType.Equals(this.LinkType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int code = 0;
            code ^= this.FromOperationModel.GetHashCode();
            code ^= this.ToOperationModel.GetHashCode();
            code ^= this.LinkType.GetHashCode();
            return code;
        }
    }
}
