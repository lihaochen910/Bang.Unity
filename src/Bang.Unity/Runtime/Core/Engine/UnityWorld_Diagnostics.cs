using System.Collections.Generic;
using Bang.Diagnostics;


namespace Bang.Unity {

	public partial class UnityWorld {
		
		public readonly Dictionary<int, SmoothCounter> DrawCounters = new();
		public readonly Dictionary<int, SmoothCounter> DrawGuiCounters = new();
		
	}

}
