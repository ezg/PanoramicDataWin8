using PanoramicDataWin8.utils;

namespace PanoramicDataWin8.model.view
{
    public class BudgetViewModel : ExtendedBindableBase
    {
        private double _budgetToSpend = 0;
        private string _defaultLabel = "h";

        public double BudgetToSpend
        {
            get { return _budgetToSpend; }
            set { SetProperty(ref _budgetToSpend, value); }
        }

        public string DefaultLabel
        {
            get { return _defaultLabel; }
            set { SetProperty(ref _defaultLabel, value); }
        }
    }
}