using System.Collections.Generic;
using System.Collections.Immutable;
using Bang.Components;
using Bang.Entities;


namespace Bang.Unity.Components {

	[RuntimeOnly, DoNotPersistOnSave]
	public readonly struct CollisionCacheComponent : IComponent {

		/// <summary>
		/// Id of the entity that caused this collision.
		/// </summary>
		public readonly ImmutableHashSet< int > CollidingWith;
		
		public CollisionCacheComponent( int id ) => CollidingWith = ImmutableHashSet< int >.Empty.Add( id );
	
		public CollisionCacheComponent( ImmutableHashSet< int > idList ) => CollidingWith = idList;


		public bool Contains< T >( World world ) where T : IComponent {
			foreach ( var id in CollidingWith ) {
				if ( world.TryGetEntity( id ) is {} entity && entity.HasComponent< T >() ) {
					return true;
				}
			}

			return false;
		}

		public IEnumerable< Entity > GetCollidingEntities( World world ) {
			foreach ( var id in CollidingWith ) {
				var entity = world.TryGetEntity( id );
				if ( entity is { IsDestroyed: false } ) {
					yield return entity;
				}
			}
		}

		public bool HasId( int id ) => CollidingWith.Contains( id );

		public CollisionCacheComponent Remove( int id ) {
			return new ( CollidingWith.Remove( id ) );
		}

		public CollisionCacheComponent Add( int id ) {
			return new ( CollidingWith.Add( id ) );
		}

	}

}
