using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class BoundsTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(Bounds);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.BoundsField(memberName, (Bounds)value);
	}
	
}
