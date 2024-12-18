using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Bang.Systems;
using UnityEngine;


namespace Bang.Unity {

	public class GameScene : Scene {

		private readonly Guid _worldGuid;
		private readonly ImmutableArray<(Type system, bool isActive)> _systemsToStart;

		private UnityWorld? _world;

		public Guid WorldGuid => _worldGuid;

		public override UnityWorld? World => _world;

		internal bool IsSimulate { get; set; }

		public GameScene( ImmutableArray<(Type system, bool isActive)> systemsToStart ) {
			_worldGuid = Guid.Empty;
			_systemsToStart = systemsToStart;
		}

		public override void LoadContentImpl() {
			_world = CreateWorld();
			GC.Collect( generation: 0, mode: GCCollectionMode.Forced, blocking: true );
			Debug.Log( $"[GameScene] CreateWorld: {_world}" );
		}

		public override void ReloadImpl() {
			_world?.Dispose();
			_world = null;
		}

		public override void ResumeImpl() {
			_world?.ActivateAllSystems();
		}

		public override void SuspendImpl() {
			_world?.DeactivateAllSystems();
		}

		public virtual void Stop() {
			// if ( _world?.TryGetUniqueEntity< GodotSceneTreeComponent >() is {} entity ) {
			// 	entity.SendMessage< StopPlayGameMessage >();
			// }
		}

		public override async Task UnloadAsyncImpl() {
			// ValueTask< bool > result = Game.Data.PendingSave ?? new(true);
			ValueTask< bool > result = new ValueTask< bool >( true );
			await result;

			_world?.Dispose();
			_world = null;
		}

		/// <summary>
		/// Replace world and return the previous one, which should be disposed.
		/// </summary>
		public bool ReplaceWorld( UnityWorld world, bool disposeWorld ) {
			UnityWorld? previousWorld = _world;

			_world = world;

			if ( disposeWorld ) {
				previousWorld?.Dispose();
			}

			return true;
		}

		private UnityWorld CreateWorld() {
			// Logger.Verify( RenderContext is not null );
			
			List<(ISystem, bool)> systemInstances = new();

			// Actually instantiate and add each of our system types.
			foreach (var (type, isActive) in _systemsToStart)
			{
				if (type is null)
				{
					// Likely a debug system, skip!
					Debug.LogWarning( "found null type at GameScene::CreateWorld()!" );
					continue;
				}

				if (Activator.CreateInstance(type) is ISystem system)
				{
					systemInstances.Add((system, isActive));
				}
				else
				{
					Debug.LogError($"[GameScene] failed create system instance {type.FullName}!");
				}
			}
			
			// TODO: create from save??
			// return Game.Data.CreateWorldInstanceFromSave( _worldGuid, deepCopy: !IsSimulate );
			return new UnityWorld( systemInstances );
		}
		
	}

}
