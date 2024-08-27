using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class Vector2TypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(Vector2);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.Vector2Field(memberName, (Vector2)value);
	}
	
}
