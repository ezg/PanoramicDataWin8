namespace PanoramicDataWin8.model.data.operation
{
    public interface IStatisticallyComparableOperationModel : IOperationModel
    {
        bool IncludeDistribution { get; set; }
    }
}