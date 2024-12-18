// #define DEBUG_FRAME_UPDATE
using System.Collections.Generic;
using Bang.Contexts;
using Bang.Systems;
using Bang.Unity.Graphics;
using Unity.Profiling;
#if DEBUG_FRAME_UPDATE
using UnityEngine;
#endif


namespace Bang.Unity {

	public partial class UnityWorld : World {
		
		private static readonly ProfilerMarker k_NotifyReactiveSystemsMarker = new ( "UnityWorld.NotifyReactiveSystems" );
		private readonly Dictionary< int, ProfilerMarker > k_EarlyStartupSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_StartupSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ExitSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_UpdateSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_LateUpdateSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_FixedUpdateSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_UnityDrawSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_UnityGuiSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_UnityGizmosSystemMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ReactiveSystemOnAddedMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ReactiveSystemOnRemovedMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ReactiveSystemOnModifiedMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ReactiveSystemOnBeforeRemovingMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ReactiveSystemOnBeforeModifyingMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ReactiveSystemOnActivatedMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_ReactiveSystemOnDeactivatedMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_OnSystemActivatedMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_OnSystemDeactivatedMarkers = new ();
		private readonly Dictionary< int, ProfilerMarker > k_IMessagerSystemOnMessageMarkers = new ();

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

			if ( DIAGNOSTICS_MODE ) {
				foreach ( var (systemId, (system, _)) in _cachedEarlyStartupSystems ) {
					k_EarlyStartupSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.EarlyStart()" ) );
				}
				foreach ( var (systemId, (system, _)) in _cachedStartupSystems ) {
					k_StartupSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.Start()" ) );
				}
				foreach ( var (systemId, (system, _)) in _cachedExitSystems ) {
					k_ExitSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.Exit()" ) );
				}
				foreach ( var (systemId, (system, _)) in _cachedExecuteSystems ) {
					k_UpdateSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.Update()" ) );
				}
				foreach ( var (systemId, (system, _)) in _cachedLateExecuteSystems ) {
					k_LateUpdateSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.LateUpdate()" ) );
				}
				foreach ( var (systemId, (system, _)) in _cachedFixedExecuteSystems ) {
					k_FixedUpdateSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.FixedUpdate()" ) );
				}

				foreach ( var (systemId, system) in IdToSystem ) {
					if ( system is IReactiveSystem ) {
						k_ReactiveSystemOnAddedMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnAdded()" ) );
						k_ReactiveSystemOnRemovedMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnRemoved()" ) );
						k_ReactiveSystemOnModifiedMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnModified()" ) );
						k_ReactiveSystemOnBeforeRemovingMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnBeforeRemoving()" ) );
						k_ReactiveSystemOnBeforeModifyingMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnBeforeModifying()" ) );
						k_ReactiveSystemOnActivatedMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnActivated()" ) );
						k_ReactiveSystemOnDeactivatedMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnDeactivated()" ) );
					}

					if ( system is IActivateAndDeactivateListenerSystem ) {
						k_OnSystemActivatedMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnActivated()" ) );
						k_OnSystemDeactivatedMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnDeactivated()" ) );
					}

					if ( system is IMessagerSystem ) {
						k_IMessagerSystemOnMessageMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.OnMessage()" ) );
					}

					if ( system is IDrawSystem ) {
						k_UnityDrawSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.Draw()" ) );
					}
					
					if ( system is IGuiSystem ) {
						k_UnityGuiSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.DrawGui()" ) );
					}
					
					if ( system is IGizmosSystem ) {
						k_UnityGizmosSystemMarkers.Add( systemId, new ProfilerMarker( ProfilerCategory.Scripts, $"{system.GetType().Name}.DrawGizmos()" ) );
					}
				}
			}
			
		}

		// public override void Dispose() {
		// 	base.Dispose();
		// }

		public override void Pause() {
			base.Pause();
			Game.Pause();
		}

		public override void Resume() {
			base.Resume();
			Game.Resume();
		}

