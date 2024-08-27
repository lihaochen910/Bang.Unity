using System.Collections.Immutable;
using Bang.Entities;
using Bang.StateMachines;
using Bang.Systems;
using Bang.Unity.Components;


namespace Bang.Unity.Systems {

	[Watch( typeof( IStateMachineComponent ) )]
	[Filter( typeof( DestroyEntityDuringCoroutineFinishedComponent ) )]
	public class DestroyEntityDuringCoroutineFinishedSystem : IReactiveSystem {

		public void OnAdded( World world, ImmutableArray< Entity > entities ) {}

		public void OnRemoved( World world, ImmutableArray< Entity > entities ) {
			foreach ( var entity in entities ) {
				entity.Destroy();
			}
		}

		public void OnModified( World world, ImmutableArray< Entity > entities ) {}
	}

}
