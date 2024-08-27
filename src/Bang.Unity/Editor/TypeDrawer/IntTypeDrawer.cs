using System;
using UnityEditor;


namespace Bang.Unity.Editor {

	public class IntTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(int);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.IntField(memberName, (int)value);
	}
	
}