		public override void EarlyStart() {

			foreach ( var (systemId, (system, contextId)) in _cachedEarlyStartupSystems ) {
				if ( DIAGNOSTICS_MODE ) {
					_stopwatch.Reset();
					_stopwatch.Start();
					k_EarlyStartupSystemMarkers[ systemId ].Begin();
				}

				system.EarlyStart( Contexts[ contextId ] );

				if ( DIAGNOSTICS_MODE ) {
					k_EarlyStartupSystemMarkers[ systemId ].End();
					
					InitializeDiagnosticsCounters();

					_stopwatch.Stop();
#if NET6_0_OR_GREATER
                    EarlyStartCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, Contexts[contextId].Entities.Length);
#else
					EarlyStartCounters[systemId].Update(_stopwatch.Elapsed.TotalMilliseconds, Contexts[contextId].Entities.Length);
#endif
				}
			}

		}
		
		public override void Start() {
			foreach ( var (systemId, (system, contextId)) in _cachedStartupSystems ) {
				if ( DIAGNOSTICS_MODE ) {
					_stopwatch.Reset();
					_stopwatch.Start();
					k_StartupSystemMarkers[ systemId ].Begin();
				}

				system.Start( Contexts[ contextId ] );

				// Track that this system has been started (only once).
				_systemsInitialized.Add( systemId );

				if ( DIAGNOSTICS_MODE ) {
					k_StartupSystemMarkers[ systemId ].End();
					
					InitializeDiagnosticsCounters();

					_stopwatch.Stop();
#if NET6_0_OR_GREATER
                    StartCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, Contexts[contextId].Entities.Length);
#else
					StartCounters[systemId].Update(_stopwatch.Elapsed.TotalMilliseconds, Contexts[contextId].Entities.Length);
#endif
				}
			}

			NotifyReactiveSystems();
			DestroyPendingEntities();
			ActivateOrDeactivatePendingSystems();
		}

		public override void Update() {
			
#if DEBUG_FRAME_UPDATE
			Debug.Log( $"[{Game.Frame}] Update Begin." );
#endif

			foreach ( var (systemId, (system, contextId)) in _cachedExecuteSystems ) {
				if ( DIAGNOSTICS_MODE && k_UpdateSystemMarkers.ContainsKey( systemId ) ) {
					_stopwatch.Reset();
					_stopwatch.Start();
					k_UpdateSystemMarkers[ systemId ].Begin();
				}
				
				Context context = Contexts[ contextId ];
#if DEBUG_FRAME_UPDATE
				try {
					system.Update( context );
				}
				catch ( UnityEngine.MissingReferenceException _ ) {
					Debug.Log( $"Update Error: frame_{Game.Frame}" );
					Debug.Break();
				}
#else
				system.Update( context );
#endif
				
				if ( DIAGNOSTICS_MODE && k_UpdateSystemMarkers.ContainsKey( systemId ) ) {
					k_UpdateSystemMarkers[ systemId ].End();
					
					InitializeDiagnosticsCounters();

					_stopwatch.Stop();
#if NET6_0_OR_GREATER
                    UpdateCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, context.Entities.Length);
#else
					UpdateCounters[systemId].Update(_stopwatch.Elapsed.TotalMilliseconds, context.Entities.Length);
#endif
				}
			}
			
#if DEBUG_FRAME_UPDATE
			Debug.Log( $"[{Game.Frame}] Update Finish." );
#endif

			NotifyReactiveSystems();
#if DEBUG_FRAME_UPDATE
			Debug.Log( $"[{Game.Frame}] NotifyReactiveSystems Finish." );
#endif
			DestroyPendingEntities();
#if DEBUG_FRAME_UPDATE
			Debug.Log( $"[{Game.Frame}] DestroyPendingEntities Finish." );
#endif
			ActivateOrDeactivatePendingSystems();
#if DEBUG_FRAME_UPDATE
			Debug.Log( $"[{Game.Frame}] ActivateOrDeactivatePendingSystems Finish." );
#endif

			// Clear the messages after the update so we can persist messages sent during Start().
			ClearMessages();
#if DEBUG_FRAME_UPDATE
			Debug.Log( $"[{Game.Frame}] FrameUpdate Finish." );
