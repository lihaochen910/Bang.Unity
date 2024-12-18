using System;


namespace Bang.Unity.Editor {

	public class SystemObjectDrawer : ITypeDrawer {

		public bool CanHandlesType( Type type ) => false;

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {

			// EditorGUI.indentLevel++;
			
			var memberInfos = memberType.GetPublicMemberInfos();
			foreach ( var info in memberInfos ) {
				if ( info.HasAttribute< HideInEditorAttribute >() ) {
					continue;
				}
				
				var subMemberValue = info.GetValue( value );
				var subMemberType = subMemberValue == null ? info.Type : subMemberValue.GetType();
				if ( EntityDrawer.DrawObjectMember( subMemberType, info.Name, subMemberValue, value, info.SetValue ) ) {
					
				}
			}
			
			// EditorGUI.indentLevel--;

			return value;
		}
		
	}

}
