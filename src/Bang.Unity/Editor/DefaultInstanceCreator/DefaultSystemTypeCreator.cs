using System;


namespace Bang.Unity.Editor {

	public class DefaultSystemTypeCreator : IDefaultInstanceCreator {
		
		public bool CanHandlesType(Type type) => type == typeof( System.Type );

		public object CreateDefault(Type type) => typeof( System.Type );
	}

}
