using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GraphSharp.Sample
{
	public class PocGraphViewModel : INotifyPropertyChanged
	{
		private string layoutAlgorithmType;
		private PocGraph graph;

		public string LayoutAlgorithmType
		{
			get { return layoutAlgorithmType; }
			set {
				if (value != layoutAlgorithmType)
				{
					layoutAlgorithmType = value;
					NotifyChanged("LayoutAlgorithmType");
				}
			}
		}

        public IEnumerable<Controls.AlgorithmConstraints> algorithmConstraintEnum
        {
            get {
                return Enum.GetValues(typeof(Controls.AlgorithmConstraints)).Cast<Controls.AlgorithmConstraints>();
            }
        }


		public PocGraph Graph
		{
			get { return graph; }
			set {
				if (value != graph)
				{
					graph = value;
					NotifyChanged("Graph");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyChanged(string propertyName)
		{
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
        }
	}
}