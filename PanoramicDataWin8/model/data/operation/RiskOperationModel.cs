using System.Collections.Generic;
using IDEA_common.operations.risk;

namespace PanoramicDataWin8.model.data.operation
{
    public class RiskOperationModel : OperationModel
    {
        private double _alpha = 0.05;

        private ModelId _modelId;

        private List<RiskControlType> _riskControlTypes = new List<RiskControlType>
        {
            RiskControlType.PCER,
            RiskControlType.Bonferroni,
            RiskControlType.AdaBonferroni,
            RiskControlType.HolmBonferroni,
            RiskControlType.BHFDR,
            RiskControlType.SeqFDR,
            RiskControlType.AlphaFDR,
            RiskControlType.BestFootForward,
            RiskControlType.BetaFarsighted,
            RiskControlType.GammaFixed,
            RiskControlType.DeltaHopeful,
            RiskControlType.EpsilonHybrid,
            RiskControlType.PsiSupport
        };

        public RiskOperationModel(SchemaModel schemaModel) : base(schemaModel)
        {
        }

        public ModelId ModelId 
        {
            get { return _modelId; }
            set { SetProperty(ref _modelId, value); }
        }

        public double Alpha
        {
            get { return _alpha; }
            set { SetProperty(ref _alpha, value); }
        }

        public List<RiskControlType> RiskControlTypes
        {
            get { return _riskControlTypes; }
            set { SetProperty(ref _riskControlTypes, value); }
        }
    }
}