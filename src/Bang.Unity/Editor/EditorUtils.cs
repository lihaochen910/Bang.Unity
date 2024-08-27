using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

///<summary> AssetDatabase related utility</summary>
public static class EditorUtils {
	
	private static GUIContent tempContent;

	///<summary>A cached temporary content</summary>
	public static GUIContent GetTempContent(string text = "", Texture image = null, string tooltip = null) {
		if ( tempContent == null ) { tempContent = new GUIContent(); }
		tempContent.text = text;
		tempContent.image = image;
		tempContent.tooltip = tooltip;
		return tempContent;
	}

	///<summary>A cached temporary content</summary>
	public static GUIContent GetTempContent(Texture image = null, string tooltip = null) {
		return GetTempContent(null, image, tooltip);
	}

	///<summary>Create asset of type T with a dialog prompt to chose path</summary>
	public static T CreateAsset< T >() where T : ScriptableObject {
		return ( T )CreateAsset( typeof( T ) );
	}

	///<summary>Create asset of type T at target path</summary>
	public static T CreateAsset< T >( string path ) where T : ScriptableObject {
		return ( T )CreateAsset( typeof( T ), path );
	}

	///<summary>Create asset of type and show or not the File Panel</summary>
	public static ScriptableObject CreateAsset( System.Type type ) {
		ScriptableObject asset = null;
		var path = EditorUtility.SaveFilePanelInProject(
			"Create Asset of type " + type.ToString(),
			type.Name + ".asset",
			"asset", "" );
		asset = CreateAsset( type, path );
		return asset;
	}

	///<summary>Create asset of type at target path</summary>
	public static ScriptableObject CreateAsset( System.Type type, string path ) {
		if ( string.IsNullOrEmpty( path ) ) {
			return null;
		}

		ScriptableObject data = null;
		data = ScriptableObject.CreateInstance( type );
		AssetDatabase.CreateAsset( data, path );
		AssetDatabase.SaveAssets();
		return data;
	}

	
	///<summary>Used just after a field to highlight it</summary>
	// public static void HighlightLastField() {
	// 	var lastRect = GUILayoutUtility.GetLastRect();
	// 	lastRect.xMin += 2;
	// 	lastRect.xMax -= 2;
	// 	lastRect.yMax -= 4;
	// 	Styles.Draw(lastRect, Styles.highlightBox);
	// }

	///<summary>Used just after a field to mark it as a prefab override (similar to native unity's one)</summary>
	public static void MarkLastFieldOverride() {
		var rect = GUILayoutUtility.GetLastRect();
		rect.x -= 3;
		rect.width = 2;
		GUI.color = new Color(0.05f, 0.5f, 0.75f, 1f);
		GUI.DrawTexture(rect, Texture2D.whiteTexture);
		GUI.color = Color.white;
	}
	
	private static GUIStyle _styleTopLeftLabel;
	public static GUIStyle StyleTopLeftLabel {
		get
		{
			if ( _styleTopLeftLabel == null ) {
				_styleTopLeftLabel = new GUIStyle(GUI.skin.label);
				_styleTopLeftLabel.richText = true;
				_styleTopLeftLabel.fontSize = 11;
				_styleTopLeftLabel.alignment = TextAnchor.UpperLeft;
				_styleTopLeftLabel.padding.right = 6;
			}
			return _styleTopLeftLabel;
		}
	}
	
	///<summary>Used just after a textfield with no prefix to show an italic transparent text inside when empty</summary>
	public static void CommentLastTextField(string check, string comment = "Comments...") {
		if ( string.IsNullOrEmpty(check) ) {
			var lastRect = GUILayoutUtility.GetLastRect();
			GUI.Label(lastRect, " <i>" + comment + "</i>", StyleTopLeftLabel);
		}
	}
}

}
