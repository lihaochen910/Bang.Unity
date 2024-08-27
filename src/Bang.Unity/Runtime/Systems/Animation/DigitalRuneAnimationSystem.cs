using Bang.Components;
using Bang.Contexts;
using Bang.Entities;
using Bang.Systems;
using Bang.Unity.Services;


namespace Bang.Unity.Systems {

	[WorldEditor( startActive: true )]
	// [Messager(typeof( DRAnimationFinishedMessage ) )]
	[Filter]
	public class DigitalRuneAnimationSystem : IStartupSystem, IExitSystem, IUpdateSystem/*, IMessagerSystem*/ {

		public void Start( Context context ) {
			if ( DigitalRuneAnimationServices.AnimationManager is null ) {
				DigitalRuneAnimationServices.Initialize( enableMultiThread: false );
			}
		}
	
		public void Exit( Context context ) {
			// DRAnimationServices.Uninitialize();
		}

		public void Update( Context context ) {
			DigitalRuneAnimationServices.Tick( Game.DeltaTime );
		}
	
		public void OnMessage( World world, Entity entity, IMessage message ) {
		
		}
	
	}

}
