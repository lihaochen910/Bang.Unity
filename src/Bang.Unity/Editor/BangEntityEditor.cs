using System;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Bang.Unity.Editor {

[CustomEditor(typeof(BangEntity)), CanEditMultipleObjects]
public class BangEntityEditor : UnityEditor.Editor {

	private BangEntity owner => ( BangEntity ) target;
	private SerializedProperty _entityDestroyFollowPolicyProp;
	private SerializedProperty _entityAssetProp;
	private SerializedProperty _boundEntitySerializationProp;
	private SerializedProperty _boundEntityReferencesProp;
	private SerializedProperty _lockPrefabProp;

	private bool isOwnerPeristant => EditorUtility.IsPersistent( owner );
	
	public bool isBoundEntityOnPrefabRoot => isOwnerPeristant && owner.EntityIsBound;

	public bool isBoundEntityOnPrefabInstance => !isOwnerPeristant && owner.EntityIsBound && PrefabUtility.IsPartOfAnyPrefab( owner );
	
	public bool isBoundEntityPrefabOverridden => _boundEntitySerializationProp.prefabOverride;

	private void OnEnable() {
		_entityDestroyFollowPolicyProp = serializedObject.FindProperty( "_entityDestroyFollowPolicy" );
		_entityAssetProp = serializedObject.FindProperty( "_entityAsset" );
		_boundEntitySerializationProp = serializedObject.FindProperty( "_boundEntitySerialization" );
		_boundEntityReferencesProp = serializedObject.FindProperty( "_boundEntityObjectReferences" );
		_lockPrefabProp = serializedObject.FindProperty( "LockBoundEntityPrefabOverrides" );
	}

	public override void OnInspectorGUI() {
		if ( !Application.isPlaying ) {
			
			serializedObject.Update();
			EditorGUILayout.PropertyField( _entityDestroyFollowPolicyProp, EditorUtils.GetTempContent( "DestroyPolicy" ) );
			EditorGUILayout.Space();
			serializedObject.ApplyModifiedProperties();
			
			DoPrefabRelatedGUI();

			var entityAsset = owner.EntityAsset;
			if ( entityAsset == null/* && !owner.EntityIsBound*/ ) {
				DoMissingEntityControls();
				serializedObject.ApplyModifiedProperties();
				return;
			}
			
			EditorGUI.BeginChangeCheck();
			DoValidEntityControls();
			DoStandardFields();

			GUI.enabled = ( !isBoundEntityOnPrefabInstance || !owner.LockBoundEntityPrefabOverrides ) && !isBoundEntityOnPrefabRoot;
			// OnPreExtraGraphOptions();
			GUI.enabled = true;
			if ( EditorGUI.EndChangeCheck() && entityAsset != null ) {
				// UndoUtility.RecordObject(owner.graph, "Sub Option Change");
				entityAsset.SelfSerialize();
				// UndoUtility.SetDirty(owner.graph);
			}

			serializedObject.Update();
			if ( entityAsset != null &&
				 entityAsset.GetEntityInstance().Components != null &&
				 EntityDrawer.DrawComponents( entityAsset.GetEntityInstance().Components ) ) {
				if ( entityAsset.SelfSerialize() ) {
					owner.OnAfterEntitySerialized( entityAsset );
					EditorUtility.SetDirty( owner );
					serializedObject.ApplyModifiedProperties();
				}

				// owner.Validate();
			}
			
			// debug: json serialization
			// EditorGUI.BeginDisabledGroup( true );
			// EditorGUILayout.TextArea( owner.BoundEntitySerialization );
			// EditorGUI.EndDisabledGroup();
		}
		else {
			if ( targets.Length == 1 ) {
				EntityDrawer.DrawEntity( owner.Entity );
			}
			else {
				var entities = targets
							   .Select( t => ( ( BangEntity )t ).Entity )
							   .ToArray();

				EntityDrawer.DrawMultipleEntities( entities );
			}

			if ( target != null ) {
				EditorUtility.SetDirty( target );
			}
		}
		
	}
	
	// create new graph asset and assign it to owner
	public EntityAsset NewAsAsset() {
		var newEntityAsset = ( EntityAsset )EditorUtils.CreateAsset( typeof( EntityAsset ) );
		if ( newEntityAsset != null ) {
			// UndoUtility.RecordObject(owner, "New Asset Graph");
			owner.GetType().GetProperty( nameof( BangEntity.EntityAsset ) ).SetValue( owner, newEntityAsset );
			// UndoUtility.SetDirty(owner);
			// UndoUtility.SetDirty(newEntityAsset);
			AssetDatabase.SaveAssets();
		}
		return newEntityAsset;
	}

	// create new local graph and assign it to owner
	public EntityAsset NewAsBound() {
		var newEntity = ( EntityAsset )ScriptableObject.CreateInstance( typeof( EntityAsset ) );
		// UndoUtility.RecordObject(owner, "New Bound Graph");
		owner.SetBoundEntityReference( newEntity );
		// UndoUtility.SetDirty(owner);
		return newEntity;
	}
	
	//Bind graph to owner
	public void AssetToBound() {
		// UndoUtility.RecordObject(owner, "Bind Asset Graph");
		owner.SetBoundEntityReference( owner.EntityAsset );
		// UndoUtility.SetDirty(owner);
	}
	
	// Revert bound entity
	public void PrefabRevertBoundEntity() {
		// UndoUtility.RecordObject(owner, "Revert Graph From Prefab");
		var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( owner );
		PrefabUtility.RevertPropertyOverride( _boundEntitySerializationProp, InteractionMode.UserAction );
		PrefabUtility.RevertPropertyOverride( _boundEntityReferencesProp, InteractionMode.UserAction );
		// UndoUtility.SetDirty(owner);
	}

	// Apply bound entity
	public void PrefabApplyBoundEntity() {
		// UndoUtility.RecordObject(owner, "Apply Graph To Prefab");
		var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( owner );
		PrefabUtility.ApplyPropertyOverride( _boundEntitySerializationProp, prefabAssetPath, InteractionMode.UserAction );
		PrefabUtility.ApplyPropertyOverride( _boundEntityReferencesProp, prefabAssetPath, InteractionMode.UserAction );
		// UndoUtility.SetDirty(owner);
	}


	///----------------------------------------------------------------------------------------------
	
	//...
	void DoPrefabRelatedGUI() {
		
		//show lock bound graph prefab overrides
		if ( owner.EntityIsBound ) {
			var case1 = PrefabUtility.IsPartOfPrefabAsset(owner) || UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot == owner.gameObject;
			var case2 = PrefabUtility.IsPartOfAnyPrefab(owner) && !isBoundEntityPrefabOverridden;
			if ( case1 || case2 ) {
				EditorGUILayout.PropertyField( _lockPrefabProp, EditorUtils.GetTempContent( "Lock Prefab Graph Overrides" ) );
			}
		}

		//show bound graph prefab overrides controls
		if ( isBoundEntityPrefabOverridden ) {
			GUILayout.Space(5);
			GUI.color = new Color(0.05f, 0.5f, 0.75f, 1f);
			GUILayout.BeginHorizontal();
			GUI.color = Color.white;
			var content = EditorUtils.GetTempContent("<b>Bound Graph is prefab overridden.</b>"); //, StyleSheet.canvasIcon);
			GUILayout.Label( content, EditorUtils.StyleTopLeftLabel );
			if ( GUILayout.Button( "Revert Entity", EditorStyles.miniButtonLeft, GUILayout.Width( 100 ) ) ) {
				PrefabRevertBoundEntity();
			}
			if ( GUILayout.Button( "Apply Entity", EditorStyles.miniButtonRight, GUILayout.Width( 100 ) ) ) {
				PrefabApplyBoundEntity();
			}
			GUILayout.EndHorizontal();
			EditorUtils.MarkLastFieldOverride();
			GUILayout.Space(5);
		}
		
	}
	
	
	//...
	void DoMissingEntityControls() {
		EditorGUILayout.HelpBox( "Needs a entity instance.\nAssign or Create a new one...", MessageType.Info );
		if ( !Application.isPlaying && GUILayout.Button( "CREATE NEW" ) ) {
			EntityAsset newEntityAsset = NewAsBound();
			// if ( EditorUtility.DisplayDialog("Create Entity", "Create a Bound or an Asset Entity?\n\n" +
			// 												 "Bound Entity is saved with the BangEntity and you can use direct scene references within it.\n\n" +
			// 												 "Asset Entity is an asset file and can be reused amongst any number of BangEntities.\n\n" +
			// 												 "You can convert from one type to the other at any time.",
			// 		"Bound", "Asset") ) {
			//
			// 	newEntityAsset = NewAsBound();
			//
			// } else {
			//
			// 	newEntityAsset = NewAsAsset();
			// }

			if ( newEntityAsset != null ) {
				owner.Validate();
			}
		}

		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( _entityAssetProp, new GUIContent( "Entity" ) );

		if ( EditorGUI.EndChangeCheck() ) {
			owner.Validate();
		}
	}
	
	
	//...
    void DoValidEntityControls() {

        // Entity comments ONLY if Bound graph else readonly
        if ( owner.EntityAsset != null ) {
            if ( owner.EntityIsBound ) {
                GUI.contentColor = new Color( 1.0f, 1.0f, 1.0f, 0.6f );
                owner.EntityAsset.Comments = GUILayout.TextArea(owner.EntityAsset.Comments, GUILayout.Height(45));
                GUI.contentColor = Color.white;
                EditorUtils.CommentLastTextField(owner.EntityAsset.Comments, "Entity comments...");
            } else {
                GUI.enabled = false;
                GUILayout.TextArea(owner.EntityAsset.Comments, GUILayout.Height(45));
                GUI.enabled = true;
            }
        }

        if ( !isBoundEntityOnPrefabRoot ) {

            // Open behaviour
            // GUI.backgroundColor = Colors.lightBlue;
            // if ( GUILayout.Button(( "Edit " + owner.graphType.Name.SplitCamelCase() ).ToUpper()) ) {
            //     GraphEditor.OpenWindow(owner);
            // }
            // GUI.backgroundColor = Color.white;

        }
		else {

            EditorGUILayout.HelpBox("Bound Graphs on prefabs can only be edited by opening the prefab in the prefab editor.", MessageType.Info);

            //Open prefab and behaviour
            GUI.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 1) : Color.white;
            if ( GUILayout.Button(( "Open Prefab And Edit" ).ToUpper()) ) {
                AssetDatabase.OpenAsset(owner);
                // GraphEditor.OpenWindow(owner);
            }
            GUI.backgroundColor = Color.white;
        }

        //bind asset or delete bound graph
        if ( !Application.isPlaying ) {
            if ( !owner.EntityIsBound ) {
                if ( GUILayout.Button("Bind Entity") ) {
                    if ( EditorUtility.DisplayDialog("Bind Entity", "This will make a local copy of the graph, bound to the owner.\n\nThis allows you to make local changes and assign scene object references directly.\n\nNote that you can also use scene object references through the use of Blackboard Variables.\n\nBind Graph?", "YES", "NO") ) {
                        AssetToBound();
                    }
                }
            } else {
                if ( GUILayout.Button("Delete Bound Entity") ) {
                    if ( EditorUtility.DisplayDialog("Delete Bound Entity", "Are you sure?", "YES", "NO") ) {
						UnityEngine.Object.DestroyImmediate(owner.EntityAsset, true);
                        // UndoUtility.RecordObject(owner, "Delete Bound Entity");
						owner.SetBoundEntityReference( null );
						// UndoUtility.SetDirty(owner);
					}
                }
            }
        }
    }

	
	//...
	void DoStandardFields() {
		//basic options
		if ( Application.isPlaying || !owner.EntityIsBound ) {
			EditorGUILayout.PropertyField( _entityAssetProp, EditorUtils.GetTempContent( "Entity" ) );
		}
	}

}

}