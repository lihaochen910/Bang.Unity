using System;
using UnityEditor;


namespace Bang.Unity.Editor {

	public class BoolTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(bool);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.Toggle(memberName, (bool)value);
	}
	
}
