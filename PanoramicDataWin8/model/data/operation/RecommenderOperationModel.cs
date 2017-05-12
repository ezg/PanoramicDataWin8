using IDEA_common.operations;
using IDEA_common.operations.risk;
using Newtonsoft.Json;

namespace PanoramicDataWin8.model.data.operation
{
    [JsonObject(MemberSerialization.OptOut)]
    public class RecommenderOperationModel : OperationModel
    {
        private ModelId _modelId;
        private int _page;

        private int _pageSize = 9;

        public RecommenderOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
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