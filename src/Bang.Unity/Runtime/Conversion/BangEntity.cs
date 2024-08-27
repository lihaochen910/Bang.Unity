using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bang.Entities;
using Bang.Unity.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using Entity = Bang.Entities.Entity;


namespace Bang.Unity {

	public enum EntityDestroyFollowPolicy : byte {
		DestroyGameObjectWhenEntityDestroyed,
		DoNothing
	}

	[AddComponentMenu("Bang/BangEntity")]
	[DefaultExecutionOrder(-100)]
	[DisallowMultipleComponent]
	public class BangEntity : MonoBehaviour {
		
		// [SerializeField]
		// private EntityConversionOptions _options;

		[SerializeField]
		private EntityDestroyFollowPolicy _entityDestroyFollowPolicy = EntityDestroyFollowPolicy.DestroyGameObjectWhenEntityDestroyed;
		
		[SerializeField]
		private EntityAsset _entityAsset;
		
		[SerializeField, FormerlySerializedAs("boundEntitySerialization")]
		private string _boundEntitySerialization;
		
		[SerializeField, FormerlySerializedAs("boundEntityObjectReferences")]
		private List<UnityEngine.Object> _boundEntityObjectReferences;
		
		private EntityInstance _boundEntityInstance = new ();
		
		[Tooltip("If enabled, bound graph prefab overrides in instances will not be possible")]
		public bool LockBoundEntityPrefabOverrides = true;

		[NonSerialized]
		public int EntityId;

		public Entity Entity => _entity;
		private Entity _entity;
		
		internal World World { get; set; }
		internal bool UseDisabledComponent { get; set; }
		
		///<summary>The current behaviour Graph assigned</summary>
		public EntityAsset EntityAsset {
			get
			{
#if UNITY_EDITOR
				//In Editor only and if graph is bound, return the bound graph instance
				if ( EntityIsBound && !Application.isPlaying ) {
					return _boundEntityAssetInstance;
				}
#endif
				//In runtime an instance of either boundGraphSerialization json or Asset Graph is created in awake
				return _entityAsset;
			}
			set => _entityAsset = value;
		}
		
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool IsEntityAlive() => World != null && !_entity.IsDestroyed;
		
		///<summary>Do we have a bound entity serialization?</summary>
		public bool EntityIsBound => !string.IsNullOrEmpty(_boundEntitySerialization);

		public string BoundEntitySerialization => _boundEntitySerialization;

		private void Awake() {
			// var entity = EntityConversion.Convert( gameObject, _options );
			if ( _entityAsset != null ) {
				_entity = _entityAsset.CreateInstance( Game.ActiveScene?.World );
				EntityId = _entity.EntityId;
				World = _entity.World;
			}
			else {
				JSONSerializer.TryDeserializeOverwrite< EntityInstance >( _boundEntityInstance, _boundEntitySerialization, _boundEntityObjectReferences );
				if ( _boundEntityInstance != null && Game.ActiveScene?.World != null ) {
					_entity = Game.ActiveScene?.World.AddEntity( _boundEntityInstance.Components.ToArray() );
					EntityId = _entity.EntityId;
					World = _entity.World;
				}
			}

			if ( _entity != null ) {
				_entity.SetGameObjectReference( gameObject );
				_entity.SetEntityName( gameObject.name );

				if ( _entityDestroyFollowPolicy is EntityDestroyFollowPolicy.DestroyGameObjectWhenEntityDestroyed ) {
					_entity.OnEntityDestroyed += OnEntityDestroyed;
				}
			}
			else {
				Debug.LogError( $"cannot create entity for GameObject: {gameObject.name}, BangWorld is null." );
				gameObject.SetActive( false );
			}
			
			// Debug.Log( "BangEntity Awake()!" );
		}

		void OnEnable() {
			if ( !IsEntityAlive() ) {
				return;
			}

			if ( UseDisabledComponent && _entity.HasGameObjectDisabled() ) {
				_entity.RemoveGameObjectDisabled();
			}
		}

		void OnDisable() {
			if ( !IsEntityAlive() ) {
				return;
			}

			if ( UseDisabledComponent && !_entity.HasGameObjectDisabled() ) {
				_entity.SetGameObjectDisabled();
			}
		}

		void OnDestroy() {
			if ( IsEntityAlive() ) {
				_entity.RemoveGameObjectReference();
				_entity.Destroy();
			}
		}

		private void OnEntityDestroyed( int entityId ) {
			_entity.OnEntityDestroyed -= OnEntityDestroyed;
			
			Destroy( gameObject );
		}

		public void ClearAllComponentsAndSetNew( EntityAsset asset ) {
			if ( _entity is null ) {
				return;
			}

			foreach ( var component in _entity.Components ) {
				_entity.RemoveComponent( component.GetType() );
			}

			var newComponents = asset.CreateComponents();
			foreach ( var newComponent in newComponents ) {
				_entity.AddComponent( newComponent, newComponent.GetType() );
			}

			_boundEntityAssetInstance = asset;
		}
		
