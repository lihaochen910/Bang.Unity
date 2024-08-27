// using System;
// using UnityEditor;
//
//
// namespace Bang.Unity.Editor {
// 	
// 	public class UnityVisualElementTypeDrawer : ITypeDrawer
// 	{
// 		public bool CanHandlesType(Type type) =>
// 			type == typeof(UnityEngine.UIElements.VisualElement) ||
// 			type.IsSubclassOf(typeof(UnityEngine.UIElements.VisualElement));
//
// 		public object DrawAndGetNewValue(Type memberType, string memberName, object value, object target) =>
// 			EditorGUILayout.ObjectField(memberName, (UnityEngine.Object)value, memberType, true);
// 	}
//
// }