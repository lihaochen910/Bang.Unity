using System;
using System.Reflection;
using Bang.Components;
using Bang.Interactions;


namespace Bang.Unity.Editor.ComponentDrawer {
	
	public class BangInteractiveComponentDrawer : IComponentDrawer {

		public bool CanHandlesType( Type type ) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof( InteractiveComponent<> );

		public IComponent DrawComponent( IComponent component ) {
			var interactionField = component.GetType().GetField( "_interaction", BindingFlags.NonPublic | BindingFlags.Instance );
			var interaction = interactionField.GetValue( component );
			if ( interaction is null ) {
				return component;
			}

			var changed = false;
			
			var memberInfos = interaction.GetType().GetPublicMemberInfos();
			foreach ( var info in memberInfos ) {
				var memberValue = info.GetValue( interaction );
				var memberType = memberValue == null ? info.Type : memberValue.GetType();
				if ( EntityDrawer.DrawObjectMember( memberType, info.Name, memberValue, interaction, info.SetValue ) ) {
					changed = true;
				}
			}

			if ( changed ) {
				interactionField.SetValue( component, interaction );
			}

			return component;
		}
	}
	
}
