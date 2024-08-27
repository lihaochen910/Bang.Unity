using System;


namespace Bang.Unity.Editor {

	public class DefaultStringCreator : IDefaultInstanceCreator {
		
		public bool CanHandlesType(Type type) => type == typeof(string);

		public object CreateDefault(Type type) => string.Empty;
		
	}
	
}
