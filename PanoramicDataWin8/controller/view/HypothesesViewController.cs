using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PanoramicDataWin8.model.view;

namespace PanoramicDataWin8.controller.view
{
    public class HypothesesViewController
    {
        private static HypothesesViewController _instance;

        private HypothesesViewController()
        {
        }

        public static HypothesesViewController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new HypothesesViewController();
                return _instance;
            }
        }

        private HypothesesViewModel _hypothesesViewModel = new HypothesesViewModel();
        public HypothesesViewModel HypothesesViewModel
        {
            get
            {
                return _hypothesesViewModel;
            }
        }
    }
}
