using System;


namespace Bang.Unity.Editor {

	public interface ITypeDrawer {
		
		bool CanHandlesType(Type type);

		object DrawAndGetNewValue(Type memberType, string memberName, object value, object target);
		
	}
	
}
