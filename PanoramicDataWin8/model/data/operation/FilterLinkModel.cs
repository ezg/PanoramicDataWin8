using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.data.operation
{
    public class FilterLinkModel : ExtendedBindableBase
    {
        private IFilterProviderOperationModel _fromOperationModel;

        private bool _isInverted;

        private LinkType _linkType = LinkType.Filter;

        private IFilterConsumerOperationModel _toOperationModel;

        public IFilterProviderOperationModel FromOperationModel
        {
            get { return _fromOperationModel; }
            set { SetProperty(ref _fromOperationModel, value); }
        }

        public IFilterConsumerOperationModel ToOperationModel
        {
            get { return _toOperationModel; }
            set { SetProperty(ref _toOperationModel, value); }
        }

        public LinkType LinkType
        {
            get { return _linkType; }
            set { SetProperty(ref _linkType, value); }
        }

        public bool IsInverted
        {
            get { return _isInverted; }
            set { SetProperty(ref _isInverted, value); }
        }

        public override bool Equals(object obj)
        {
            if (obj is FilterLinkModel)
            {
                var link = obj as FilterLinkModel;
                return
                    link.FromOperationModel.Equals(FromOperationModel) &&
                    link.ToOperationModel.Equals(ToOperationModel) &&
                    link.LinkType.Equals(LinkType);
            }
            return false;
        }

        public override int GetHashCode()
        {
            var code = 0;
            code ^= FromOperationModel.GetHashCode();
            code ^= ToOperationModel.GetHashCode();
            code ^= LinkType.GetHashCode();
            return code;
        }
    }
}