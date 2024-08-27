using System;
using UnityEditor;


namespace Bang.Unity.Editor {

	public class UnityObjectTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) =>
			type == typeof(UnityEngine.Object) ||
			type.IsSubclassOf(typeof(UnityEngine.Object));

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
			EditorGUILayout.ObjectField(memberName, (UnityEngine.Object)value, memberType, true);
	}
	
}