#endif
		}

		public override void LateUpdate() {
			foreach ( var (systemId, (system, contextId)) in _cachedLateExecuteSystems ) {
				if ( DIAGNOSTICS_MODE ) {
					_stopwatch.Reset();
					_stopwatch.Start();
					k_LateUpdateSystemMarkers[ systemId ].Begin();
				}

				// TODO: We want to run systems which do not cross components in parallel.
				system.LateUpdate( Contexts[ contextId ] );

				if ( DIAGNOSTICS_MODE ) {
					k_LateUpdateSystemMarkers[ systemId ].End();
					
					InitializeDiagnosticsCounters();

					_stopwatch.Stop();
#if NET6_0_OR_GREATER
                    LateUpdateCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, Contexts[contextId].Entities.Length);
#else
					LateUpdateCounters[systemId].Update(_stopwatch.Elapsed.TotalMilliseconds, Contexts[contextId].Entities.Length);
#endif
				}
			}
		}

		public override void FixedUpdate() {
			foreach ( var (systemId, (system, contextId)) in _cachedFixedExecuteSystems ) {
				if ( DIAGNOSTICS_MODE ) {
					_stopwatch.Reset();
					_stopwatch.Start();
					k_FixedUpdateSystemMarkers[ systemId ].Begin();
				}

				// TODO: We want to run systems which do not cross components in parallel.
				system.FixedUpdate( Contexts[ contextId ] );

				if ( DIAGNOSTICS_MODE ) {
					k_FixedUpdateSystemMarkers[ systemId ].End();
					
					InitializeDiagnosticsCounters();

					_stopwatch.Stop();
#if NET6_0_OR_GREATER
                    FixedUpdateCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, Contexts[contextId].Entities.Length);
#else
					FixedUpdateCounters[systemId].Update(_stopwatch.Elapsed.TotalMilliseconds, Contexts[contextId].Entities.Length);
#endif
				}
			}
		}

		public void Draw() {
			foreach ( var (systemId, (system, contextId)) in _cachedRenderSystems ) {
				if ( system is IDrawSystem drawSystem ) {
					if ( DIAGNOSTICS_MODE ) {
						k_UnityDrawSystemMarkers[ systemId ].Begin();
					}

					drawSystem.Draw( Contexts[ contextId ] );

					if ( DIAGNOSTICS_MODE ) {
						k_UnityDrawSystemMarkers[ systemId ].End();
					}
				}
			}
		}

		public void DrawGui() {
			// TODO: Do not make a copy every frame.
			foreach ( var (systemId, (system, contextId)) in _cachedRenderSystems ) {
				if ( system is IGuiSystem guiSystem ) {
					if ( DIAGNOSTICS_MODE ) {
						// _stopwatch.Reset();
						// _stopwatch.Start();
						k_UnityGuiSystemMarkers[ systemId ].Begin();
					}

					guiSystem.DrawGui( Contexts[ contextId ] );

					if ( DIAGNOSTICS_MODE ) {
						// InitializeDiagnosticsCounters();
						//
						// _stopwatch.Stop();

						// GuiCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, Contexts[contextId].Entities.Length);
						k_UnityGuiSystemMarkers[ systemId ].End();
					}
				}
			}
		}

		public void DrawGizmos() {
			foreach ( var (systemId, (system, contextId)) in _cachedRenderSystems ) {
				if ( system is IGizmosSystem guiSystem ) {
					if ( DIAGNOSTICS_MODE ) {
						// _stopwatch.Reset();
						// _stopwatch.Start();
						k_UnityGizmosSystemMarkers[ systemId ].Begin();
					}

					guiSystem.DrawGizmos( Contexts[ contextId ] );

					if ( DIAGNOSTICS_MODE ) {
						// InitializeDiagnosticsCounters();
						//
						// _stopwatch.Stop();

						// GuiCounters[systemId].Update(_stopwatch.Elapsed.TotalMicroseconds, Contexts[contextId].Entities.Length);
						k_UnityGizmosSystemMarkers[ systemId ].End();
					}
				}
			}
		}

		public void DrawImGui() {
			foreach ( var (systemId, (system, contextId)) in _cachedRenderSystems ) {
				if ( system is IImGuiSystem guiSystem ) {
					guiSystem.DrawImGui( Contexts[ contextId ] );
				}
			}
		}

#if DEBUG
		protected override void DiagnosticsBeforeNotifyReactiveSystemsCall() {
			if ( DIAGNOSTICS_MODE ) {
				k_NotifyReactiveSystemsMarker.Begin();
			}
		}

		protected override void DiagnosticsAfterNotifyReactiveSystemsCall() {
			if ( DIAGNOSTICS_MODE ) {
				k_NotifyReactiveSystemsMarker.End();
			}
		}

		protected override void DiagnosticsBeforeOnAddedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnAddedMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnAddedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnAddedMarkers[ systemId ].End();
			}
		}
		
		protected override void DiagnosticsBeforeOnRemovedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnRemovedMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnRemovedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnRemovedMarkers[ systemId ].End();
			}
		}
		
		protected override void DiagnosticsBeforeOnModifiedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnModifiedMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnModifiedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnModifiedMarkers[ systemId ].End();
			}
		}
		
		protected override void DiagnosticsBeforeOnBeforeRemovingCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnBeforeRemovingMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnBeforeRemovingCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnBeforeRemovingMarkers[ systemId ].End();
			}
		}

		protected override void DiagnosticsBeforeOnBeforeModifyingCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnBeforeModifyingMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnBeforeModifyingCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnBeforeModifyingMarkers[ systemId ].End();
			}
		}

		protected override void DiagnosticsBeforeOnMessageCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_IMessagerSystemOnMessageMarkers[ systemId ].Begin();
			}
		}

		protected override void DiagnosticsAfterOnMessageCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_IMessagerSystemOnMessageMarkers[ systemId ].End();
			}
		}

		protected override void DiagnosticsBeforeOnActivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnActivatedMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnActivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnActivatedMarkers[ systemId ].End();
			}
		}
		
		protected override void DiagnosticsBeforeOnDeactivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnDeactivatedMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnDeactivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_ReactiveSystemOnDeactivatedMarkers[ systemId ].End();
			}
		}
		
		protected override void DiagnosticsBeforeOnSystemActivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_OnSystemActivatedMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnSystemActivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_OnSystemActivatedMarkers[ systemId ].End();
			}
		}
		
		protected override void DiagnosticsBeforeOnSystemDeactivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_OnSystemDeactivatedMarkers[ systemId ].Begin();
			}
		}
		
		protected override void DiagnosticsAfterOnSystemDeactivatedCall( int systemId ) {
			if ( DIAGNOSTICS_MODE ) {
				k_OnSystemDeactivatedMarkers[ systemId ].End();
			}
		}
#endif
		
	}
	
}
