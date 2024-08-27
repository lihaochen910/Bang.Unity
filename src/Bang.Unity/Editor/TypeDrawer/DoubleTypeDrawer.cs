using System;
using UnityEditor;


namespace Bang.Unity.Editor {

	public class DoubleTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(double);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.DoubleField(memberName, (double)value);
	}
	
}
