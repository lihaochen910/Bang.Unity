using System.Collections.Generic;
using System.Linq;
using Bang.Components;
using Bang.Entities;
using Bang.Unity.Serialization;
using Bang.Unity.Utilities;
using UnityEngine;


namespace Bang.Unity {

	[CreateAssetMenu(menuName = "Bang/EntityAsset")]
	public class EntityAsset : ScriptableObject, ISerializationCallbackReceiver {

		///<summary>Entity category</summary>
		public string Category;

		///<summary>Entity Comments</summary>
		public string Comments;
		
		// the json EntityInstance
		[SerializeField]
		private string _serializedEntity;
		
		//the unity references used for json components
		[SerializeField]
		private List<UnityEngine.Object> _objectReferences;
		
		//the actual entity data. Mixed serialized by Unity/Json
		private EntityInstance _entityInstance = new ();
		
		///<summary>The host currently used by the entity</summary>
		public Component Owner { get; private set; }

		void ISerializationCallbackReceiver.OnBeforeSerialize() {
			SelfSerialize();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize() {
			SelfDeserialize();
		}

		public Entity CreateInstance( World world ) {
			SelfDeserialize();
			return world.AddEntity( _entityInstance.Components.ToArray() );
		}

		public IComponent[] CreateComponents() {
			SelfDeserialize();
			return _entityInstance.Components.ToArray();
		}
		
		///<summary>Serialize the Graph. Return if serialization changed</summary>
	    public bool SelfSerialize() {

			var newReferences = new List< UnityEngine.Object >();
			var newSerialization = Serialize( newReferences );
	        if ( newSerialization != _serializedEntity || !newReferences.SequenceEqual(_objectReferences) ) {
				
	            //store
	            _serializedEntity = newSerialization;
	            _objectReferences = newReferences;

	#if UNITY_EDITOR

	            // if ( _externalSerializationFile != null ) {
	            //     var externalSerializationFilePath = ParadoxNotion.Design.EditorUtils.AssetToSystemPath(UnityEditor.AssetDatabase.GetAssetPath(_externalSerializationFile));
	            //     System.IO.File.WriteAllText(externalSerializationFilePath, JSONSerializer.PrettifyJson(newSerialization));
	            // }

	            //notify owner (this is basically used for bound graphs)
	            // var owner = agent as GraphOwner;
	            // if ( owner != null ) {
	            //     owner.OnAfterGraphSerialized(this);
	            // }
	#endif

	            //raise event
	            // if ( onGraphSerialized != null ) {
	            //     onGraphSerialized(this);
	            // }

	            //purge cache and refs
	            // graphSource.PurgeRedundantReferences();
	            // flatMetaGraph = null;
	            // fullMetaGraph = null;
	            // nestedMetaGraph = null;

	            return true;
	        }

	        return false;
	    }

	    ///<summary>Deserialize the Graph. Return if that succeed</summary>
	    public bool SelfDeserialize() {
			if ( Deserialize( _serializedEntity, _objectReferences, false ) ) {
				//raise event
				// if ( onGraphDeserialized != null ) {
				//     onGraphDeserialized(this);
				// }
				return true;
			}

			return false;
	    }

		///----------------------------------------------------------------------------------------------

		///<summary>Serialize the graph and returns the serialized json string. The provided objectReferences list will be cleared and populated with the found unity object references.</summary>
		public string Serialize( List< UnityEngine.Object > references ) {
			if ( references == null ) {
				references = new List< Object >();
			}

			// UpdateNodeIDs(true);
			var result = JSONSerializer.Serialize( typeof( EntityInstance ), _entityInstance, references );
			return result;
		}

		///<summary>Deserialize the json serialized graph provided. Returns the data or null if failed.</summary>
	    //The provided references list will be used to read serialized unity object references.
	    //IMPORTANT: Validate should be called true in all deserialize cases outside of Unity's 'OnAfterDeserialize',
	    //like for example when loading from json, or manualy calling this outside of OnAfterDeserialize.
	    //Otherwise, Validate can also be called separately.
	    public bool Deserialize( string serializedEntity, List<UnityEngine.Object> references, bool validate ) {
			if ( string.IsNullOrEmpty( serializedEntity ) ) {
				Debug.LogWarning( "JSON is null or empty on entity asset when deserializing." );
				return false;
			}

			//the list to load the references from. If not provided explicitely we load from the local list
			if ( references == null ) {
				references = _objectReferences;
			}

			try {
	            //deserialize provided serialized graph into a new GraphSerializationData object and load it
				JSONSerializer.TryDeserializeOverwrite< EntityInstance >( _entityInstance, serializedEntity, references );
	            // if ( graphSource.type != this.GetType().FullName ) {
	            //     Debug.LogError("Can't Load graph because of different Graph type serialized and required.", LogTag.SERIALIZATION, this);
	            //     _haltSerialization = true;
	            //     return false;
	            // }

	            // _entityInstance = graphSource.Unpack(this);
	            _serializedEntity = serializedEntity;
	            _objectReferences = references;
	            // if ( validate ) { Validate(); }
	            return true;
	        }

	        catch ( System.Exception e ) {
				Debug.LogException( e );
	            return false;
	        }
	    }

		///----------------------------------------------------------------------------------------------
		///<summary>Returns the GraphSource object itself</summary>
		public EntityInstance GetEntityInstance() {
			return _entityInstance;
		}

		///<summary>Returns the serialization json</summary>
		public string GetSerializedJsonData() {
			return _serializedEntity;
		}

		///<summary>Return a copy of the serialized Unity object references</summary>
		public List< UnityEngine.Object > GetSerializedReferencesData() {
			return _objectReferences?.ToList();
		}
		
		///<summary>Validate the graph, it's nodes and tasks. Also called from OnEnable callback.</summary>
		public void Validate() {

			if ( string.IsNullOrEmpty(_serializedEntity) ) {
				//we dont really have anything to validate in this case
				return;
			}

#if UNITY_EDITOR
			if ( !Threader.applicationIsPlaying ) {
				UpdateReferences( Owner, true );
			}
#endif
			
		}

		///<summary>See UpdateReferences</summary>
		public void UpdateReferencesFromOwner( BangEntity owner, bool force = false ) {
			UpdateReferences( owner, force );
		}

		///<summary>Update the Agent/Component and Blackboard references. This is done when the graph initialize or start, and in the editor for convenience.</summary>
		public void UpdateReferences( Component newOwner, bool force = false ) {
			if ( !ReferenceEquals( Owner, newOwner ) || force ) {
				Owner = newOwner;
			}
		}

	}


	///<summary>Graph data and model used for serialization</summary>
	[System.Serializable, fsDeserializeOverwrite]
	public class EntityInstance : ISerializationCollector {

		///----------------------------------------------------------------------------------------------
		///<summary>We are already parsing everything on serialization/deserialization, so we might just as well collect things at the same time.</summary>
		public List< IComponent > Components = new ();

		void ISerializationCollector.OnPush(ISerializationCollector parent) {
			Components ??= new List<IComponent>();
		}

		void ISerializationCollector.OnCollect(ISerializationCollectable child, int depth) {
			if ( child is IComponent ) {
				Components.Add( ( IComponent )child );
			}
		}

		void ISerializationCollector.OnPop(ISerializationCollector parent) {}
		///----------------------------------------------------------------------------------------------
		
	}
	
}
