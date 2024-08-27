using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class Vector2IntTypeDrawer : ITypeDrawer {
		
		public bool CanHandlesType( Type type ) => type == typeof( Vector2Int );

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) =>
			EditorGUILayout.Vector2IntField( memberName, ( Vector2Int )value );

	}

}
