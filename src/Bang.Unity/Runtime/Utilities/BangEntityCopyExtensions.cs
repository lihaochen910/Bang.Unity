using System.Linq;
using Bang.Contexts;
using Bang.Entities;
using UnityEngine;


namespace Bang.Unity.Utilities {

	public static class BangEntityCopyExtensions {
		
		/// <summary>
		/// 返回指定Entity的克隆体(使用深度拷贝)
		/// </summary>
		public static Entity Clone( this Entity entity, World world ) {
			if ( entity.IsDestroyed ) {
				Debug.LogError( $"cannot create clone of destroyed entity: {entity.EntityId}" );
				return null;
			}

			if ( entity.TryGetGameObjectReference() is {} gameObjectReference ) {
				return Object.Instantiate( gameObjectReference.GameObject ).GetEntity();
			}

			// TODO:
			// var componentsCopy = EntityBuilder.CreateComponentsCopy( entity.Components );
			// return world.AddEntity( componentsCopy );
			return null;
		}

	    
		public static Entity Clone( this Entity entity, Context context ) => Clone( entity, context.World );


		/// <summary>
		/// 返回指定Entity的克隆体(使用浅拷贝)
		/// </summary>
		public static Entity ShallowClone( this Entity entity, World world ) {
			if ( entity.IsDestroyed ) {
				Debug.LogError( $"cannot create clone of destroyed entity: {entity.EntityId}" );
				return null;
			}

			// EntityBuilder.PushComponentCopyMode( EntityBuilder.ComponentCopyMode.ShallowCopy );
			// var componentsCopy = EntityBuilder.CreateComponentsCopy( entity.Components );
			// EntityBuilder.PopComponentCopyMode();
	        
			return world.AddEntity( entity.Components.ToArray() );
		}
	    
	    
		public static Entity ShallowClone( this Entity entity, Context context ) => ShallowClone( entity, context.World );

		
		/// <summary>
		/// Get Position in world position(safely).
		/// </summary>
		public static Vector3? TryGetPosition( this Entity entity ) {
			if ( entity.TryGetGameObjectReference() is {} gameObjectReference ) {
				return gameObjectReference.GameObject?.transform.position;
			}

			return null;
		}


		/// <summary>
		/// Get Position in world position(unsafe).
		/// </summary>
		public static Vector3 GetPosition( this Entity entity ) {
			return entity.GetGameObjectReference().GameObject.transform.position;
		}


		public static void SetPosition( this Entity entity, in Vector3 position ) {
			entity.GetGameObjectReference().GameObject.transform.position = position;
		}


		public static Camera? TryGetCamera( this Entity entity ) {
			return entity.GetGameObjectReference().GameObject.GetComponent< Camera >();
		}

		
		public static Camera GetCamera( this Entity entity ) {
			return entity.GetGameObjectReference().GameObject.GetComponent< Camera >();
		}
		
	}
	
}
