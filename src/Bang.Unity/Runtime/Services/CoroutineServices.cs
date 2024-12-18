using System.Collections.Generic;
using Bang.Entities;
using Bang.StateMachines;
using Bang.Unity.StateMachines;


namespace Bang.Unity.Services {

	public static class CoroutineServices {

		public static Entity RunCoroutine( this World world, IEnumerator< Wait > routine, bool autoDestroy = false ) {
			// TODO: Figure out object pulling of entities here.
			Entity e = world.AddEntity(
				new StateMachineComponent< Coroutine >( new Coroutine( routine ) ) );

			if ( autoDestroy ) {
				e.SetDestroyEntityDuringCoroutineFinished();
				// e.AddOrReplaceComponent(new global::Bang.Unity.Components.DestroyEntityDuringCoroutineFinishedComponent(), global::Bang.Entities.BangUnityComponentTypes.DestroyEntityDuringCoroutineFinished);
			}

			// Immediately run the first tick!
			e.GetStateMachine().Tick( Game.DeltaTime );

			return e;
		}

		public static Entity RunCoroutine( this Entity e, IEnumerator< Wait > routine, bool autoDestroy = false ) {
			e.SetStateMachine( new StateMachineComponent< Coroutine >( new Coroutine( routine ) ) );

			if ( autoDestroy ) {
				e.SetDestroyEntityDuringCoroutineFinished();
				// e.AddOrReplaceComponent(new global::Bang.Unity.Components.DestroyEntityDuringCoroutineFinishedComponent(), global::Bang.Entities.BangUnityComponentTypes.DestroyEntityDuringCoroutineFinished);
			}

			// Immediately run the first tick!
			e.GetStateMachine().Tick( Game.DeltaTime );

			return e;
		}

	}

}
