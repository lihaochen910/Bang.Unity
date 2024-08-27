using System.Collections.Generic;
using Bang.Entities;
using Bang.StateMachines;
using Bang.Unity.StateMachines;


namespace Bang.Unity.Services {

	public static class CoroutineServices {
		
		public static void RunCoroutine(this World world, IEnumerator<Wait> routine)
		{
			// TODO: Figure out object pulling of entities here.
			Entity e = world.AddEntity(
				new StateMachineComponent<Coroutine>(new Coroutine(routine)));
			
			e.SetDestroyEntityDuringCoroutineFinished();

			// Immediately run the first tick!
			e.GetStateMachine().Tick(Game.DeltaTime);
		}

		public static void RunCoroutine(this Entity e, IEnumerator<Wait> routine)
		{
			e.SetStateMachine(new StateMachineComponent<Coroutine>(new Coroutine(routine)));
			e.SetDestroyEntityDuringCoroutineFinished();

			// Immediately run the first tick!
			e.GetStateMachine().Tick(Game.DeltaTime);
		}
		
	}

}
