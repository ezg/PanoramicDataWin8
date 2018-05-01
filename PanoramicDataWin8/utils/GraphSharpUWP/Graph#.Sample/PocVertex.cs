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
		public string ID
		{
			get;
			private set;
		}

		public PocVertex( string id )
		{
			ID = id;
		}

        public override string ToString()
        {
            return "philomena" + ID;
        }
    }
}