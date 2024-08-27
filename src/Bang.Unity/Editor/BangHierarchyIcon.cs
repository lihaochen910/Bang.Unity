using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {
	
[InitializeOnLoad]
public static class BangHierarchyIcon {
	
	private static Texture2D _entityHierarchyIcon;
	private static Texture2D EntityHierarchyIcon {
		get {
			if ( _entityHierarchyIcon == null ) {
				_entityHierarchyIcon = LoadTexture( "Textures/BangEntityHierarchyIcon" );
			}

			return _entityHierarchyIcon;
		}
	}


	static BangHierarchyIcon() {
		EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
		EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
	}


	static void OnHierarchyWindowItemOnGUI( int instanceID, Rect selectionRect ) {
		var gameObject = EditorUtility.InstanceIDToObject( instanceID ) as GameObject;
		if ( gameObject == null ) {
			return;
		}

		const float iconSize = 12; // 16f;
		const float iconOffset = iconSize + 2f;

		if ( gameObject.TryGetComponent< BangEntity >( out var _ ) ) {
			var rect = new Rect( selectionRect.x + selectionRect.width - iconOffset, selectionRect.y + 2f, iconSize, iconSize );
			GUI.DrawTexture( rect, EntityHierarchyIcon );
			return;
		}
	
	}


	public static Texture2D LoadTexture( string label ) {
		return Resources.Load< Texture2D >( label );
		
		// string text = AssetDatabase.FindAssets( label ).FirstOrDefault();
		// if ( string.IsNullOrEmpty( text ) ) {
		// 	return null;
		// }
		//
		// return AssetDatabase.LoadAssetAtPath< Texture2D >( AssetDatabase.GUIDToAssetPath( text ) );
	}

}

}
