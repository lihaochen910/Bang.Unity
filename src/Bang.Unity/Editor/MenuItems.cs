using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

	public static class MenuItems {

		[MenuItem( "GameObject/Create BangEntity", priority = 0 )]
		public static void GameObjectCreateBangEntity( MenuCommand menuCommand ) {
			// var parent = menuCommand.context as GameObject;
			var newGO = ObjectFactory.CreateGameObject( "Entity", typeof( BangEntity ) );
			var entityAsset = ScriptableObject.CreateInstance( typeof( EntityAsset ) ) as EntityAsset;
			newGO.GetComponent< BangEntity >().SetBoundEntityReference( entityAsset );
			Selection.activeObject = newGO;

			// GOCreationCommands.Place(newGO, parent);
			if ( EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D ) {
				var position = newGO.transform.position;
				position.z = 0;
				newGO.transform.position = position;
			}

			Undo.RegisterCreatedObjectUndo( newGO, string.Format( "Create {0}", "Entity" ) );
		}

	}

}
