using System;
using Bang.Components;
using Bang.Entities;
using UnityEngine;


namespace Bang.Unity.Conversion {

	[DoNotPersistOnSave]
	public readonly struct GameObjectReferenceComponent : IModifiableComponent, IEquatable< GameObjectReferenceComponent > {
		
		public readonly GameObject GameObject;

		public GameObjectReferenceComponent( GameObject gameObject ) {
			GameObject = gameObject;
		}

		public bool Equals( GameObjectReferenceComponent other ) {
			return other.GameObject == GameObject;
		}

		public override bool Equals( object obj ) {
			if ( obj is GameObjectReferenceComponent reference ) return Equals( reference );
			return false;
		}

		public override int GetHashCode() {
			return GameObject.GetHashCode();
		}
	}


	// public static class EntityGameObjectReferenceExtensions {
	// 	
	// 	public static global::Bang.Unity.Conversion.GameObjectReferenceComponent GetGameObjectRef(this global::Bang.Entities.Entity e)
	// 		=> e.GetComponent<global::Bang.Unity.Conversion.GameObjectReferenceComponent>(global::Bang.Entities.BangUnityComponentTypes.GameObjectReference);
	// 	
	// 	public static bool HasGameObjectRef(this global::Bang.Entities.Entity e)
	// 		=> e.HasComponent(global::Bang.Entities.BangUnityComponentTypes.GameObjectReference);
	// 	
	// 	public static global::Bang.Unity.Conversion.GameObjectReferenceComponent? TryGetGameObjectRef(this global::Bang.Entities.Entity e)
	// 		=> e.HasGameObjectReference() ? e.GetGameObjectReference() : null;
	//
	// 	public static void SetGameObjectRef(this global::Bang.Entities.Entity e, UnityEngine.GameObject gameObject)
	// 	{
	// 		e.AddOrReplaceComponent(new global::Bang.Unity.Conversion.GameObjectReferenceComponent(gameObject), global::Bang.Entities.BangUnityComponentTypes.GameObjectReference);
	// 	}
	// 	
	// 	public static bool RemoveGameObjectRef(this global::Bang.Entities.Entity e)
	// 		=> e.RemoveComponent(global::Bang.Entities.BangUnityComponentTypes.GameObjectReference);
	// }
	
}
