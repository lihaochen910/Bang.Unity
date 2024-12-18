using System;
using System.Collections.Immutable;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Bang.Unity.Components;
using Bang.Unity.Messages;


namespace Bang.Unity.Systems {

	[Watch( typeof( EntityDestroyListenerComponent ) )]
	public class EntityDestroyMessageBroadcastSystem : IStartupSystem, IReactiveSystem {

		private WeakReference< World > _worldRef;
		
		public void Start( Context context ) {
			_worldRef = new WeakReference< World >( context.World );
		}

		public void OnAdded( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				entity.OnEntityDestroyed += OnEntityDestroyed;
			}
		}

		public void OnRemoved( World world, ImmutableArray< Entity > entities ) {}

		public void OnModified( World world, ImmutableArray< Entity > entities ) {}

		public void OnBeforeRemoving( World world, Entity entity, int index, bool causedByDestroy ) {
			if ( !causedByDestroy ) {
				entity.OnEntityDestroyed -= OnEntityDestroyed;
			}
			else {
				entity.SendMessage< EntityWillBeDestroyMessage >();
			}
		}

		private void OnEntityDestroyed( int entityId ) {
			if ( _worldRef.TryGetTarget( out var world ) &&
				 world.TryGetEntity( entityId ) is {} destroyedEntity ) {
				destroyedEntity.OnEntityDestroyed -= OnEntityDestroyed;
				destroyedEntity.SendMessage< EntityDestroyedMessage >();
			}
		}

	}

}
