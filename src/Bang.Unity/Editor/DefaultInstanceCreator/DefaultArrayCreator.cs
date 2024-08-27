using System;


namespace Bang.Unity.Editor {

	public class DefaultArrayCreator : IDefaultInstanceCreator {
		
		public bool CanHandlesType(Type type) => type.IsArray;

		public object CreateDefault(Type type) =>
			Array.CreateInstance(type.GetElementType(), new int[type.GetArrayRank()]);
		
	}
	
}
