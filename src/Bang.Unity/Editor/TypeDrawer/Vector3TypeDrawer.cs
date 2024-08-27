using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class Vector3TypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(Vector3);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.Vector3Field(memberName, (Vector3)value);
	}
	
}
