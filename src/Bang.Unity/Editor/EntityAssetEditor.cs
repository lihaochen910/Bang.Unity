using System;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

[CustomEditor(typeof(EntityAsset), true)]
public class EntityAssetEditor : UnityEditor.Editor {
	
	private EntityAsset entityAsset => (EntityAsset)target;
	
	private SerializedProperty _serializedEntityProp;

	private void OnEnable() {
		_serializedEntityProp = serializedObject.FindProperty( "_serializedEntity" );
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();
		
		if ( EntityDrawer.DrawComponents( entityAsset.GetEntityInstance().Components ) ) {
			if ( entityAsset.SelfSerialize() ) {
				_serializedEntityProp.stringValue = entityAsset.GetSerializedJsonData();
				EditorUtility.SetDirty( entityAsset );
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
	
}


// [CustomPropertyDrawer(typeof(EntityAsset), true)]
// public sealed class EntityAssetPropertyDrawer : PropertyDrawer {
//
// 	public override void OnGUI( Rect rect, SerializedProperty prop, GUIContent label ) {
// 		// var entityAsset = prop.objectReferenceValue as EntityAsset;
// 		// EntityDrawer.DrawComponents( entityAsset.GetEntityInstance().AllComponents );
// 	}
// }

}
