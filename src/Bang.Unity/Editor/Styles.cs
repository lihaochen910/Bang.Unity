using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	internal static class Styles {
		
		public static Color LineColor => EditorGUIUtility.isProSkin ? new(0.05f, 0.05f, 0.05f) : new(0.6f, 0.6f, 0.6f);
		public static Color ThinLineColor => EditorGUIUtility.isProSkin ? new(0.2f, 0.2f, 0.2f) : new(0.7f, 0.7f, 0.7f);

		public static readonly GUIContent GameObjectIcon = EditorGUIUtility.IconContent( "GameObject Icon" );
		public static readonly GUIContent AssemblyIcon = EditorGUIUtility.IconContent( "Assembly Icon" );
		public static readonly GUIContent ModelImporterIcon = EditorGUIUtility.IconContent( "ModelImporter Icon" );
		public static readonly GUIContent BooScriptIcon = EditorGUIUtility.IconContent( "boo Script Icon" );
		public static readonly GUIContent ToggleIcon = EditorGUIUtility.IconContent( "Toggle Icon" );

		public static readonly Texture BooScriptIconTexture = BooScriptIcon.image;
		public static readonly Texture ToggleIconTexture = ToggleIcon.image;
		
		private static GUIStyle _toolbarSearchField;
		public static GUIStyle ToolbarSearchTextField {
#if UNITY_2022_3_OR_NEWER
			get { return _toolbarSearchField ?? ( _toolbarSearchField = new GUIStyle((GUIStyle)"ToolbarSearchTextField") ); }
#else
            get { return _toolbarSearchField ?? ( _toolbarSearchField = new GUIStyle((GUIStyle)"ToolbarSeachTextField") ); }
#endif
		}
		
		private static GUIStyle _toolbarSearchCancelButtonStyle;
		public static GUIStyle ToolbarSearchCancelButtonStyle => _toolbarSearchCancelButtonStyle ??= GUI.skin.FindStyle("ToolbarSearchCancelButton");
		
	}

}
