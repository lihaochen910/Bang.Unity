using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace Bang.Unity.Editor {

	[CustomEditor(typeof(BangEntity)), CanEditMultipleObjects]
	public class BangEntityEditor : UnityEditor.Editor {

		private BangEntity _owner => ( BangEntity )target;
		private SerializedProperty _entityDestroyFollowPolicyProp;
		private SerializedProperty _entityAssetProp;
		private SerializedProperty _boundEntitySerializationProp;
		private SerializedProperty _boundEntityReferencesProp;
		private SerializedProperty _lockPrefabProp;
		private bool _foldoutDebugBoundSerialization;
		// private string _prettifyBoundEntitySerialization;
		private Vector2 _debugBoundSerializationScrollPosition;

		private bool IsOwnerPeristant => EditorUtility.IsPersistent( _owner );
		
		public bool IsBoundEntityOnPrefabRoot => IsOwnerPeristant && _owner.EntityIsBound;

		public bool IsBoundEntityOnPrefabInstance => !IsOwnerPeristant && _owner.EntityIsBound && PrefabUtility.IsPartOfAnyPrefab( _owner );
		
		public bool IsBoundEntityPrefabOverridden => _boundEntitySerializationProp.prefabOverride;

		private void OnEnable() {
			_entityDestroyFollowPolicyProp = serializedObject.FindProperty( "_entityDestroyFollowPolicy" );
			_entityAssetProp = serializedObject.FindProperty( "_entityAsset" );
			_boundEntitySerializationProp = serializedObject.FindProperty( "_boundEntitySerialization" );
			_boundEntityReferencesProp = serializedObject.FindProperty( "_boundEntityObjectReferences" );
			_lockPrefabProp = serializedObject.FindProperty( "LockBoundEntityPrefabOverrides" );
			
			// _prettifyBoundEntitySerialization = Bang.Unity.Serialization.JSONSerializer.PrettifyJson( _owner.BoundEntitySerialization );
		}

		private void InitializeProperties() {
			_entityDestroyFollowPolicyProp = serializedObject.FindProperty( "_entityDestroyFollowPolicy" );
			_entityAssetProp = serializedObject.FindProperty( "_entityAsset" );
			_boundEntitySerializationProp = serializedObject.FindProperty( "_boundEntitySerialization" );
			_boundEntityReferencesProp = serializedObject.FindProperty( "_boundEntityObjectReferences" );
			_lockPrefabProp = serializedObject.FindProperty( "LockBoundEntityPrefabOverrides" );
		}

		private void OnValidate() {
			// _prettifyBoundEntitySerialization = Bang.Unity.Serialization.JSONSerializer.PrettifyJson( _owner.BoundEntitySerialization );
		}

		public override void OnInspectorGUI() {
			if ( !Application.isPlaying ) {
				
				serializedObject.Update();
				EditorGUILayout.PropertyField( _entityDestroyFollowPolicyProp, EditorUtils.GetTempContent( "DestroyPolicy" ) );
				EditorGUILayout.Space();
				serializedObject.ApplyModifiedProperties();
				
				DoPrefabRelatedGUI();
				
				var entityAsset = _owner.EntityAsset;
				if ( entityAsset == null && !_owner.EntityIsBound ) {
					DoMissingEntityControls();
					serializedObject.ApplyModifiedProperties();
					return;
				}
				
				EditorGUI.BeginChangeCheck();
				DoValidEntityControls();
				DoStandardFields();
				
				// GUI.enabled = ( !IsBoundEntityOnPrefabInstance || !_owner.LockBoundEntityPrefabOverrides ) && !IsBoundEntityOnPrefabRoot;
				// OnPreExtraGraphOptions();
				GUI.enabled = true;
				if ( EditorGUI.EndChangeCheck() && entityAsset != null ) {
					Undo.RecordObject( _owner.EntityAsset, "Entity Asset Change" );
					entityAsset.SelfSerialize();
					EditorUtility.SetDirty( _owner.EntityAsset );
				}

				var componentDataChanged = false;
				serializedObject.Update();
				if ( entityAsset != null &&
					 entityAsset.GetEntityInstance()?.Components != null ) {
					componentDataChanged = EntityDrawer.DrawComponents( entityAsset.GetEntityInstance().Components );
					if ( componentDataChanged &&
						 entityAsset.SelfSerialize() ) {
						Undo.RecordObject( _owner.EntityAsset, "Entity Component Data Change" );
						_owner.OnAfterEntitySerialized( entityAsset );
						EditorUtility.SetDirty( _owner );
						serializedObject.ApplyModifiedProperties();
					}
				
					// owner.Validate();
				}
				
				// debug: json serialization
				// if ( componentDataChanged ) {
				// 	_prettifyBoundEntitySerialization = Bang.Unity.Serialization.JSONSerializer.PrettifyJson( _owner.BoundEntitySerialization );
				// }
				
				EditorGUI.indentLevel++;
				_foldoutDebugBoundSerialization = EditorGUILayout.Foldout( _foldoutDebugBoundSerialization, "Serialization(json)" );
				if ( _foldoutDebugBoundSerialization ) {
					// _debugBoundSerializationScrollPosition = GUILayout.BeginScrollView( _debugBoundSerializationScrollPosition,
					// 	GUILayout.Width( EditorGUIUtility.currentViewWidth ),
					// 	GUILayout.Height( 130 ) );
					EditorGUILayout.BeginVertical();
					// EditorGUILayout.LabelField( "JSON Serialization", EditorStyles.boldLabel );
					EditorGUI.BeginDisabledGroup( true );
					EditorGUILayout.TextArea( _owner.BoundEntitySerialization, GUILayout.Height( 50 ) );
					EditorGUI.EndDisabledGroup();
					
					if ( !string.IsNullOrEmpty( _owner.BoundEntitySerialization ) ) {
						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if ( GUILayout.Button( "Copy" ) ) {
							GUIUtility.systemCopyBuffer = Bang.Unity.Serialization.JSONSerializer.PrettifyJson( _owner.BoundEntitySerialization );
						}
						if ( GUILayout.Button( "Print" ) ) {
							Debug.Log( Bang.Unity.Serialization.JSONSerializer.PrettifyJson( _owner.BoundEntitySerialization ) );
						}
						GUILayout.EndHorizontal();
					}
					
					EditorGUILayout.EndVertical();
					// GUILayout.EndScrollView();
				}
				EditorGUI.indentLevel--;
				
			}
			else {
				if ( targets.Length == 1 ) {
					EntityDrawer.DrawEntity( _owner.Entity );
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
		
		// create new entity asset and assign it to owner
		public EntityAsset NewAsAsset() {
			var newEntityAsset = ( EntityAsset )EditorUtils.CreateAsset( typeof( EntityAsset ) );
			if ( newEntityAsset != null ) {
				// UndoUtility.RecordObject(owner, "New Asset Entity");
				_owner.GetType().GetProperty( nameof( BangEntity.EntityAsset ) ).SetValue( _owner, newEntityAsset );
				// UndoUtility.SetDirty(owner);
				// UndoUtility.SetDirty(newEntityAsset);
				AssetDatabase.SaveAssets();
			}
			return newEntityAsset;
		}

		// create new local entity and assign it to owner
		public EntityAsset NewAsBound() {
			var newEntity = ( EntityAsset )ScriptableObject.CreateInstance( typeof( EntityAsset ) );
			Undo.RecordObject( target, "New Bound Entity" );
			_owner.SetBoundEntityReference( newEntity );
			EditorUtility.SetDirty( target );
			return newEntity;
		}
		
		// Bind entity to owner
		public void AssetToBound() {
			UnityEditor.Undo.RecordObject( target, "Bind Asset Entity" );
			_owner.SetBoundEntityReference( _owner.EntityAsset );
			UnityEditor.EditorUtility.SetDirty( target );
		}
		
		// Revert bound entity
		public void PrefabRevertBoundEntity() {
			UnityEditor.Undo.RecordObject( target, "Revert Entity From Prefab" );
			// var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( _owner );
			PrefabUtility.RevertPropertyOverride( _boundEntitySerializationProp, InteractionMode.UserAction );
			PrefabUtility.RevertPropertyOverride( _boundEntityReferencesProp, InteractionMode.UserAction );
			UnityEditor.EditorUtility.SetDirty( target );
		}

		// Apply bound entity
		public void PrefabApplyBoundEntity() {
			UnityEditor.Undo.RecordObject( target, "Apply Entity To Prefab" );
			var prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( _owner );
			PrefabUtility.ApplyPropertyOverride( _boundEntitySerializationProp, prefabAssetPath, InteractionMode.UserAction );
			PrefabUtility.ApplyPropertyOverride( _boundEntityReferencesProp, prefabAssetPath, InteractionMode.UserAction );
			UnityEditor.EditorUtility.SetDirty( target );
		}


		///----------------------------------------------------------------------------------------------
		
		private bool IsInPrefabAssetMode() {
			// 检查是否在Prefab Asset中
			bool isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset( _owner );
			
			// 检查是否在Prefab Stage中编辑
			var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			bool isInPrefabStage = prefabStage?.prefabContentsRoot == _owner.gameObject;
			
			return isPrefabAsset || isInPrefabStage;
		}

		private string GetPrefabAssetPath() {
			// 方法1: 如果是Prefab实例，获取最近的Prefab根节点的路径
			if ( PrefabUtility.IsPartOfPrefabInstance( _owner ) ) {
				return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot( _owner );
			}

			// 方法2: 如果是在Prefab Stage中编辑
			var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();

			if ( prefabStage != null && prefabStage.prefabContentsRoot == _owner.gameObject ) {
				return prefabStage.assetPath;
			}

			// 方法3: 如果是Prefab Asset本身
			if ( PrefabUtility.IsPartOfPrefabAsset( _owner ) ) {
				return AssetDatabase.GetAssetPath( _owner );
			}

			return null;
		}

		private void OpenPrefabAsset( string prefabPath ) {
			// 方法1: 使用AssetDatabase.OpenAsset
			var prefabAsset = AssetDatabase.LoadAssetAtPath< GameObject >( prefabPath );
			if ( prefabAsset != null ) {
				AssetDatabase.OpenAsset( prefabAsset );
				return;
			}

			// 方法2: 使用PrefabStage打开
			UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab( prefabPath );

			// 方法3: 如果需要选中特定对象
			Selection.activeObject = prefabAsset;
			EditorGUIUtility.PingObject( prefabAsset );
		}

		//...
		void DoPrefabRelatedGUI() {
			
			//show lock bound entity prefab overrides
			if ( _owner.EntityIsBound ) {
				// var case1 = PrefabUtility.IsPartOfPrefabAsset(_owner) || UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot == _owner.gameObject;
				// var case2 = PrefabUtility.IsPartOfAnyPrefab(_owner) && !isBoundEntityPrefabOverridden;
				// if ( case1 || case2 ) {
				// 	EditorGUILayout.PropertyField( _lockPrefabProp, EditorUtils.GetTempContent( "Lock Prefab Entity Overrides" ) );
				// }

				// 使用新的检查方法
				// var isRootInPrefabAsset = IsInPrefabAssetMode();
				// var isPrefabInstanceNotOverridden = PrefabUtility.IsPartOfAnyPrefab( _owner ) && !IsBoundEntityPrefabOverridden;
				
// 				if (isRootInPrefabAsset || isPrefabInstanceNotOverridden) {
// 					// EditorGUILayout.PropertyField(_lockPrefabProp, EditorUtils.GetTempContent("Lock Prefab Entity Overrides"));
// 					
// 					var prefabPath = GetPrefabAssetPath();
// 					
// 					// 添加调试信息
// #if UNITY_EDITOR && DEBUG
// 					EditorGUILayout.HelpBox(
// 						// $"Prefab Mode Info:\n" +
// 						$"Is Prefab Asset: {PrefabUtility.IsPartOfPrefabAsset(_owner)}\n" +
// 						$"Is In Prefab Stage: {UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot == _owner.gameObject}\n" +
// 						$"Is Prefab Instance: {PrefabUtility.IsPartOfAnyPrefab(_owner)}\n" +
// 						$"Is Overridden: {IsBoundEntityPrefabOverridden}\n" +
// 						$"Prefab: {prefabPath}",
// 						MessageType.Info
// 					);
// #endif
// 				}

				if ( PrefabUtility.IsPartOfAnyPrefab( _owner ) && IsBoundEntityPrefabOverridden ) {
					EditorGUILayout.HelpBox( "Prefab instance changed.", MessageType.Info );
				}
				
			}

			//show bound entity prefab overrides controls
			// if ( IsBoundEntityPrefabOverridden ) {
			// 	GUILayout.Space( 5f );
			// 	GUI.color = new Color( 0.05f, 0.5f, 0.75f, 1f );
			// 	GUILayout.BeginHorizontal();
			// 	GUI.color = Color.white;
			// 	
			// 	// 在布局中添加空的空间以推送按钮到右侧
			// 	GUILayout.FlexibleSpace();
			// 	GUILayout.Space( 10f );
			// 	// var content = EditorUtils.GetTempContent("<b>Bound Entity is prefab overridden.</b>"); //, StyleSheet.canvasIcon);
			// 	// GUILayout.Label( content, EditorUtils.StyleTopLeftLabel );
			// 	if ( GUILayout.Button( "Revert to Prefab Entity", EditorStyles.miniButtonLeft, GUILayout.Width( 130 ) ) ) {
			// 		PrefabRevertBoundEntity();
			// 	}
			// 	// if ( GUILayout.Button( "Apply Entity", EditorStyles.miniButtonRight, GUILayout.Width( 100 ) ) ) {
			// 	// 	PrefabApplyBoundEntity();
			// 	// }
			// 	GUILayout.EndHorizontal();
			// 	EditorUtils.MarkLastFieldOverride();
			// 	GUILayout.Space( 5 );
			// }
			
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
					_owner.Validate();
				}
			}

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( _entityAssetProp, new GUIContent( "Entity" ) );

			if ( EditorGUI.EndChangeCheck() ) {
				_owner.Validate();
			}
		}
		
		
		//...
	    void DoValidEntityControls() {

	        // // Entity comments ONLY if Bound entity else readonly
	        // if ( _owner.EntityAsset != null ) {
	        //     if ( _owner.EntityIsBound ) {
	        //         GUI.contentColor = new Color( 1.0f, 1.0f, 1.0f, 0.6f );
	        //         _owner.EntityAsset.Comments = GUILayout.TextArea(_owner.EntityAsset.Comments, GUILayout.Height(45));
	        //         GUI.contentColor = Color.white;
	        //         EditorUtils.CommentLastTextField(_owner.EntityAsset.Comments, "Entity comments...");
	        //     } else {
	        //         GUI.enabled = false;
	        //         GUILayout.TextArea(_owner.EntityAsset.Comments, GUILayout.Height(45));
	        //         GUI.enabled = true;
	        //     }
	        // }

	        if ( !IsBoundEntityOnPrefabRoot ) {

				if ( IsBoundEntityPrefabOverridden ) {
					GUILayout.Space( 5f );
					GUI.color = new Color( 0.05f, 0.5f, 0.75f, 1f );
					GUILayout.BeginHorizontal();
					GUI.color = Color.white;
				}

				// Open behaviour
				GUI.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 1) : Color.white;
	            if ( GUILayout.Button("Edit Entity" ) ) {
					var prefabPath = GetPrefabAssetPath();
					if ( !string.IsNullOrEmpty( prefabPath ) ) {
						// AssetDatabase.OpenAsset( prefabPath );
						UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab( prefabPath );
					}
				}
	            GUI.backgroundColor = Color.white;

				if ( IsBoundEntityPrefabOverridden ) {
					GUILayout.EndHorizontal();
				}
	        }
			else {

	            EditorGUILayout.HelpBox("Bound Entities on prefabs can only be edited by opening the prefab in the prefab editor.", MessageType.Info);

	            // Open prefab and behaviour
	            GUI.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 1) : Color.white;
	            if ( GUILayout.Button("Open Prefab And Edit".ToUpper()) ) {
					AssetDatabase.OpenAsset( _owner );
					// GraphEditor.OpenWindow(owner);
				}
	            GUI.backgroundColor = Color.white;
	        }

	        // bind asset or delete bound entity
	        if ( !Application.isPlaying ) {
	            if ( !_owner.EntityIsBound ) {
	                if ( GUILayout.Button("Bind Entity") ) {
	                    if ( EditorUtility.DisplayDialog("Bind Entity", "This will make a local copy of the graph, bound to the owner.\n\nThis allows you to make local changes and assign scene object references directly.\n\nNote that you can also use scene object references through the use of Blackboard Variables.\n\nBind Entity?", "YES", "NO") ) {
	                        AssetToBound();
	                    }
	                }
	            } else {
					var guiColor = GUI.color;
					GUI.backgroundColor = Color.red;
	                if ( GUILayout.Button("Delete Bound Entity") ) {
	                    if ( EditorUtility.DisplayDialog("Delete Bound Entity", "Are you sure?", "YES", "NO") ) {
							UnityEngine.Object.DestroyImmediate(_owner.EntityAsset, true);
							Undo.RecordObject( target, "Delete Bound Entity" );
							_owner.SetBoundEntityReference( null );
							EditorUtility.SetDirty( target );
						}
	                }
					GUI.backgroundColor = guiColor;
				}
	        }
	    }

		
		//...
		void DoStandardFields() {
			//basic options
			if ( Application.isPlaying || !_owner.EntityIsBound ) {
				EditorGUILayout.PropertyField( _entityAssetProp, EditorUtils.GetTempContent( "Entity" ) );
			}
		}

		
		private VisualElement _rootElement;
		private VisualElement _prefabControlsContainer;
		private VisualElement _entityControlsContainer;
		private PropertyField _entityAssetField;
		private PropertyField _destroyPolicyField;
		private TextField _commentsField;

		// public override VisualElement CreateInspectorGUI() {
		// 	
		// 	// 获取默认Inspector
		// 	var root = new VisualElement();
		// 	_rootElement = root;
  //   
		// 	// 这一行相当于base.CreateInspectorGUI()的内部实现
		// 	InspectorElement.FillDefaultInspector( root, serializedObject, this );
  //   
		// 	// 添加自定义元素
		// 	var customElement = new Button( () => {
		// 		TypeSelectorFancyPopup.Open( _rootElement, typeof( IComponent ), t => {
		// 			Debug.Log( $"select: {t.Name}" );
		// 		} );
		// 	})
		// 	{
		// 		text = "Custom Button"
		// 	};
		// 	root.Add(customElement);
  //   
		// 	// 或者插入到特定位置
		// 	root.Insert(0, customElement); // 插入到最前面
  //   
		// 	return root;
		// 	
		// 	// _rootElement = base.CreateInspectorGUI();
		// 	// return _rootElement;
		//
		// 	// if ( Application.isPlaying ) {
		// 	// 	return base.CreateInspectorGUI();
		// 	// }
		// 	//
		// 	// InitializeProperties();
		// 	//
		// 	// _rootElement = new VisualElement();
		// 	//
		// 	// if ( !Application.isPlaying ) {
		// 	// 	CreateEditModeUI();
		// 	// }
		// 	// else {
		// 	// 	CreatePlayModeUI();
		// 	// }
		// 	//
		// 	// return _rootElement;
		// }
		
		private void CreateEditModeUI()
		{
			_destroyPolicyField = new PropertyField(_entityDestroyFollowPolicyProp, "DestroyPolicy");
			_rootElement.Add(_destroyPolicyField);
			_rootElement.Add(new SpaceElement());

			CreatePrefabControls();

			var entityAsset = _owner.EntityAsset;
			if (entityAsset == null)
			{
				CreateMissingEntityControls();
			}
			else
			{
				CreateValidEntityControls();
				CreateStandardFields();
				CreateComponentsView();
			}
		}
		
		private void CreatePrefabControls()
		{
			_prefabControlsContainer = new VisualElement();
	            
			if (_owner.EntityIsBound)
			{
				var case1 = PrefabUtility.IsPartOfPrefabAsset(_owner) || 
							UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot == _owner.gameObject;
				var case2 = PrefabUtility.IsPartOfAnyPrefab(_owner) && !IsBoundEntityPrefabOverridden;
	                
				if (case1 || case2)
				{
					var lockPrefabField = new PropertyField(_lockPrefabProp, "Lock Prefab Entity Overrides");
					_prefabControlsContainer.Add(lockPrefabField);
				}
			}

			if (IsBoundEntityPrefabOverridden)
			{
				var overrideContainer = new VisualElement();
				overrideContainer.style.flexDirection = FlexDirection.Row;
				overrideContainer.style.marginTop = 5;
	                
				var label = new Label("Bound Entity is prefab overridden.") 
				{ 
					style = { unityFontStyleAndWeight = FontStyle.Bold } 
				};
	                
				var revertButton = new Button(PrefabRevertBoundEntity) { text = "Revert Entity" };
				var applyButton = new Button(PrefabApplyBoundEntity) { text = "Apply Entity" };
	                
				overrideContainer.Add(label);
				overrideContainer.Add(revertButton);
				overrideContainer.Add(applyButton);
	                
				_prefabControlsContainer.Add(overrideContainer);
			}

			_rootElement.Add(_prefabControlsContainer);
		}
		
		private void CreateMissingEntityControls()
		{
			_entityControlsContainer = new VisualElement();
	            
			var helpBox = new HelpBox("Needs a entity instance.\nAssign or Create a new one...", HelpBoxMessageType.Info);
			_entityControlsContainer.Add(helpBox);

			var createButton = new Button(() => 
			{
				if (!Application.isPlaying)
				{
					var newEntityAsset = NewAsBound();
					if (newEntityAsset != null)
					{
						_owner.Validate();
					}
				}
			})
			{
				text = "CREATE NEW"
			};

			_entityAssetField = new PropertyField(_entityAssetProp, "Entity");
			_entityAssetField.RegisterValueChangeCallback(evt => _owner.Validate());

			_entityControlsContainer.Add(createButton);
			_entityControlsContainer.Add(_entityAssetField);
	            
			_rootElement.Add(_entityControlsContainer);
		}
		
		private void CreateValidEntityControls()
	    {
	        _entityControlsContainer = new VisualElement();

	        if (!IsBoundEntityOnPrefabRoot)
	        {
	            var editButton = new Button(() =>
	            {
					AssetDatabase.OpenAsset( _owner );
				})
	            {
	                text = "EDIT ENTITY",
	                style = { backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.9f)) }
	            };
	            _entityControlsContainer.Add(editButton);
	        }
	        else
	        {
	            var helpBox = new HelpBox(
	                "Bound Entities on prefabs can only be edited by opening the prefab in the prefab editor.", 
	                HelpBoxMessageType.Info
	            );
	            _entityControlsContainer.Add(helpBox);

	            var openPrefabButton = new Button(() =>
	            {
	                AssetDatabase.OpenAsset(_owner);
	                // GraphEditor.OpenWindow(_owner);
	            })
	            {
	                text = "OPEN PREFAB AND EDIT",
	                style = { backgroundColor = EditorGUIUtility.isProSkin ? 
	                         new StyleColor(new Color(0.8f, 0.8f, 1)) : 
	                         new StyleColor(Color.white) }
	            };
	            _entityControlsContainer.Add(openPrefabButton);
	        }

	        if (!Application.isPlaying)
	        {
	            if (!_owner.EntityIsBound)
	            {
	                var bindButton = new Button(() =>
	                {
	                    if (EditorUtility.DisplayDialog(
	                        "Bind Entity",
	                        "This will make a local copy of the entity, bound to the owner.\n\n" +
	                        "This allows you to make local changes and assign scene object references directly.\n\n" +
	                        "Note that you can also use scene object references through the use of Blackboard Variables.\n\n" +
	                        "Bind Entity?",
	                        "YES",
	                        "NO"))
	                    {
	                        AssetToBound();
	                    }
	                })
	                {
	                    text = "Bind Entity"
	                };
	                _entityControlsContainer.Add(bindButton);
	            }
	            else
	            {
	                var deleteButton = new Button(() =>
	                {
	                    if (EditorUtility.DisplayDialog("Delete Bound Entity", "Are you sure?", "YES", "NO"))
	                    {
							DestroyImmediate(_owner.EntityAsset, true);
	                        _owner.SetBoundEntityReference(null);
	                    }
	                })
	                {
	                    text = "Delete Bound Entity",
	                    style = { backgroundColor = new StyleColor(Color.red) }
	                };
	                _entityControlsContainer.Add(deleteButton);
	            }
	        }

	        _rootElement.Add(_entityControlsContainer);
	    }
		
		private void CreateStandardFields()
		{
			if (Application.isPlaying || !_owner.EntityIsBound)
			{
				_entityAssetField = new PropertyField(_entityAssetProp, "Entity");
				_rootElement.Add(_entityAssetField);
			}
		}
		
		private void CreateComponentsView()
		{
			var entityAsset = _owner.EntityAsset;
			if (entityAsset != null && entityAsset.GetEntityInstance().Components != null)
			{
				var componentsContainer = new VisualElement();
	                
				// 添加组件列表
				var componentsList = new ListView();
				// componentsList.makeItem = () => new ComponentElement();
				// componentsList.bindItem = (element, index) =>
				// {
				// 	var componentElement = element as ComponentElement;
				// 	var component = entityAsset.GetEntityInstance().Components[index];
				// 	componentElement.UpdateComponent(component, entityAsset.GetEntityInstance());
				// };
				componentsList.itemsSource = entityAsset.GetEntityInstance().Components;

				componentsContainer.Add(componentsList);
	                
				// 注册变更回调
				componentsList.RegisterCallback<ChangeEvent<object>>(evt =>
				{
					if (entityAsset.SelfSerialize())
					{
						_owner.OnAfterEntitySerialized(entityAsset);
						EditorUtility.SetDirty(_owner);
						serializedObject.ApplyModifiedProperties();
					}
				});

				_rootElement.Add(componentsContainer);
			}
		}
		
		private void CreatePlayModeUI()
		{
			// TODO:
			// if (targets.Length == 1)
			// {
			// 	var entityContainer = new VisualElement();
			// 	EntityDrawer.DrawEntityInVisualElement(_owner.Entity, entityContainer);
			// 	_rootElement.Add(entityContainer);
			// }
			// else
			// {
			// 	var entitiesContainer = new VisualElement();
			// 	var entities = targets
			// 				   .Select(t => ((BangEntity)t).Entity)
			// 				   .ToArray();
			// 	EntityDrawer.DrawMultipleEntitiesInVisualElement(entities, entitiesContainer);
			// 	_rootElement.Add(entitiesContainer);
			// }
			//
			// if (target != null)
			// {
			// 	EditorUtility.SetDirty(target);
			// }
		}
	}

	public class SpaceElement : VisualElement
	{
		public new class UxmlFactory : UxmlFactory<SpaceElement, SpaceElement.UxmlTraits> { }

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				ve.style.height = 10;
			}
		}

		public override bool canGrabFocus => false;
	}

}
