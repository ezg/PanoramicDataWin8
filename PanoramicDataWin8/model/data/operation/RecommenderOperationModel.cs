using IDEA_common.operations;
using IDEA_common.operations.risk;
using Newtonsoft.Json;
using PanoramicDataWin8.model.data.attribute;
using System.Collections.ObjectModel;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class RecommenderOperationModel : OperationModel
    {
        private ModelId _modelId;
        private int _page;
        private double _budget;

        private int _pageSize = 9;

        public RecommenderOperationModel(OriginModel schemaModel) : base(schemaModel)
        {
        }

        public override void Dispose()
        {
        }

        public double Budget
        {
            get { return _budget; }
            set { SetProperty(ref _budget, value); }
        }


        private HistogramOperationModel _target = null;
        public HistogramOperationModel Target
        {
            get { return _target; }
            set { SetProperty(ref _target, value); }
        }

        private ObservableCollection<AttributeModel> _include = new ObservableCollection<AttributeModel>();

        public ObservableCollection<AttributeModel> Include
        {
            get { return _include; }
            set { SetProperty(ref _include, value); }
        }

        private ObservableCollection<AttributeModel> _exclude = new ObservableCollection<AttributeModel>();

        public ObservableCollection<AttributeModel> Exlude
        {
            get { return _exclude; }
            set { SetProperty(ref _exclude, value); }
        }


        public ModelId ModelId
        {
            get { return _modelId; }
            set { SetProperty(ref _modelId, value); }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { SetProperty(ref _pageSize, value); }
        }

        public int Page
        {
            get { return _page; }
            set { SetProperty(ref _page, value); }
        }

        public override ResultParameters ResultParameters => new RecommenderResultParameters
        {
            From = _pageSize * _page,
            To = _pageSize * _page + _pageSize
        };
    }
}