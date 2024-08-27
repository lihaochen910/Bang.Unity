using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {
	
	public class NullableTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => Nullable.GetUnderlyingType(type) != null;

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) {
			var argType = memberType.GetGenericArguments()[ 0 ];
			if ( EntityDrawer.GetTypeDrawer( argType ) is {} typeDrawer ) {
				// EditorGUILayout.BeginHorizontal();
				var ret = typeDrawer.DrawAndGetNewValue( argType, memberName, value, target );
				// if ( GUILayout.Button( "x" ) ) {
				// 	ret = null;
				// }
				// EditorGUILayout.EndHorizontal();
				return ret;
			}
			
			return null;
		}
	}

}