		///----------------------------------------------------------------------------------------------
        ///---------------------------------------UNITY EDITOR-------------------------------------------
        ///----------------------------------------------------------------------------------------------
#if UNITY_EDITOR

        protected EntityAsset _boundEntityAssetInstance;

        ///<summary>Editor. Called after assigned graph is serialized.</summary>
        public void OnAfterEntitySerialized( EntityAsset serializedEntityAsset ) {
			
            //If the entity is bound, we store the serialization data here.
            if ( EntityIsBound && _boundEntityAssetInstance == serializedEntityAsset ) {
       
                //--- This is basically only for showing the log...
                if ( UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this) ) {
                    var boundProp = new UnityEditor.SerializedObject(this).FindProperty(nameof(_boundEntitySerialization));
                    if ( !boundProp.prefabOverride && _boundEntitySerialization != serializedEntityAsset.GetSerializedJsonData() ) {
                        if ( LockBoundEntityPrefabOverrides ) {
                            Debug.LogWarning("The Bound Graph is Prefab Locked!\nChanges you make are not saved!\nUnlock the Prefab Instance, or Edit the Prefab Asset.");
                            return;
                        }
						else {
							Debug.LogWarning("Prefab Bound Graph just got overridden!");
                        }
                    }
				}
       
                // ParadoxNotion.Design.UndoUtility.RecordObject(this, ParadoxNotion.Design.UndoUtility.GetLastOperationNameOr("Bound Graph Change"));
                _boundEntityInstance = serializedEntityAsset.GetEntityInstance();
                _boundEntitySerialization = serializedEntityAsset.GetSerializedJsonData();
                _boundEntityObjectReferences = serializedEntityAsset.GetSerializedReferencesData();
                // ParadoxNotion.Design.UndoUtility.SetDirty(this);
            }
        }

		///<summary>Editor. Validate.</summary>
		protected void OnValidate() {
			Validate();
		}

		///<summary>Editor. Validate.</summary>
        public void Validate() {

            if ( !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ) {
                //everything here is relevant to bound graphs only.
                //we only do this for when the object is an instance or is edited in the prefab editor.
                if ( !UnityEditor.EditorUtility.IsPersistent( this ) && EntityIsBound ) {

                    if ( _boundEntityAssetInstance == null ) {
						_boundEntityAssetInstance = ( EntityAsset )ScriptableObject.CreateInstance( typeof( EntityAsset ) );
					}

                    _boundEntityAssetInstance.name = nameof( EntityAsset );
                    // boundEntityAssetInstance.SetGraphSourceMetaData(this.boundGraphSource);
					_boundEntityAssetInstance.Deserialize( _boundEntitySerialization, _boundEntityObjectReferences, false );
					_boundEntityAssetInstance.UpdateReferencesFromOwner(this);
					_boundEntityAssetInstance.Validate();
				}
				else if ( _entityAsset != null ) {
					_entityAsset.UpdateReferencesFromOwner( this );
                    _entityAsset.Validate();
                }

            }
        }

        ///<summary>Editor. Binds the target entity (null to delete current bound).</summary>
        public void SetBoundEntityReference( EntityAsset target ) {
        
            if ( UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode ) {
                Debug.LogError("SetBoundGraphReference method is an Editor only method!");
                return;
            }
        
            //cleanup
            _entityAsset = null;
			
            if ( target == null ) {
                _boundEntityInstance = null;
                _boundEntitySerialization = null;
                _boundEntityObjectReferences = null;
                return;
            }
        
            //serialize target and store boundGraphSerialization data
            target.SelfSerialize();
            _boundEntitySerialization = target.GetSerializedJsonData();
            _boundEntityObjectReferences = target.GetSerializedReferencesData();
			// _boundEntityInstance
            Validate(); //validate to handle bound graph instance
        }

        ///<summary>Reset unity callback</summary>
        protected void Reset() {
            
        }

        //...
        virtual protected void OnDrawGizmos() {

        } 

        ///<summary>Forward Gizmos callback</summary>
        virtual protected void OnDrawGizmosSelected() {
            // if ( Editor.GraphEditorUtility.activeElement != null ) {
            //     var rootElement = Editor.GraphEditorUtility.activeElement.graph.GetFlatMetaGraph().FindReferenceElement(Editor.GraphEditorUtility.activeElement);
            //     if ( rootElement != null ) {
            //         foreach ( var task in rootElement.GetAllChildrenReferencesOfType<Task>() ) {
            //             task.OnDrawGizmosSelected();
            //         }
            //     }
            // }
        }
#endif

	}


	public static class BangEntityExtensions {

		public static Entity GetEntity( this GameObject go ) {
			if ( go.GetComponent< BangEntity >() is {} bangEntity ) {
				return bangEntity.Entity;
			}

			return null;
		}
		
		public static Entity? TryGetEntity( this GameObject go ) {
			if ( go.GetComponent< BangEntity >() is {} bangEntity ) {
				return bangEntity.Entity;
			}

			return null;
		}
		
		public static BangEntity GetBangEntity( this GameObject go ) {
			return go.GetComponent< BangEntity >();
		}
		
	}

}
