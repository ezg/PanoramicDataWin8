using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GraphSharp.Sample
{
	/// <summary>
	/// A simple identifiable vertex.
	/// </summary>
	[DebuggerDisplay( "{ID}" )]
	public class PocVertex 
	{
        public override string ToString()
        {
            return ID;
        }
        public string ID
		{
			get;
			private set;
		}
        public string Output
        {
            get;
            private set;
        }
        public string HyperParamPlaceholder
        {
            get;
            private set;
        }

        public List<string> HyperParams
        {
            get;
            private set;
        }

        public List<string> Params
        {
            get;
            private set;
        }

		public PocVertex( string id, string output, List<string> hyperParams, List<string> arguments )
		{
			ID = id;
            Output = output;
            HyperParams = hyperParams;
            HyperParamPlaceholder = hyperParams.Count == 1 ? hyperParams[0] : hyperParams.Count > 1 ? "(...)" : "";
            Params = arguments;
		}
    }
}