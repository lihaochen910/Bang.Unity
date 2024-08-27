using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {
	
	public class Vector3IntTypeDrawer : ITypeDrawer {
		
		public bool CanHandlesType( Type type ) => type == typeof( Vector3Int );

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) =>
			EditorGUILayout.Vector3IntField( memberName, ( Vector3Int )value );

	}
}