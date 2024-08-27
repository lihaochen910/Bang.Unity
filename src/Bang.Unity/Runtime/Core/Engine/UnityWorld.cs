using System.Collections.Generic;
using System.Collections.Immutable;
using Bang.Systems;
using Bang.Unity.Graphics;


namespace Bang.Unity {

	public class UnityWorld : World {
		
		// public GlobalMonoBehaviourOnGuiComponent? GlobalMonoBehaviourOnGuiComponent { get; protected set; }

		public UnityWorld( IList< (ISystem system, bool isActive) > systems ) : base( systems ) {
			
			// var builder = ImmutableArray.CreateBuilder< UnityMonoBehaviourOnGuiFunction >();
			// foreach ( var cachedRenderSystemKV in _cachedRenderSystems ) {
			// 	if ( cachedRenderSystemKV.Value.System is IGuiSystem guiSystem ) {
			// 		builder.Add( guiSystem.DrawGui );
			// 	}
			// }
			//
			// if ( builder.Count > 0 ) {
			// 	GlobalMonoBehaviourOnGuiComponent = new GlobalMonoBehaviourOnGuiComponent( builder.ToImmutableArray() );
			// 	AddEntity( GlobalMonoBehaviourOnGuiComponent );
			// }
			
		}
		
		public override void Pause() {
			base.Pause();
			Game.Pause();
		}

		public override void Resume() {
			base.Resume();
			Game.Resume();
		}

		public void DrawGui() {
			// TODO: Do not make a copy every frame.
			foreach (var (systemId, (system, contextId)) in _cachedRenderSystems)
			{
				if (system is IGuiSystem guiSystem)
				{
					if (DIAGNOSTICS_MODE)
					{
						_stopwatch.Reset();
						_stopwatch.Start();
					}

					guiSystem.DrawGui(Contexts[contextId]);

					if (DIAGNOSTICS_MODE)
					{
						InitializeDiagnosticsCounters();

						_stopwatch.Stop();
						// GuiCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, Contexts[contextId].Entities.Length);
					}
				}
			}
		}
		
	}
	
}
