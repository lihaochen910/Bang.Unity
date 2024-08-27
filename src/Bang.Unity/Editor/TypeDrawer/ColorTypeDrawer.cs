using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public class ColorTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(Color);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.ColorField(memberName, (Color)value);
	}
	
}
