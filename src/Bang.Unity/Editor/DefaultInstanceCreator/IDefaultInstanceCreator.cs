using System;


namespace Bang.Unity.Editor {

	public interface IDefaultInstanceCreator {
		bool CanHandlesType(Type type);

		object CreateDefault(Type type);
	}
	
}
