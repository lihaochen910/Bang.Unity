using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class Vector4TypeDrawer : ITypeDrawer {
		
		public bool CanHandlesType( Type type ) => type == typeof( Vector4 );

		public object DrawAndGetNewValue( Type memberType, string memberName, object value, object target ) =>
			EditorGUILayout.Vector3Field( memberName, ( Vector4 )value );

	}

}
