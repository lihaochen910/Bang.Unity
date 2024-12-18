using System;
using System.Linq;
using Bang.Unity.Editor.Utilities;
using Bang.Unity.Utilities;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


namespace Bang.Unity.Editor {

	[CustomPropertyDrawer(typeof(SerializableSystemType))]
	public class SerializableSystemTypePropertyDrawer : PropertyDrawer {
		
		public override VisualElement CreatePropertyGUI( SerializedProperty property ) {
			var root = new VisualElement();
			
			var baseInfoField = new PropertyField( property.FindPropertyRelative( "_baseInfo" ) );
			
			root.Add(new Label("BaseInfo:"));
			// root.Add(new Space());
			root.Add( baseInfoField );
			
			return root;
		}

		private int _selectTypeIndex = -1;
		
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label ) {
			if ( property.GetAttribute< TypeOfAttribute >( true ) is {} typeOfAttribute ) {

				EditorGUILayout.BeginHorizontal();
				position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );
				
				using ( var check = new EditorGUI.ChangeCheckScope() ) {
				
					var systemTypes = SystemTypeDrawer.GetAllImplementationsOfCached( typeOfAttribute.Type );
					var systemNames = systemTypes
									  .Select( t => t.FullName.Replace( '.', '/' ) )
									  .ToArray();

					// var selectTypeIndex = -1;
					if ( property.boxedValue is SerializableSystemType value ) {
						_selectTypeIndex = Array.IndexOf( systemTypes, value.SystemType );
					}
					else {
						_selectTypeIndex = -1;
					}
					
					_selectTypeIndex = EditorGUI.Popup( position, _selectTypeIndex, systemNames, EditorStyles.popup );
					if ( check.changed ) {
						property.boxedValue = new SerializableSystemType( systemTypes[ _selectTypeIndex ] );
					}
				
				}

				if ( GUILayout.Button( "reset" ) ) {
					property.boxedValue = new SerializableSystemType( null );
				}
				EditorGUILayout.EndHorizontal();
			}
			else {
				base.OnGUI( position, property, label );
			}
		}

	}

}
