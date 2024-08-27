using System;
using UnityEditor;


namespace Bang.Unity.Editor {

	public class CharTypeDrawer : ITypeDrawer
	{
		public bool CanHandlesType(Type type) => type == typeof(char);

		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target)
		{
			var str = EditorGUILayout.TextField(memberName, ((char)value).ToString());
			return str.Length > 0
				? str[0]
				: default;
		}
	}
	
}
