using System;
using System.Collections.Immutable;
using UnityEngine;


namespace Bang.Unity {

	[DefaultExecutionOrder(-1000)]
	[DisallowMultipleComponent]
	public sealed class BangWorldRunner : MonoBehaviour {

		[SerializeField]
		private FeatureAsset MainFeatures;

		private GameScene _bangGameScene;

		private void Awake() {
			if ( Game.ActiveScene is not null &&
				 Game.ActiveScene.World is not null ) {
				return;
			}
			
			ImmutableArray<(Type system, bool isActive)> systemsToStart = MainFeatures.FetchAllSystems( enabled: true );
			if ( systemsToStart.IsDefaultOrEmpty ) {
				Debug.LogWarning( "[BangWorldRunner] No any system To Start!" );
			}

			_bangGameScene = new GameScene( systemsToStart );
			Game.SwitchSceneNow( _bangGameScene );
		}

		private void OnDestroy() {
			if ( _bangGameScene != null ) {
				_bangGameScene.Stop();
				_bangGameScene = null;
				Game.QueueReplaceWorldOnCurrentScene( world: null, disposeWorld: true );
				Game.QueueExitGame();
			}
		}

	}
	
}
