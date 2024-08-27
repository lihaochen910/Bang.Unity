using System;
using UnityEditor;


namespace Bang.Unity.Editor {

	public class FloatTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(float);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.FloatField(memberName, (float)value);
	}
	
}
