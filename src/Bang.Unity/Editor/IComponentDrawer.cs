using System;
using Bang.Components;


namespace Bang.Unity.Editor {

	public interface IComponentDrawer {
		
		bool CanHandlesType(Type type);

		IComponent DrawComponent(IComponent component);
		
	}
	
}
