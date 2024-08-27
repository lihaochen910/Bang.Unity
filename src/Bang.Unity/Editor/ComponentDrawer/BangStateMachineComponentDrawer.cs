using System;
using System.Reflection;
using Bang.Components;
using Bang.StateMachines;


namespace Bang.Unity.Editor.ComponentDrawer {

	public class BangStateMachineComponentDrawer : IComponentDrawer {

		public bool CanHandlesType( Type type ) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof( StateMachineComponent<> );

		public IComponent DrawComponent( IComponent component ) {
			var routineField = component.GetType().GetField( "_routine", BindingFlags.NonPublic | BindingFlags.Instance );
			var routine = routineField.GetValue( component );
			if ( routine is null ) {
				return component;
			}
			
			var memberInfos = routine.GetType().GetPublicMemberInfos();
			foreach ( var info in memberInfos ) {
				var memberValue = info.GetValue( routine );
				var memberType = memberValue == null ? info.Type : memberValue.GetType();
				if ( EntityDrawer.DrawObjectMember( memberType, info.Name, memberValue, routine, info.SetValue ) ) {
					
				}
			}

			return component;
		}
	}

}
